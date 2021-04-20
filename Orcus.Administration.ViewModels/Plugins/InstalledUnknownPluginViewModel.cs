using System;
using System.Windows.Media.Imaging;
using Orcus.Administration.Core.Plugins;
using Orcus.Plugins;

namespace Orcus.Administration.ViewModels.Plugins
{
    public class InstalledUnknownPluginViewModel : IPluginViewModel
    {
        internal InstalledUnknownPluginViewModel(IPlugin plugin)
        {
            Name = plugin.PluginInfo.Name;
            Version = plugin.PluginInfo.Version;
            Description = plugin.PluginInfo.Description;
            Author = plugin.PluginInfo.Author;
            AuthorUrl = plugin.PluginInfo.AuthorUrl;
            Guid = plugin.PluginInfo.Guid;
            Type = plugin.PluginInfo.PluginType;
            IsUpdateAvailable = false;
            ProjectWebsite = null;
            Thumbnail = plugin.Thumbnail;
            IsInstalled = true;
            LocalPath = plugin.Path;
        }

        public string Name { get; }
        public PluginVersion Version { get; }
        public string Description { get; }
        public string Author { get; }
        public string AuthorUrl { get; }
        public Guid Guid { get; }
        public PluginType Type { get; }
        public BitmapImage Thumbnail { get; }
        public bool IsUpdateAvailable { get; set; }
        public bool IsInstalled { get; set; }
        public string LocalPath { get; set; }
        public string ProjectWebsite { get; }
    }
}