using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OrcusPluginStudio.Views.ManualTest
{
    /// <summary>
    ///     Interaction logic for ClientPluginTest.xaml
    /// </summary>
    public partial class ClientPluginTest : UserControl
    {
        public ClientPluginTest()
        {
            InitializeComponent();
        }

        private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            var e2 = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) {RoutedEvent = MouseWheelEvent};
            ((FrameworkElement) sender).RaiseEvent(e2);
        }
    }
}