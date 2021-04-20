using System;

namespace Orcus.Shared.Connection
{
    [Serializable]
    public class PluginInfo
    {
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public bool IsLoaded { get; set; }
    }
}