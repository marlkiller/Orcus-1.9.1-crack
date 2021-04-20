using System;
using System.Collections.Generic;

namespace Orcus.Shared.Connection
{
    [Serializable]
    public class OnlineClientInformation : ClientInformation
    {
        public DateTime OnlineSince { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public int Version { get; set; }
        public double FrameworkVersion { get; set; }
        public string ClientPath { get; set; }
        public List<PluginInfo> Plugins { get; set; }
        public List<LoadablePlugin> LoadablePlugins { get; set; }
    }
}