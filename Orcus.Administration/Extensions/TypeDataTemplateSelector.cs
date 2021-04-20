using System;
using System.Windows;
using System.Windows.Controls;

namespace Orcus.Administration.Extensions
{
    public class TypeDataTemplateSelector : DataTemplateSelector
    {
        public Type Type { get; set; }
        public DataTemplate TypeTemplate { get; set; }
        public DataTemplate OtherTypeTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item?.GetType() == Type ? TypeTemplate : OtherTypeTemplate;
        }
    }
}