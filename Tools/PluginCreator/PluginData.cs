using System;
using Orcus.Plugins;

namespace PluginCreator
{
    public class PluginData
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string AuthorUrl { get; set; }
        public Guid Guid { get; set; }
        public string Version { get; set; }
        public string ThumbnailPath { get; set; }
        public PluginType PluginType { get; set; }
        public string Library1Path { get; set; }
        public string Library2Path { get; set; }
    }
}