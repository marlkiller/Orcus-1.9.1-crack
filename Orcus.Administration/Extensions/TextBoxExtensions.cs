using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Orcus.Administration.Extensions
{
    public class TextBoxExtensions
    {
        public static readonly DependencyProperty LinesProperty = DependencyProperty.RegisterAttached(
            "Lines", typeof (ObservableCollection<string>), typeof (TextBoxExtensions),
            new PropertyMetadata(default(ObservableCollection<string>), PropertyChangedCallback));

        public static void SetLines(DependencyObject element, ObservableCollection<string> value)
        {
            element.SetValue(LinesProperty, value);
        }

        public static ObservableCollection<string> GetLines(DependencyObject element)
        {
            return (ObservableCollection<string>) element.GetValue(LinesProperty);
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var textBox = dependencyObject as TextBox;
            if (textBox == null)
                return;

            var newValue = dependencyPropertyChangedEventArgs.NewValue as ObservableCollection<string>;
            if (newValue == null)
            {
                textBox.Text = string.Empty;
                return;
            }

            textBox.Text = string.Join("\r\n", newValue);
            newValue.CollectionChanged += (sender, args) => textBox.Text = string.Join("\r\n", newValue);
        }
    }
}