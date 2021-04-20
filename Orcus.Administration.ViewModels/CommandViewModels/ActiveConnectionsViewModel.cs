using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orcus.Administration.Commands.ActiveConnections;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.ActiveConnections;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    public class ActiveConnectionsViewModel : CommandView
    {
        private List<ActiveConnection> _activeConnections;
        private RelayCommand _refreshCommand;

        public override string Name { get; } = (string) Application.Current.Resources["ActiveConnections"];
        public override Category Category { get; } = Category.Information;
        public ActiveConnectionsCommand ActiveConnectionsCommand { get; private set; }

        public List<ActiveConnection> Connections
        {
            get { return _activeConnections; }
            set { SetProperty(value, ref _activeConnections); }
        }

        public RelayCommand RefreshCommand
        {
            get
            {
                return _refreshCommand ??
                       (_refreshCommand =
                           new RelayCommand(parameter => { ActiveConnectionsCommand.GetActiveConnections(); }));
            }
        }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            ActiveConnectionsCommand = clientController.Commander.GetCommand<ActiveConnectionsCommand>();
            ActiveConnectionsCommand.ConnectionsReceived += ActiveConnectionsCommand_ConnectionsReceived;
        }

        protected override ImageSource GetIconImageSource()
        {
            return new BitmapImage(new Uri("pack://application:,,,/Resources/Images/VisualStudio/ConnectWeb_16x.png", UriKind.Absolute));
        }

        public override void LoadView(bool loadData)
        {
            if (loadData)
                RefreshCommand.Execute(null);
        }

        private void ActiveConnectionsCommand_ConnectionsReceived(object sender, List<ActiveConnection> e)
        {
            Connections = e;
        }
    }
}