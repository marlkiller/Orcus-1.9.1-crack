using System;
using Orcus.Plugins;

namespace OrcusPluginStudio.Core
{
    public class PluginInformation
    {
        public string Description { get; set; }
        public string Author { get; set; }
        public string AuthorUrl { get; set; }
        public Guid Guid { get; set; }
        public PluginVersion Version { get; set; }
        public string Name { get; set; }
    }
}