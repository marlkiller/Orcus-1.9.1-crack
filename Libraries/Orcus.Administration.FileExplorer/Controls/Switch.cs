using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Orcus.Administration.FileExplorer.Controls
{
    [TemplatePart(Name = "PART_Background", Type = typeof (Grid))]
    [TemplatePart(Name = "PART_ContentOn", Type = typeof (ContentPresenter))]
    [TemplatePart(Name = "PART_ContentOff", Type = typeof (ContentPresenter))]
    public class Switch : HeaderedContentControl
    {
        public static readonly DependencyProperty IsSwitchedProperty = DependencyProperty.Register(
            "IsSwitched", typeof (bool), typeof (Switch), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty ContentOffProperty = DependencyProperty.Register(
            "ContentOff", typeof (FrameworkElement), typeof (Switch), new PropertyMetadata(default(FrameworkElement)));

        public static readonly DependencyProperty ContentOnProperty = DependencyProperty.Register(
            "ContentOn", typeof (FrameworkElement), typeof (Switch), new PropertyMetadata(default(FrameworkElement), ContentOnPropertyChangedCallback));

        public static readonly DependencyProperty IsContentVisibleProperty = DependencyProperty.RegisterAttached(
            "IsContentVisible", typeof (bool), typeof (Switch), new PropertyMetadata(default(bool)));

        public static void SetIsContentVisible(DependencyObject element, bool value)
        {
            element.SetValue(IsContentVisibleProperty, value);
        }

        public static bool GetIsContentVisible(DependencyObject element)
        {
            return (bool) element.GetValue(IsContentVisibleProperty);
        }

        private static void ContentOnPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var switchControl = (Switch) dependencyObject;
            if (dependencyPropertyChangedEventArgs.OldValue != null)
            {
                var oldFrameworkElement = (FrameworkElement) dependencyPropertyChangedEventArgs.OldValue;
                switchControl.UnsubscribeFocusEvents(oldFrameworkElement);
            }
            if (dependencyPropertyChangedEventArgs.NewValue != null)
            {
                var newFrameworkElement = (FrameworkElement) dependencyPropertyChangedEventArgs.NewValue;
                switchControl.SubsribeFocusEvents(newFrameworkElement);
            }
        }

        public bool IsSwitched
        {
            get { return (bool) GetValue(IsSwitchedProperty); }
            set
            {
                SetValue(IsSwitchedProperty, value);
                SetIsContentVisible(ContentOn, value);
                SetIsContentVisible(ContentOff, !value);
            }
        }

        public FrameworkElement ContentOff
        {
            get { return (FrameworkElement) GetValue(ContentOffProperty); }
            set { SetValue(ContentOffProperty, value); }
        }

        public FrameworkElement ContentOn
        {
            get { return (FrameworkElement) GetValue(ContentOnProperty); }
            set { SetValue(ContentOnProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var backgroundGrid = (Grid) Template.FindName("PART_Background", this);
            backgroundGrid.MouseLeftButtonDown += BackgroundGridOnMouseLeftButtonDown;
        }

        private void SubsribeFocusEvents(FrameworkElement frameworkElement)
        {
            var customLoosingControl = frameworkElement as ICustomFocusLoosingControl;
            if (customLoosingControl != null)
            {
                customLoosingControl.FocusLost += CustomLoosingControlOnFocusLost;
            }
            else
            {
                frameworkElement.LostFocus += ContentOnLostFocus;
                frameworkElement.LostKeyboardFocus += ContentOnLostKeyboardFocus;
            }
        }

        private void UnsubscribeFocusEvents(FrameworkElement frameworkElement)
        {
            var customLoosingControl = frameworkElement as ICustomFocusLoosingControl;
            if (customLoosingControl != null)
            {
                customLoosingControl.FocusLost -= CustomLoosingControlOnFocusLost;
            }
            else
            {
                frameworkElement.LostFocus -= ContentOnLostFocus;
                frameworkElement.LostKeyboardFocus -= ContentOnLostKeyboardFocus;
            }
        }

        private void CustomLoosingControlOnFocusLost(object sender, EventArgs eventArgs)
        {
            IsSwitched = false;
        }

        private void ContentOnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs keyboardFocusChangedEventArgs)
        {
            IsSwitched = false;
        }

        private void ContentOnLostFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            IsSwitched = false;
        }

        private async void BackgroundGridOnMouseLeftButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            IsSwitched = true;
            ContentOn.Focus();
            await Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
            ContentOn.Focus();
        }
    }
}