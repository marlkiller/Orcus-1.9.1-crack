using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Orcus.Administration.Views
{
    /// <summary>
    ///     Interaction logic for StatisticsWindow.xaml
    /// </summary>
    public partial class StatisticsWindow
    {
        public StatisticsWindow()
        {
            InitializeComponent();
        }

        private void HandlePreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer && !e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                {
                    RoutedEvent = MouseWheelEvent,
                    Source = sender
                };
                var parent = (UIElement) ((Control) sender).Parent;
                parent.RaiseEvent(eventArg);
            }
        }
    }
}