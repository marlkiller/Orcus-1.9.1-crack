using System.Windows.Input;
using OrcusPluginStudio.Core;
using OrcusPluginStudio.Core.Settings;
using OrcusPluginStudio.ViewModels;

namespace OrcusPluginStudio.Views
{
    /// <summary>
    ///     Interaction logic for WelcomeWindow.xaml
    /// </summary>
    public partial class WelcomeWindow
    {
        public WelcomeWindow()
        {
            InitializeComponent();
            DataContext = new WelcomeViewModel(this);
        }

        public OrcusPluginProject PluginProject => ((WelcomeViewModel) DataContext).PluginProject;
        public string PluginPath => ((WelcomeViewModel) DataContext).PluginPath;

        private void RecentListBox_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (RecentListBox.SelectedItem != null)
            {
                var item = (RecentEntry) RecentListBox.SelectedItem;
                ((WelcomeViewModel) DataContext).RecentItemDoubleClicked(item);
            }
        }
    }
}