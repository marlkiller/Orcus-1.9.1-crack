using System.Windows;

namespace Orcus.Administration.Views.Dialogs
{
    /// <summary>
    ///     Interaction logic for InputTextWindow.xaml
    /// </summary>
    public partial class InputTextWindow
    {
        public InputTextWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Loaded -= OnLoaded;
            NameTextBox.SelectAll();
        }
    }
}