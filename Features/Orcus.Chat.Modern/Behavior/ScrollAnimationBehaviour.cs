using System.Windows;
using System.Windows.Controls;

namespace Orcus.Chat.Modern.Behavior
{
    static class ScrollAnimationBehaviour
    {
        public static DependencyProperty VerticalOffsetProperty =
            DependencyProperty.RegisterAttached("VerticalOffset",
                typeof (double),
                typeof (ScrollAnimationBehaviour),
                new UIPropertyMetadata(0.0, OnVerticalOffsetChanged));

        public static void SetVerticalOffset(FrameworkElement target, double value)
        {
            target.SetValue(VerticalOffsetProperty, value);
        }

        public static double GetVerticalOffset(FrameworkElement target)
        {
            return (double) target.GetValue(VerticalOffsetProperty);
        }

        private static void OnVerticalOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ScrollViewer scrollViewer = target as ScrollViewer;
            scrollViewer?.ScrollToVerticalOffset((double) e.NewValue);
        }
    }
}