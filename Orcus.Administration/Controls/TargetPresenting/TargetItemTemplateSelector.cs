using System.Windows;
using System.Windows.Controls;

namespace Orcus.Administration.Controls.TargetPresenting
{
    public class TargetItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate GroupTemplate { get; set; }
        public DataTemplate ClientTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is string)
                return GroupTemplate;
            return ClientTemplate;
        }
    }
}