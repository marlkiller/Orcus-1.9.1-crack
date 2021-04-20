using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace Orcus.Administration.Behavior
{
    public class DropDownButtonBehavior : Behavior<Button>
    {
        public static readonly DependencyProperty PopupProperty = DependencyProperty.RegisterAttached(
            "Popup", typeof (Popup), typeof (DropDownButtonBehavior), new PropertyMetadata(default(Popup)));

        public static void SetPopup(DependencyObject element, Popup value)
        {
            element.SetValue(PopupProperty, value);
        }

        public static Popup GetPopup(DependencyObject element)
        {
            return (Popup) element.GetValue(PopupProperty);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(AssociatedObjectOnClick), true);
        }

        void AssociatedObjectOnClick(object sender, RoutedEventArgs e)
        {
            Button source = sender as Button;
            if (source == null)
                return;

            var popup = GetPopup(source);
            if (popup != null)
            {
                if (!popup.IsOpen)
                {
                    // If there is a drop-down assigned to this button, then position and display it 
                    popup.PlacementTarget = source;
                    popup.VerticalOffset = source.ActualHeight + 5;
                    popup.HorizontalOffset = source.ActualWidth + 5;
                    popup.Placement = PlacementMode.Left;
                    popup.IsOpen = true;
                }
            }
        }
        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.RemoveHandler(ButtonBase.ClickEvent, new RoutedEventHandler(AssociatedObjectOnClick));
        }
    }
}