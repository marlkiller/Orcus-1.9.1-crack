using System.Windows;
using System.Windows.Controls;

namespace Orcus.Administration.Views.CommandViews
{
    /// <summary>
    ///     Interaction logic for ReverseProxyCommandView.xaml
    /// </summary>
    public partial class ReverseProxyCommandView : UserControl
    {
        public ReverseProxyCommandView()
        {
            InitializeComponent();
        }

        private void PortNumericUpDownOnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (IsLoaded)
                ConnectionDetailsTextBox.Text = "127.0.0.1:" + e.NewValue;
        }
    }
}