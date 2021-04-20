using System;
using System.Globalization;
using System.Windows.Data;

namespace Orcus.Administration.Converter
{
    [ValueConversion(typeof (string), typeof (string))]
    internal class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Format((string) parameter, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}