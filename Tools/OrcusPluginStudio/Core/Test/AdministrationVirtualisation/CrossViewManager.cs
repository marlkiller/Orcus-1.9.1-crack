using System;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;

namespace OrcusPluginStudio.Core.Test.AdministrationVirtualisation
{
    public class CrossViewManager : ICrossViewManager
    {
        public bool ContainsMethod(Guid methodGuid)
        {
            return true;
        }

        public void RegisterMethod<T>(ICommandView commandView, Guid methodGuid, EventHandler<T> eventHandler)
        {
        }

        public void ExecuteMethod(Guid methodGuid, object parameter)
        {
        }
    }
}