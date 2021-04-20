using System;
using System.Collections.Generic;
using System.Windows;

namespace Orcus.Administration.Extensions
{
    public static class ContextMenuExtensions
    {
        private static readonly Dictionary<string, FrameworkElement> OpenedControls =
            new Dictionary<string, FrameworkElement>();

        public static readonly DependencyProperty IsContextMenuOpenProperty = DependencyProperty.RegisterAttached(
            "IsContextMenuOpen", typeof (bool), typeof (ContextMenuExtensions), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty ListenForContextMenuGroupProperty = DependencyProperty
            .RegisterAttached(
                "ListenForContextMenuGroup", typeof (string), typeof (ContextMenuExtensions),
                new PropertyMetadata(default(string), PropertyChangedCallback));

        public static void SetListenForContextMenuGroup(DependencyObject element, string value)
        {
            element.SetValue(ListenForContextMenuGroupProperty, value);
        }

        public static string GetListenForContextMenuGroup(DependencyObject element)
        {
            return (string) element.GetValue(ListenForContextMenuGroupProperty);
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var control = dependencyObject as FrameworkElement;
            if (control == null)
                throw new ArgumentException();

            control.ContextMenuOpening += (sender, args) =>
            {
                FrameworkElement element;
                var group = GetListenForContextMenuGroup(control);
                if (OpenedControls.TryGetValue(group, out element))
                {
                    SetIsContextMenuOpen(element, false);
                    OpenedControls.Remove(group);
                }

                SetIsContextMenuOpen(control, true);
                OpenedControls.Add(group, control);
            };
            control.ContextMenuClosing += (sender, args) =>
            {
                var group = GetListenForContextMenuGroup(control);
                FrameworkElement element;
                if (OpenedControls.TryGetValue(group, out element) && element == control)
                    OpenedControls.Remove(group);

                SetIsContextMenuOpen(control, false);
            };
        }

        public static void SetIsContextMenuOpen(DependencyObject element, bool value)
        {
            element.SetValue(IsContextMenuOpenProperty, value);
        }

        public static bool GetIsContextMenuOpen(DependencyObject element)
        {
            return (bool) element.GetValue(IsContextMenuOpenProperty);
        }
    }
}