using System;
using System.Windows.Media.Imaging;
using Orcus.Plugins;

namespace Orcus.Administration.ViewModels.Plugins
{
    public interface IPluginViewModel
    {
        string Name { get; }
        PluginVersion Version { get; }
        string Description { get; }
        string Author { get; }
        string AuthorUrl { get; }
        Guid Guid { get; }
        PluginType Type { get; }
        BitmapImage Thumbnail { get; }
        bool IsUpdateAvailable { get; set; }
        bool IsInstalled { get; set; }
        string ProjectWebsite { get; }
        string LocalPath { get; }
    }
}