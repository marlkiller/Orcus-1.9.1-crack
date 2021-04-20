using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orcus.Administration.Core;
using Orcus.Administration.Core.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Administration.ViewModels.CommandViewModels.ClientPlugins;
using Orcus.Plugins;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    public class ClientPluginsViewModel : CommandView
    {
        public override string Name { get; } = (string) Application.Current.Resources["Plugins"];
        public override Category Category { get; } = Category.Client;
        public ObservableCollection<PluginPresenter> InstalledPlugins { get; set; }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            InstalledPlugins = new ObservableCollection<PluginPresenter>();

            foreach (var pluginInfo in clientController.Client.Plugins)
            {
                var foundPlugin =
                    PluginManager.Current.LoadedPlugins.FirstOrDefault(x => x.PluginInfo.Guid == pluginInfo.Guid);
                var plugin = new PluginPresenter
                {
                    Name = pluginInfo.Name,
                    Guid = pluginInfo.Guid,
                    Version = pluginInfo.Version,
                    IsLoaded = pluginInfo.IsLoaded
                };

                if (foundPlugin != null)
                {
                    plugin.Plugin = foundPlugin;
                    if (foundPlugin.PluginInfo.Version > PluginVersion.Parse(plugin.Version))
                        plugin.IsUpgradeAvailable = true;

                    if (string.IsNullOrEmpty(plugin.Name))
                        plugin.Name = foundPlugin.PluginInfo.Name;
                }

                InstalledPlugins.Add(plugin);
            }

            ((ConnectionManager) clientController.ConnectionManager).PluginLoaded += ClientPluginsViewModel_PluginLoaded;
        }

        protected override ImageSource GetIconImageSource()
        {
            return new BitmapImage(new Uri("pack://application:,,,/Resources/Images/VisualStudio/Extension.ico", UriKind.Absolute));
        }

        private void ClientPluginsViewModel_PluginLoaded(object sender, Administration.Core.Args.PluginLoadedEventArgs e)
        {
            var plugin = PluginManager.Current.LoadedPlugins.First(x => x.PluginInfo.Guid == e.Guid);
            var pluginPresenter = new PluginPresenter
            {
                Guid = e.Guid,
                IsUpgradeAvailable = false,
                Version = e.Version,
                Name = plugin.PluginInfo.Name,
                Plugin = plugin,
                IsLoaded = e.IsLoaded
            };

            Application.Current.Dispatcher.BeginInvoke(new Action(() => InstalledPlugins.Add(pluginPresenter)));
        }
    }
}