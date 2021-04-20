using System.Windows.Media.Imaging;
using Orcus.Administration.Plugins;
using Orcus.Plugins;

namespace Orcus.Administration.Core.Plugins.Wrappers
{
    public class BuildPlugin : IPlugin
    {
        public BuildPlugin(string path, PluginInfo pluginInfo, BitmapImage thumbnail, BuildPluginBase plugin)
        {
            Path = path;
            PluginInfo = pluginInfo;
            Thumbnail = thumbnail;
            Plugin = plugin;
        }

        public BuildPluginBase Plugin { get; }
        public string Path { get; }
        public PluginInfo PluginInfo { get; }
        public BitmapImage Thumbnail { get; }
    }
}