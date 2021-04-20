using Orcus.Administration.Core.Plugins;

namespace Orcus.Administration.ViewModels.ClientBuilder
{
    public class BuilderSettingsOverwriteRequestEventArgs
    {
        public BuilderSettingsOverwriteRequestEventArgs(IPlugin appliedPlugin, IPlugin requestingPlugin)
        {
            AppliedPlugin = appliedPlugin;
            RequestingPlugin = requestingPlugin;
        }

        public IPlugin AppliedPlugin { get; }
        public IPlugin RequestingPlugin { get; }

        public bool Overwrite { get; set; }
    }
}