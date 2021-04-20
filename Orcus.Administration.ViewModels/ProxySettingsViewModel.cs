using System;
using System.ComponentModel;
using System.Windows;
using Orcus.Administration.Core;
using Orcus.Administration.ViewModels.ViewInterface;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels
{
    public class ProxySettingsViewModel : PropertyChangedBase
    {
        private Func<string> _getPasswordFunc;
        private Action<string> _setPasswordAction;

        public ProxySettingsViewModel()
        {
            Settings = Settings.Current;
        }

        public Settings Settings { get; }

        public Action<string> SetPasswordAction
        {
            get { return _setPasswordAction; }
            set
            {
                if (SetProperty(value, ref _setPasswordAction))
                    value(Settings.ProxyPassword);
            }
        }

        public Func<string> GetPasswordFunc
        {
            get { return _getPasswordFunc; }
            set { SetProperty(value, ref _getPasswordFunc); }
        }

        public void Closing(CancelEventArgs e)
        {
            Settings.ProxyPassword = GetPasswordFunc();

            var pass = true;
            if (Settings.UseProxyToConnectToServer)
            {
                if (Settings.ProxyAuthenticate &&
                    (string.IsNullOrWhiteSpace(Settings.ProxyUsername) ||
                     string.IsNullOrWhiteSpace(Settings.ProxyPassword)))
                    pass = false;

                if (string.IsNullOrWhiteSpace(Settings.ProxyIpAddress))
                    pass = false;
            }

            if (!pass)
            {
                if (
                    WindowServiceInterface.Current.ShowMessageBox(
                        (string) Application.Current.Resources["ProxyMissingInput"],
                        (string) Application.Current.Resources["Error"], MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Settings.UseProxyToConnectToServer = false;
                    return;
                }
            }
            else
                return;

            e.Cancel = true;
            Settings.Save();
        }
    }
}