using System;
using System.Windows;
using System.Windows.Controls;

namespace Orcus.Administration.Extensions
{
    public static class RadioButtonEnumExtensions
    {
        public static readonly DependencyProperty AvailableValuesProperty = DependencyProperty.RegisterAttached(
            "AvailableValues", typeof (Enum), typeof (RadioButtonEnumExtensions),
            new PropertyMetadata(default(Enum), AvailableValuesPropertyChangedCallback));

        public static readonly DependencyProperty EnumValueProperty = DependencyProperty.RegisterAttached(
            "EnumValue", typeof (Enum), typeof (RadioButtonEnumExtensions), new PropertyMetadata(default(Enum)));

        private static void AvailableValuesPropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var availableValues = Convert.ToInt32((Enum) dependencyPropertyChangedEventArgs.NewValue);
            var radioButton = (RadioButton) dependencyObject;
            if (!radioButton.IsLoaded)
            {
                RoutedEventHandler eventHandler = null;
                eventHandler = (sender, args) =>
                {
                    SetEnabledByAvailableValues(radioButton, availableValues);
                    radioButton.Loaded -= eventHandler;
                };
                radioButton.Loaded += eventHandler;
            }
            else
                SetEnabledByAvailableValues(radioButton, availableValues);
        }

        private static void SetEnabledByAvailableValues(RadioButton radioButton, int availableValues)
        {
            var radioButtonEnumValue = Convert.ToInt32(GetEnumValue(radioButton));
            radioButton.IsEnabled = (availableValues & radioButtonEnumValue) == radioButtonEnumValue;
        }

        public static void SetAvailableValues(DependencyObject element, Enum value)
        {
            element.SetValue(AvailableValuesProperty, value);
        }

        public static Enum GetAvailableValues(DependencyObject element)
        {
            return (Enum) element.GetValue(AvailableValuesProperty);
        }

        public static void SetEnumValue(DependencyObject element, Enum value)
        {
            element.SetValue(EnumValueProperty, value);
        }

        public static Enum GetEnumValue(DependencyObject element)
        {
            return (Enum) element.GetValue(EnumValueProperty);
        }
    }
}