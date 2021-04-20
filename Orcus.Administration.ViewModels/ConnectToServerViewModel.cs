using System;
using System.Security;
using System.Threading.Tasks;
using NLog;
using Orcus.Administration.Core;
using Orcus.Administration.ViewModels.ViewInterface;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels
{
    public class ConnectToServerViewModel : PropertyChangedBase
    {
        private RelayCommand _connectCommand;
        private ConnectionManager _connectionManager;
        private RelayCommand _createNewServerCommand;
        private string _defaultPassword;
        private bool? _dialogResult;
        private string _failMessage;
        private Func<string> _getPasswordFunc;
        private string _ipAddress;
        private bool _isFailed;
        private bool _isProxyEnabled;
        private bool _isUpdateAvailable;
        private bool _isWindowEnabled = true;
        private RelayCommand _openProxySettingsCommand;
        private SecureString _password;
        private int _port;
        private Action<string> _setPasswordAction;
        private RelayCommand _updateCommand;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ConnectToServerViewModel(string ipAddress, int port, string password)
        {
            IpAddress = ipAddress;
            Port = port;
            _defaultPassword = password;
        }

        public ConnectionManager ConnectionManager => _connectionManager;

        public bool IsUpdateAvailable
        {
            get { return _isUpdateAvailable; }
            set { SetProperty(value, ref _isUpdateAvailable); }
        }

        public string IpAddress
        {
            get { return _ipAddress; }
            set
            {
                if (SetProperty(value, ref _ipAddress))
                    IsFailed = false;
            }
        }

        public int Port
        {
            get { return _port; }
            set
            {
                if (SetProperty(value, ref _port))
                    IsFailed = false;
            }
        }

        public bool IsProxyEnabled
        {
            get { return _isProxyEnabled; }
            set { SetProperty(value, ref _isProxyEnabled); }
        }

        public bool IsWindowEnabled
        {
            get { return _isWindowEnabled; }
            set { SetProperty(value, ref _isWindowEnabled); }
        }

        public bool? DialogResult
        {
            get { return _dialogResult; }
            set { SetProperty(value, ref _dialogResult); }
        }

        public bool IsFailed
        {
            get { return _isFailed; }
            set { SetProperty(value, ref _isFailed); }
        }

        public string FailMessage
        {
            get { return _failMessage; }
            set { SetProperty(value, ref _failMessage); }
        }

        public SecureString Password
        {
            get { return _password; }
            set { SetProperty(value, ref _password); }
        }

        public Func<string> GetPasswordFunc
        {
            get { return _getPasswordFunc; }
            set { SetProperty(value, ref _getPasswordFunc); }
        }

        public Action<string> SetPasswordAction
        {
            get { return _setPasswordAction; }
            set
            {
                if (SetProperty(value, ref _setPasswordAction))
                    if (!string.IsNullOrEmpty(_defaultPassword))
                    {
                        value(_defaultPassword);
                        _defaultPassword = null;
                    }
            }
        }

        public RelayCommand UpdateCommand
        {
            get
            {
                return _updateCommand ??
                       (_updateCommand =
                           new RelayCommand(parameter => { OpenUpdateWindow?.Invoke(this, EventArgs.Empty); }));
            }
        }

        public RelayCommand CreateNewServerCommand
        {
            get
            {
                return _createNewServerCommand ??
                       (_createNewServerCommand =
                           new RelayCommand(
                               parameter =>
                               {
                                   WindowServiceInterface.Current.OpenWindowDialog(new ConfigureServerViewModel());
                               }));
            }
        }

        public RelayCommand OpenProxySettingsCommand
        {
            get
            {
                return _openProxySettingsCommand ?? (_openProxySettingsCommand = new RelayCommand(parameter =>
                {
                    WindowServiceInterface.Current.OpenWindowDialog(new ProxySettingsViewModel());
                    IsProxyEnabled = Settings.Current.UseProxyToConnectToServer;
                }));
            }
        }

        public RelayCommand ConnectCommand
        {
            get
            {
                return _connectCommand ?? (_connectCommand = new RelayCommand(async parameter =>
                {
                    IsWindowEnabled = false;
                    var password = GetPasswordFunc();

                    try
                    {
                        var result = await
                            Task.Run(
                                () =>
                                    ConnectionManager.ConnectToServer(IpAddress, Port, password, out _connectionManager));
                        if (result.IsConnected)
                        {
                            DialogResult = true;
                            return;
                        }

                        FailMessage = result.ErrorMessage;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Connecting to server");
                        FailMessage = ex.Message;
                    }

                    IsFailed = true;
                    IsWindowEnabled = true;
                }));
            }
        }

        public event EventHandler OpenUpdateWindow;
    }
}