using System.Windows;

namespace Orcus.Administration.Views.Dialogs
{
    /// <summary>
    ///     Interaction logic for ChangeSizeWindow.xaml
    /// </summary>
    public partial class ChangeSizeWindow
    {
        public ChangeSizeWindow(double currentWidth, double currentHeight)
        {
            InitializeComponent();
            NewWidth = currentWidth;
            NewHeight = currentHeight;
        }

        public double NewWidth { get; set; }
        public double NewHeight { get; set; }

        private void ChangeSizeButtonOnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}