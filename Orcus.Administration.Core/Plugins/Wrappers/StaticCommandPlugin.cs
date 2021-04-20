using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using Orcus.Plugins;

namespace Orcus.Administration.Core.Plugins.Wrappers
{
    public class StaticCommandPlugin : IPlugin
    {
        public StaticCommandPlugin(string path, PluginInfo pluginInfo, BitmapImage thumbnail,
            List<Type> staticCommandTypes, byte[] pluginHash)
        {
            StaticCommandTypes = staticCommandTypes;
            PluginHash = pluginHash;

            Path = path;
            PluginInfo = pluginInfo;
            Thumbnail = thumbnail;
        }

        public List<Type> StaticCommandTypes { get; }
        public byte[] PluginHash { get; set; }
        public string Path { get; }
        public PluginInfo PluginInfo { get; }
        public BitmapImage Thumbnail { get; }
    }
}