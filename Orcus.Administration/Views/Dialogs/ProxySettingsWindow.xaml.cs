using System.ComponentModel;
using Orcus.Administration.ViewModels;

namespace Orcus.Administration.Views.Dialogs
{
    /// <summary>
    ///     Interaction logic for ProxySettingsWindow.xaml
    /// </summary>
    public partial class ProxySettingsWindow
    {
        public ProxySettingsWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            //apply text fields
            UseProxyCheckBox.Focus();

            ((ProxySettingsViewModel) DataContext).Closing(e);
        }
    }
}