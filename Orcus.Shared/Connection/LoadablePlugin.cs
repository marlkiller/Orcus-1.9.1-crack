using System;

namespace Orcus.Shared.Connection
{
    [Serializable]
    public class LoadablePlugin
    {
        public Guid Guid { get; set; }
        public string Version { get; set; }
    }
}