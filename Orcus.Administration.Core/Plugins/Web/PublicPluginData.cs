using Orcus.Plugins;

namespace Orcus.Administration.Core.Plugins.Web
{
    public class PublicPluginData
    {
        public string Guid { get; set; }
        public string Version { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string AuthorUrl { get; set; }
        public string ProjectUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public PluginType PluginType { get; set; }
        public string Tags { get; set; }
        public int DownloadCount { get; set; }
    }
}