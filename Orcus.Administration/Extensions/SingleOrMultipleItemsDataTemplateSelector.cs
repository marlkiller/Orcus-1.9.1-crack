using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Orcus.Administration.Extensions
{
    public class SingleOrMultipleItemsDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SingleItemDataTemplate { get; set; }
        public DataTemplate MultipleItemsDataTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null)
                return null;

            var collection = item as ICollection;
            if (collection == null)
                throw new ArgumentException("Property must implement ICollection");

            //zero items would be MultipleItemsDataTemplate
            return collection.Count == 1 ? SingleItemDataTemplate : MultipleItemsDataTemplate;
        }
    }
}