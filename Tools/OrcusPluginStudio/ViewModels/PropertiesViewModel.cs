using OrcusPluginStudio.Core;

namespace OrcusPluginStudio.ViewModels
{
    public class PropertiesViewModel
    {
        public PropertiesViewModel(PluginInformation pluginInformation)
        {
            PluginInformation = pluginInformation;
        }

        public PluginInformation PluginInformation { get; }
    }
}