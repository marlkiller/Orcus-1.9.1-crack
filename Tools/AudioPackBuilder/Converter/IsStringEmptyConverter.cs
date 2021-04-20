using System;
using System.Globalization;
using System.Windows.Data;

namespace AudioPackBuilder.Converter
{
    [ValueConversion(typeof (string), typeof (bool))]
    internal class IsStringEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrWhiteSpace(value?.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}