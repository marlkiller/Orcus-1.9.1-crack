using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.Extensions
{
    //Taken from https://stackoverflow.com/questions/419596/how-does-the-wpf-button-iscancel-property-work/512292#512292, a little bit modified
    /// <summary>
    ///     Attached behavior that keeps the window on the screen
    /// </summary>
    public static class WindowService
    {
        /// <summary>
        ///     KeepOnScreen Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty EscapeClosesWindowProperty = DependencyProperty.RegisterAttached(
            "EscapeClosesWindow",
            typeof (bool),
            typeof (WindowService),
            new FrameworkPropertyMetadata(false, OnEscapeClosesWindowChanged));

        public static readonly DependencyProperty ShowIconProperty = DependencyProperty.RegisterAttached(
            "ShowIcon", typeof (bool), typeof (WindowService),
            new PropertyMetadata(default(bool), ShowIconPropertyChangedCallback));

        public static readonly DependencyProperty DialogResultProperty = DependencyProperty.RegisterAttached(
            "DialogResult", typeof (bool?), typeof (WindowService),
            new PropertyMetadata(default(bool?), DialogResultPropertyChangedCallback));

        private static void DialogResultPropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var window = dependencyObject as Window;
            if (window != null)
                if (window.IsModal())
                    window.DialogResult = dependencyPropertyChangedEventArgs.NewValue as bool?;
                else
                    window.Close();
        }

        public static void SetDialogResult(DependencyObject element, bool? value)
        {
            element.SetValue(DialogResultProperty, value);
        }

        public static bool? GetDialogResult(DependencyObject element)
        {
            return (bool?) element.GetValue(DialogResultProperty);
        }

        public static bool IsModal(this Window window)
        {
            return
                (bool)
                    typeof (Window).GetField("_showingAsDialog", BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue(window);
        }

        /// <summary>
        ///     Gets the EscapeClosesWindow property.  This dependency property
        ///     indicates whether or not the escape key closes the window.
        /// </summary>
        /// <param name="d"><see cref="DependencyObject" /> to get the property from</param>
        /// <returns>The value of the EscapeClosesWindow property</returns>
        public static bool GetEscapeClosesWindow(DependencyObject d)
        {
            return (bool) d.GetValue(EscapeClosesWindowProperty);
        }

        /// <summary>
        ///     Sets the EscapeClosesWindow property.  This dependency property
        ///     indicates whether or not the escape key closes the window.
        /// </summary>
        /// <param name="d"><see cref="DependencyObject" /> to set the property on</param>
        /// <param name="value">value of the property</param>
        public static void SetEscapeClosesWindow(DependencyObject d, bool value)
        {
            d.SetValue(EscapeClosesWindowProperty, value);
        }

        /// <summary>
        ///     Handles changes to the EscapeClosesWindow property.
        /// </summary>
        /// <param name="d"><see cref="DependencyObject" /> that fired the event</param>
        /// <param name="e">A <see cref="DependencyPropertyChangedEventArgs" /> that contains the event data.</param>
        private static void OnEscapeClosesWindowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = d as Window;
            if (target != null)
            {
                if ((bool) e.NewValue)
                {
                    target.PreviewKeyDown += Window_PreviewKeyDown;
                }
                else
                {
                    target.PreviewKeyDown -= Window_PreviewKeyDown;
                }
            }
        }

        /// <summary>
        ///     Handle the PreviewKeyDown event on the window
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="KeyEventArgs" /> that contains the event data.</param>
        private static void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // If this is the escape key, close the window
            if (e.Key == Key.Escape)
                ((Window) sender).Close();
        }

        private static void ShowIconPropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if ((bool) dependencyPropertyChangedEventArgs.NewValue == false)
            {
                var window = (Window) dependencyObject;
                if (window.IsLoaded)
                    window.RemoveIcon();
                else
                {
                    window.SourceInitialized -= WindowOnSourceInitialized;
                    window.SourceInitialized += WindowOnSourceInitialized;
                }
            }
        }

        private static void WindowOnSourceInitialized(object sender, EventArgs eventArgs)
        {
            ((Window) sender).RemoveIcon();
        }

        public static void SetShowIcon(DependencyObject element, bool value)
        {
            element.SetValue(ShowIconProperty, value);
        }

        public static bool GetShowIcon(DependencyObject element)
        {
            return (bool) element.GetValue(ShowIconProperty);
        }
    }
}