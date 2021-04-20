using Orcus.Administration.Core.Plugins.Wrappers;

namespace Orcus.Administration.ViewModels.ClientBuilder
{
    public class ClientPluginViewModel : PluginViewModel
    {
        public ClientPluginViewModel(ClientPlugin clientPlugin) : base(clientPlugin, clientPlugin.Plugin, false)
        {
            ClientPlugin = clientPlugin;
            Size = ClientPlugin.Size;
        }

        public ClientPlugin ClientPlugin { get; }
    }

    public class FactoryPluginViewModel : PluginViewModel
    {
        public FactoryPluginViewModel(FactoryCommandPlugin factoryCommandPlugin)
            : base(factoryCommandPlugin, factoryCommandPlugin.Plugin, false)
        {
        }
    }
}