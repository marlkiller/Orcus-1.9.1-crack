using System.Windows;
using System.Windows.Input;
using Orcus.Administration.ViewModels;

namespace Orcus.Administration.Views.Licensing
{
    /// <summary>
    ///     Interaction logic for RegisterOrcusWindow.xaml
    /// </summary>
    public partial class RegisterOrcusWindow
    {
        public RegisterOrcusWindow()
        {
            InitializeComponent();
            PART_CLOSE.Click += PART_CLOSE_Click;
            PART_MINIMIZE.Click += PART_MINIMIZE_Click;
            ((LicensingViewModel) DataContext).EverythingIsAwesome += RegisterOrcusWindow_EverythingIsAwesome;
            MouseLeftButtonDown += OnMouseLeftButtonDown;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            DragMove();
        }

        private void RegisterOrcusWindow_EverythingIsAwesome(object sender, System.EventArgs e)
        {
            DialogResult = true;
        }

        private void PART_MINIMIZE_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void PART_CLOSE_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}