using System.Windows.Media.Imaging;
using Orcus.Administration.Plugins;
using Orcus.Plugins;

namespace Orcus.Administration.Core.Plugins.Wrappers
{
    public class AudioPlugin : IPlugin
    {
        public AudioPlugin(string path, PluginInfo pluginInfo, BitmapImage thumbnail, IAudioPlugin plugin)
        {
            Path = path;
            PluginInfo = pluginInfo;
            Thumbnail = thumbnail;
            Plugin = plugin;
        }

        public IAudioPlugin Plugin { get; }
        public string Path { get; }
        public PluginInfo PluginInfo { get; }
        public BitmapImage Thumbnail { get; }
    }
}