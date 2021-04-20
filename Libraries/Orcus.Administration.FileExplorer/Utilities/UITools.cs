using System;
using System.ComponentModel;
using System.Windows;

namespace Orcus.Administration.FileExplorer.Utilities
{
    public static class UITools
    {
        public static void AddValueChanged<T>(this T obj, DependencyProperty property, EventHandler handler)
            where T : DependencyObject
        {
            var desc = DependencyPropertyDescriptor.FromProperty(property, typeof (T));
            desc.AddValueChanged(obj, handler);
        }

        public static void RemoveValueChanged<T>(this T obj, DependencyProperty property, EventHandler handler)
            where T : DependencyObject
        {
            var desc = DependencyPropertyDescriptor.FromProperty(property, typeof (T));
            desc.RemoveValueChanged(obj, handler);
        }
    }
}