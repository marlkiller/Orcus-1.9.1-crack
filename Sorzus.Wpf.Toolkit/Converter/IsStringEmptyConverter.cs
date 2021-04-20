using System;
using System.Globalization;
using System.Windows.Data;

namespace Sorzus.Wpf.Toolkit.Converter
{
    /// <summary>
    ///     Return true if the string is empty or consists only of white-space characters
    /// </summary>
    [ValueConversion(typeof (string), typeof (bool))]
    public class IsStringEmptyConverter : IValueConverter
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