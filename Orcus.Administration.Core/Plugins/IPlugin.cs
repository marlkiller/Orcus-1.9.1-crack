using System.Windows.Media.Imaging;
using Orcus.Plugins;

namespace Orcus.Administration.Core.Plugins
{
    public interface IPlugin
    {
        string Path { get; }
        PluginInfo PluginInfo { get; }
        BitmapImage Thumbnail { get; }
    }
}