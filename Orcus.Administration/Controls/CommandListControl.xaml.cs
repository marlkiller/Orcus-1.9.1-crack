using System.Collections;
using System.Windows;

namespace Orcus.Administration.Controls
{
    /// <summary>
    ///     Interaction logic for CommandListControl.xaml
    /// </summary>
    public partial class CommandListControl
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource", typeof (IEnumerable), typeof (CommandListControl), new PropertyMetadata(default(IEnumerable)));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            "SelectedItem", typeof (object), typeof (CommandListControl), new PropertyMetadata(default(object)));

        public CommandListControl()
        {
            InitializeComponent();
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable) GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
    }
}