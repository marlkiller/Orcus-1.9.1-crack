using System;
using Orcus.Plugins;

namespace OrcusPluginStudio.Core
{
    [Serializable]
    public class OrcusPluginProject
    {
        public PluginInformation PluginInformation { get; set; }
        public string ThumbnailPath { get; set; }
        public string Library1Path { get; set; }
        public string Library2Path { get; set; }
        public PluginType PluginType { get; set; }
    }
}