using System.Windows.Media.Imaging;
using Orcus.Administration.Plugins;
using Orcus.Plugins;

namespace Orcus.Administration.Core.Plugins.Wrappers
{
    public class AdministrationPlugin : IPlugin
    {
        public AdministrationPlugin(string path, PluginInfo pluginInfo, BitmapImage thumbnail,
            IAdministrationPlugin administrationPlugin)
        {
            Path = path;
            PluginInfo = pluginInfo;
            Thumbnail = thumbnail;
            Plugin = administrationPlugin;
        }

        public IAdministrationPlugin Plugin { get; }
        public string Path { get; }
        public PluginInfo PluginInfo { get; }
        public BitmapImage Thumbnail { get; }
    }
}