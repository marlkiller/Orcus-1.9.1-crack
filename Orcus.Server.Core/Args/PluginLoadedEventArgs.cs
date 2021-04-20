using System;
using Orcus.Shared.Connection;

namespace Orcus.Server.Core.Args
{
    public class PluginLoadedEventArgs : EventArgs
    {
        public PluginLoadedEventArgs(PluginInfo pluginInfo)
        {
            PluginInfo = pluginInfo;
        }

        public PluginInfo PluginInfo { get; }
    }
}