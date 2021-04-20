using Orcus.Administration.Plugins;

namespace OrcusPluginStudio.Core.Test.ManualTests
{
    public class BuildPluginTest : IManualTest
    {
        public BuildPluginTest(BuildPluginBase buildPlugin)
        {
            BuildPlugin = buildPlugin;
        }

        public BuildPluginBase BuildPlugin { get; }

        public void Dispose()
        {
        }
    }
}