using Orcus.Administration.Core.Plugins;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.BuildPlugin;

namespace Orcus.Administration.Core.Build
{
    public class BuildPluginEvent
    {
        public BuildPluginEvent(BuilderEvent builderEvent, IPlugin buildPlugin)
        {
            BuilderEvent = builderEvent;
            BuildPlugin = buildPlugin;
        }

        public BuilderEvent BuilderEvent { get; }
        public IPlugin BuildPlugin { get; }
    }
}