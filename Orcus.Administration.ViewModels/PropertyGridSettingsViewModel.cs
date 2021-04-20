using Orcus.Plugins.PropertyGrid;

namespace Orcus.Administration.ViewModels
{
    public class PropertyGridSettingsViewModel
    {
        public PropertyGridSettingsViewModel(IProvideEditableProperties propertiesProvider)
        {
            PropertiesProvider = propertiesProvider;
        }

        public IProvideEditableProperties PropertiesProvider { get; }
    }
}