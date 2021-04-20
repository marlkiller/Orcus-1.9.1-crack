using System;
using System.Globalization;
using System.Windows.Data;

namespace Orcus.Administration.Converter
{
    [ValueConversion(typeof (DateTime), typeof (DateTime))]
    public class DateTimeUtcToLocal : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((DateTime) value).ToLocalTime();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}