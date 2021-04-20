using System.Windows;
using Orcus.Administration.ViewModels.CommandViewModels.Registry;

namespace Orcus.Administration.Views.CommandViewWindows
{
    /// <summary>
    ///     Interaction logic for RegistryEditValueWindow.xaml
    /// </summary>
    public partial class RegistryEditValueWindow
    {
        public RegistryEditValueWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (((EditValueViewModel) DataContext).IsInCreationMode)
                KeyNameTextBox.Focus();
            else
                ValueContentControl.Focus();
        }
    }
}