using OrcusPluginStudio.Core;
using OrcusPluginStudio.ViewModels;

namespace OrcusPluginStudio.Views
{
    /// <summary>
    ///     Interaction logic for ProjectPropertiesWindow.xaml
    /// </summary>
    public partial class ProjectPropertiesWindow
    {
        public ProjectPropertiesWindow(PluginInformation pluginInformation)
        {
            InitializeComponent();
            DataContext = new PropertiesViewModel(pluginInformation);
        }
    }
}