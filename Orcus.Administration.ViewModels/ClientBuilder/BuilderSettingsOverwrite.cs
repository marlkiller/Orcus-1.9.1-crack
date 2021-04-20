using Orcus.Administration.Core.Plugins;
using Orcus.Shared.Core;

namespace Orcus.Administration.ViewModels.ClientBuilder
{
    public class BuilderSettingsOverwrite
    {
        public BuilderSettingsOverwrite(int propertyIndex, IBuilderProperty newBuilderProperty, IPlugin plugin)
        {
            PropertyIndex = propertyIndex;
            NewBuilderProperty = newBuilderProperty;
            Plugin = plugin;
        }

        public int PropertyIndex { get; }
        public IBuilderProperty NewBuilderProperty { get; }
        public IPlugin Plugin { get; }
    }
}