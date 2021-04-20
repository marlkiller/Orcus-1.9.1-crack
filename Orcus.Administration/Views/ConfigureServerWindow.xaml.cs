using System.Windows;
using System.Windows.Controls;
using Orcus.Administration.ViewModels;

namespace Orcus.Administration.Views
{
    /// <summary>
    ///     Interaction logic for ConfigureServerWindow.xaml
    /// </summary>
    public partial class ConfigureServerWindow
    {
        public ConfigureServerWindow()
        {
            InitializeComponent();
        }

        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            ((ConfigureServerViewModel) DataContext).Password = ((PasswordBox) sender).SecurePassword;
        }

        private void Ip2LocationPasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            ((ConfigureServerViewModel) DataContext).Ip2LocationPassword = ((PasswordBox) sender).SecurePassword;
        }
    }
}