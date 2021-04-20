using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using Orcus.Administration.Utilities;
using Orcus.Administration.ViewModels;

namespace Orcus.Administration.Views
{
    /// <summary>
    ///     Interaction logic for ConnectToServerWindow.xaml
    /// </summary>
    public partial class ConnectToServerWindow
    {
        public ConnectToServerWindow()
        {
            InitializeComponent();
        }

        private void TextBoxButtonOnLoaded(object sender, RoutedEventArgs e)
        {
            //workaround for a bug in MahApps.Metro
            var button = WpfExtensions.FindParent<Button>((DependencyObject)sender);
            button.ToolTip = (string)Application.Current.Resources["Proxy"];
            TextBoxHelper.SetIsClearTextButtonBehaviorEnabled(button, false);
            button.Command = ((ConnectToServerViewModel) DataContext).OpenProxySettingsCommand;
        }
    }
}