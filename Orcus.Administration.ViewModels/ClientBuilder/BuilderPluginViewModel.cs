using Orcus.Administration.Core.Plugins.Wrappers;
using Orcus.Administration.Plugins;

namespace Orcus.Administration.ViewModels.ClientBuilder
{
    public class BuilderPluginViewModel : PluginViewModel
    {
        public BuilderPluginViewModel(BuildPlugin buildPlugin) : base(buildPlugin, buildPlugin.Plugin, true)
        {
            BuildPlugin = buildPlugin.Plugin;
        }

        public BuildPluginBase BuildPlugin { get; }
    }
}