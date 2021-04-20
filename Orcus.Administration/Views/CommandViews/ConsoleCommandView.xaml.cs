using System.Windows;
using System.Windows.Media;

namespace Orcus.Administration.Views.CommandViews
{
    /// <summary>
    ///     Interaction logic for ConsoleCommandView.xaml
    /// </summary>
    public partial class ConsoleCommandView
    {
        public ConsoleCommandView()
        {
            InitializeComponent();
            ConsoleTextBox.Background = (Brush) Application.Current.Resources["ConsoleBackgroundBrush"];
        }
    }
}