using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Orcus.Administration.ViewModels.Extensions;

namespace Orcus.Administration.Extensions
{
    public static class ToggleButtonExtensions
    {
        public static readonly DependencyProperty CheckedChangeRequestCommandProperty = DependencyProperty
            .RegisterAttached(
                "CheckedChangeRequestCommand", typeof (ICommand), typeof (ToggleButtonExtensions),
                new PropertyMetadata(default(ICommand), PropertyChangedCallback));

        public static readonly DependencyProperty CheckedChangeRequestCommandParameterProperty = DependencyProperty
            .RegisterAttached(
                "CheckedChangeRequestCommandParameter", typeof (object), typeof (ToggleButtonExtensions),
                new PropertyMetadata(default(object)));

        public static void SetCheckedChangeRequestCommandParameter(DependencyObject element, object value)
        {
            element.SetValue(CheckedChangeRequestCommandParameterProperty, value);
        }

        public static object GetCheckedChangeRequestCommandParameter(DependencyObject element)
        {
            return element.GetValue(CheckedChangeRequestCommandParameterProperty);
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var checkBox = dependencyObject as ToggleButton;
            if (checkBox == null)
                throw new ArgumentException(nameof(checkBox));

            checkBox.Checked -= CheckBoxOnChecked;
            checkBox.Unchecked -= CheckBoxOnUnchecked;

            if (dependencyPropertyChangedEventArgs.NewValue != null)
            {
                checkBox.Checked += CheckBoxOnChecked;
                checkBox.Unchecked += CheckBoxOnUnchecked;
            }
        }

        private static void CheckBoxOnUnchecked(object sender, RoutedEventArgs routedEventArgs)
        {
            CheckedChanged((ToggleButton) sender, false);
        }

        private static void CheckBoxOnChecked(object sender, RoutedEventArgs routedEventArgs)
        {
            CheckedChanged((ToggleButton) sender, true);
        }

        private static void CheckedChanged(ToggleButton toggleButton, bool isChecked)
        {
            SetIsCheckedWithoutRaisingEvent(toggleButton, !isChecked);
            var command = GetCheckedChangeRequestCommand(toggleButton);
            if (command == null)
                return;

            var parameter = GetCheckedChangeRequestCommandParameter(toggleButton);
            command.Execute(new CheckedChangeRequest
            {
                CurrentStatus = !isChecked,
                RequestedStatus = isChecked,
                Parameter = parameter,
                AcceptRequest = () => SetIsCheckedWithoutRaisingEvent(toggleButton, isChecked)
            });
        }

        private static void SetIsCheckedWithoutRaisingEvent(ToggleButton toggleButton, bool isChecked)
        {
            if (toggleButton.IsChecked == isChecked)
                return;

            toggleButton.Checked -= CheckBoxOnChecked;
            toggleButton.Unchecked -= CheckBoxOnUnchecked;
            toggleButton.IsChecked = isChecked;
            toggleButton.Checked += CheckBoxOnChecked;
            toggleButton.Unchecked += CheckBoxOnUnchecked;
        }

        public static void SetCheckedChangeRequestCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(CheckedChangeRequestCommandProperty, value);
        }

        public static ICommand GetCheckedChangeRequestCommand(DependencyObject element)
        {
            return (ICommand) element.GetValue(CheckedChangeRequestCommandProperty);
        }
    }
}