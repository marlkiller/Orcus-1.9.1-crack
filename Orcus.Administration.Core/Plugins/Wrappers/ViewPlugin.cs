using System;
using System.Windows.Media.Imaging;
using Orcus.Plugins;

namespace Orcus.Administration.Core.Plugins.Wrappers
{
    public class ViewPlugin : IViewPlugin
    {
        public ViewPlugin(string path, PluginInfo pluginInfo, BitmapImage thumbnail,
            Administration.Plugins.IViewPlugin plugin)
        {
            Path = path;
            PluginInfo = pluginInfo;
            Thumbnail = thumbnail;
            Plugin = plugin;
        }

        public Administration.Plugins.IViewPlugin Plugin { get; }
        public string Path { get; }
        public PluginInfo PluginInfo { get; }
        public BitmapImage Thumbnail { get; }
        public Type CommandView => Plugin.CommandView;
        public Type ViewType => Plugin.View;
    }
}