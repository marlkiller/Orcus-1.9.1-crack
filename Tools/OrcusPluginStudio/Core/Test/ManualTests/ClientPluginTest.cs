using System;
using System.Collections.Generic;
using System.Windows;
using Orcus.Plugins;
using Orcus.Shared.Commands.Password;
using OrcusPluginStudio.Core.Test.ClientVirtualisation;
using Sorzus.Wpf.Toolkit;

namespace OrcusPluginStudio.Core.Test.ManualTests
{
    public class ClientPluginTest : PropertyChangedBase, IManualTest
    {
        private readonly ClientController _clientController;
        private RelayCommand _canStartCommand;
        private RelayCommand _canTryConnectCommand;

        private RelayCommand _cookieRecoveryCommand;
        private RelayCommand _installCommand;

        private bool _isInstalled;
        private bool _isStarted;

        private RelayCommand _passwordRecoveryCommand;

        private List<RecoveredCookie> _recoveredCookies;

        private List<RecoveredPassword> _recoveredPasswords;

        private RelayCommand _shutdownCommand;
        private RelayCommand _startCommand;
        private RelayCommand _uninstallCommand;

        public ClientPluginTest(ClientController clientController)
        {
            _clientController = clientController;
        }

        public bool IsStarted
        {
            get { return _isStarted; }
            set { SetProperty(value, ref _isStarted); }
        }

        public bool IsInstalled
        {
            get { return _isInstalled; }
            set { SetProperty(value, ref _isInstalled); }
        }

        public RelayCommand StartCommand
        {
            get
            {
                return _startCommand ?? (_startCommand = new RelayCommand(parameter =>
                {
                    try
                    {
                        _clientController.Start();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    IsStarted = true;
                }));
            }
        }

        public RelayCommand ShutdownCommand
        {
            get
            {
                return _shutdownCommand ??
                       (_shutdownCommand = new RelayCommand(parameter =>
                       {
                           try
                           {
                               _clientController.Shutdown();
                           }
                           catch (Exception ex)
                           {
                               MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                               return;
                           }

                           IsStarted = false;
                       }));
            }
        }

        public RelayCommand InstallCommand
        {
            get
            {
                return _installCommand ?? (_installCommand = new RelayCommand(parameter =>
                {
                    try
                    {
                        _clientController.Install((string) parameter);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    IsInstalled = false;
                }));
            }
        }

        public RelayCommand UninstallCommand
        {
            get
            {
                return _uninstallCommand ?? (_uninstallCommand = new RelayCommand(parameter =>
                {
                    try
                    {
                        _clientController.Uninstall("");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    IsInstalled = false;
                }));
            }
        }

        public RelayCommand CanStartCommand
        {
            get
            {
                return _canStartCommand ?? (_canStartCommand = new RelayCommand(parameter =>
                {
                    bool result;
                    try
                    {
                        result = _clientController.InfluenceStartup(new ClientStartupInformation());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    MessageBox.Show($"ClientController.InfluenceStartup(IClientStartup clientStartup) returned {result}");
                }));
            }
        }

        public RelayCommand CanTryConnectCommand
        {
            get
            {
                return _canTryConnectCommand ?? (_canTryConnectCommand = new RelayCommand(parameter =>
                {
                    bool result;
                    try
                    {
                        result = _clientController.CanTryConnect();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    MessageBox.Show($"ClientController.CanTryConnect() returned {result}");
                }));
            }
        }

        public List<RecoveredPassword> RecoveredPasswords
        {
            get { return _recoveredPasswords; }
            set { SetProperty(value, ref _recoveredPasswords); }
        }

        public List<RecoveredCookie> RecoveredCookies
        {
            get { return _recoveredCookies; }
            set { SetProperty(value, ref _recoveredCookies); }
        }

        public RelayCommand PasswordRecoveryCommand
        {
            get
            {
                return _passwordRecoveryCommand ?? (_passwordRecoveryCommand = new RelayCommand(parameter =>
                {
                    try
                    {
                        RecoveredPasswords = _clientController.RecoverPasswords();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }));
            }
        }

        public RelayCommand CookieRecoveryCommand
        {
            get
            {
                return _cookieRecoveryCommand ?? (_cookieRecoveryCommand = new RelayCommand(parameter =>
                {
                    try
                    {
                        RecoveredCookies = _clientController.RecoverCookies();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }));
            }
        }

        public void Dispose()
        {
            if (IsStarted)
                _clientController.Shutdown();
        }
    }
}