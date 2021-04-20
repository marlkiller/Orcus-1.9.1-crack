using System;
using System.Globalization;
using System.Windows.Data;

namespace Orcus.Administration.Converter
{
    [ValueConversion(typeof (string[]), typeof (string))]
    internal class StringArrayToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var array = (string[]) value;
            return string.Join("\r\n", array);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((string) value).Split(new[] {"\r", "\n"}, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}