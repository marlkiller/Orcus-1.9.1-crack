using System.Windows;
using System.Windows.Controls;
using Orcus.Administration.Utilities;

namespace Orcus.Administration.Extensions
{
    public class NotNullDataTemplateSelector : DataTemplateSelector
    {
        public static readonly DependencyProperty NotNullDataTemplateProperty = DependencyProperty.RegisterAttached(
            "NotNullDataTemplate", typeof (DataTemplate), typeof (NotNullDataTemplateSelector), new PropertyMetadata(default(DataTemplate)));

        public static void SetNotNullDataTemplate(DependencyObject element, DataTemplate value)
        {
            element.SetValue(NotNullDataTemplateProperty, value);
        }

        public static DataTemplate GetNotNullDataTemplate(DependencyObject element)
        {
            return (DataTemplate) element.GetValue(NotNullDataTemplateProperty);
        }

        public static readonly DependencyProperty NullDataTemplateProperty = DependencyProperty.RegisterAttached(
            "NullDataTemplate", typeof (DataTemplate), typeof (NotNullDataTemplateSelector), new PropertyMetadata(default(DataTemplate)));

        public static void SetNullDataTemplate(DependencyObject element, DataTemplate value)
        {
            element.SetValue(NullDataTemplateProperty, value);
        }

        public static DataTemplate GetNullDataTemplate(DependencyObject element)
        {
            return (DataTemplate) element.GetValue(NullDataTemplateProperty);
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var contentControl = WpfExtensions.VisualUpwardSearch<ContentControl>(container);
            var dataTemplate = item == null ? GetNullDataTemplate(contentControl) : GetNotNullDataTemplate(contentControl);
            return dataTemplate;
        }
    }
}