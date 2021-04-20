using System;
using Orcus.Administration.Core.Plugins;

namespace Orcus.Administration.ViewModels.CommandViewModels.ClientPlugins
{
    public class PluginPresenter
    {
        public string Name { get; set; }
        public Guid Guid { get; set; }
        public string Version { get; set; }
        public IPlugin Plugin { get; set; }
        public bool IsUpgradeAvailable { get; set; }
        public bool IsLoaded { get; set; }
    }
}