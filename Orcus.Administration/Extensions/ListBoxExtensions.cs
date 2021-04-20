using System;
using System.Windows;
using System.Windows.Controls;
using Orcus.Administration.FileExplorer.Utilities;

namespace Orcus.Administration.Extensions
{
    public static class ListBoxExtensions
    {
        public static readonly DependencyProperty IsAutomaticallyScrollingProperty = DependencyProperty.RegisterAttached
            (
                "IsAutomaticallyScrolling", typeof (bool), typeof (ListBoxExtensions),
                new PropertyMetadata(default(bool), IsAutomaticallyScrollingChanged));

        private static void IsAutomaticallyScrollingChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var listBox = (ListBox) dependencyObject;
            listBox.AddValueChanged(ItemsControl.ItemsSourceProperty, ListBoxItemsSourceChanged);
        }

        private static void ListBoxItemsSourceChanged(object sender, EventArgs eventArgs)
        {

        }

        public static void SetIsAutomaticallyScrolling(DependencyObject element, bool value)
        {
            element.SetValue(IsAutomaticallyScrollingProperty, value);
        }

        public static bool GetIsAutomaticallyScrolling(DependencyObject element)
        {
            return (bool) element.GetValue(IsAutomaticallyScrollingProperty);
        }
    }
}