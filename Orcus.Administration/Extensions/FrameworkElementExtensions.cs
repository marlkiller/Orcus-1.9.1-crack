using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Orcus.Administration.Utilities;

namespace Orcus.Administration.Extensions
{
    public static class FrameworkElementExtensions
    {
        public static readonly DependencyProperty SupressBringIntoViewProperty = DependencyProperty.RegisterAttached(
            "SupressBringIntoView", typeof(bool), typeof(FrameworkElementExtensions),
            new PropertyMetadata(default(bool), PropertyChangedCallback));

        public static readonly DependencyProperty IsBringIntoViewProperty = DependencyProperty.RegisterAttached(
            "IsBringIntoView", typeof(bool), typeof(FrameworkElementExtensions),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                IsBringIntoFrontPropertyChangedCallback));

        public static readonly DependencyProperty ProgressProperty = DependencyProperty.RegisterAttached(
            "Progress", typeof(double), typeof(FrameworkElementExtensions), new PropertyMetadata(default(double)));

        public static void SetProgress(DependencyObject element, double value)
        {
            element.SetValue(ProgressProperty, value);
        }

        public static double GetProgress(DependencyObject element)
        {
            return (double) element.GetValue(ProgressProperty);
        }

        private static void IsBringIntoFrontPropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if ((bool) dependencyPropertyChangedEventArgs.NewValue)
            {
                var frameworkElement = (FrameworkElement) dependencyObject;
                frameworkElement.BringIntoView();
                SetIsBringIntoView(frameworkElement, false);
            }
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var frameworkElement = (FrameworkElement) dependencyObject;
            frameworkElement.RequestBringIntoView -= FrameworkElementOnRequestBringIntoView; //prevent memory leak

            if ((bool) dependencyPropertyChangedEventArgs.NewValue)
                frameworkElement.RequestBringIntoView += FrameworkElementOnRequestBringIntoView;
        }

        private static void FrameworkElementOnRequestBringIntoView(object sender,
            RequestBringIntoViewEventArgs requestBringIntoViewEventArgs)
        {
            requestBringIntoViewEventArgs.Handled = true;
            var itemsControl = WpfExtensions.FindParent<ItemsControl>((FrameworkElement) sender);
            if (itemsControl != null)
            {
                var scrollViewer = WpfExtensions.GetDescendantByType<ScrollViewer>(itemsControl);
                if (scrollViewer != null)
                {
                    var item = (FrameworkElement) requestBringIntoViewEventArgs.TargetObject;
                    var relativePoint = item.TranslatePoint(new Point(0, 0), itemsControl);

                    GeneralTransform childTransform = item.TransformToAncestor(scrollViewer);
                    Rect rectangle = childTransform.TransformBounds(new Rect(new Point(0, 0), item.RenderSize));
                    //Check if the elements Rect intersects with that of the scrollviewer’s
                    Rect result = Rect.Intersect(new Rect(new Point(0, 0), scrollViewer.RenderSize), rectangle);
                    var invisible = result == Rect.Empty;

                    if (invisible)
                        scrollViewer.ScrollToVerticalOffset(relativePoint.Y);
                }
            }
        }

        public static void SetIsBringIntoView(DependencyObject element, bool value)
        {
            element.SetValue(IsBringIntoViewProperty, value);
        }

        public static bool GetIsBringIntoView(DependencyObject element)
        {
            return (bool) element.GetValue(IsBringIntoViewProperty);
        }

        public static void SetSupressBringIntoView(DependencyObject element, bool value)
        {
            element.SetValue(SupressBringIntoViewProperty, value);
        }

        public static bool GetSupressBringIntoView(DependencyObject element)
        {
            return (bool) element.GetValue(SupressBringIntoViewProperty);
        }
    }
}