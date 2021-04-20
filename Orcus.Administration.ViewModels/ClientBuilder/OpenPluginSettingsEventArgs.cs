using Orcus.Administration.Core.Plugins;

namespace Orcus.Administration.ViewModels.ClientBuilder
{
    public class OpenPluginSettingsEventArgs
    {
        public OpenPluginSettingsEventArgs(IPlugin plugin, object pluginObject)
        {
            Plugin = plugin;
            PluginObject = pluginObject;
        }

        public object PluginObject { get; }
        public IPlugin Plugin { get; }
    }
}