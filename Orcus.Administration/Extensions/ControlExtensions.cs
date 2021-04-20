using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Orcus.Administration.Extensions
{
    internal static class ControlExtensions
    {
        public static readonly DependencyProperty DoubleClickCommandProperty = DependencyProperty.RegisterAttached(
            "DoubleClickCommand", typeof(ICommand), typeof(ControlExtensions),
            new PropertyMetadata(default(ICommand), DoubleClickCommandPropertyChangedCallback));

        public static readonly DependencyProperty DoubleClickCommandParameterProperty = DependencyProperty
            .RegisterAttached(
                "DoubleClickCommandParameter", typeof(object), typeof(ControlExtensions),
                new PropertyMetadata(default(object)));

        public static readonly DependencyProperty DoubleClickSetHandledProperty = DependencyProperty.RegisterAttached(
            "DoubleClickSetHandled", typeof(bool), typeof(ControlExtensions), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty IsDragOverProperty = DependencyProperty.RegisterAttached(
            "IsDragOver", typeof(bool), typeof(ControlExtensions),
            new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty TriggerIsDragOverProperty = DependencyProperty.RegisterAttached(
            "TriggerIsDragOver", typeof(bool), typeof(ControlExtensions),
            new PropertyMetadata(default(bool), TriggerIsDragOverPropertyChangedCallback));

        private static void TriggerIsDragOverPropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var control = (FrameworkElement) dependencyObject;
            control.DragEnter += ControlOnDragEnter;
            control.DragLeave += ControlOnDragLeave;
            control.Drop += ControlOnDrop;
        }

        public static void SetTriggerIsDragOver(DependencyObject element, bool value)
        {
            element.SetValue(TriggerIsDragOverProperty, value);
        }

        public static bool GetTriggerIsDragOver(DependencyObject element)
        {
            return (bool) element.GetValue(TriggerIsDragOverProperty);
        }

        private static void ControlOnDrop(object sender, DragEventArgs dragEventArgs)
        {
            SetIsDragOver((DependencyObject) sender, false);
        }

        private static void ControlOnDragLeave(object sender, DragEventArgs dragEventArgs)
        {
            SetIsDragOver((DependencyObject) sender, false);
        }

        private static void ControlOnDragEnter(object sender, DragEventArgs dragEventArgs)
        {
            SetIsDragOver((DependencyObject) sender, true);
        }

        public static void SetIsDragOver(DependencyObject element, bool value)
        {
            element.SetValue(IsDragOverProperty, value);
        }

        public static bool GetIsDragOver(DependencyObject element)
        {
            return (bool) element.GetValue(IsDragOverProperty);
        }

        public static void SetDoubleClickSetHandled(DependencyObject element, bool value)
        {
            element.SetValue(DoubleClickSetHandledProperty, value);
        }

        public static bool GetDoubleClickSetHandled(DependencyObject element)
        {
            return (bool) element.GetValue(DoubleClickSetHandledProperty);
        }

        public static void SetDoubleClickCommandParameter(DependencyObject element, object value)
        {
            element.SetValue(DoubleClickCommandParameterProperty, value);
        }

        public static object GetDoubleClickCommandParameter(DependencyObject element)
        {
            return element.GetValue(DoubleClickCommandParameterProperty);
        }

        public static void SetDoubleClickCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(DoubleClickCommandProperty, value);
        }

        public static ICommand GetDoubleClickCommand(DependencyObject element)
        {
            return (ICommand) element.GetValue(DoubleClickCommandProperty);
        }

        private static void DoubleClickCommandPropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var control = dependencyObject as Control;
            if (control == null)
                throw new ArgumentException(nameof(dependencyObject));

            if (dependencyPropertyChangedEventArgs.NewValue == null &&
                dependencyPropertyChangedEventArgs.OldValue != null)
                control.MouseDoubleClick -= Control_MouseDoubleClick;
            else
                control.MouseDoubleClick += Control_MouseDoubleClick;
        }

        private static void Control_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var control = sender as Control;
            if (control == null)
                return;

            e.Handled = GetDoubleClickSetHandled(control);
            GetDoubleClickCommand(control).Execute(GetDoubleClickCommandParameter(control));
        }
    }
}