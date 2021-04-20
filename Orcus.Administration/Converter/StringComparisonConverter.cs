using System;
using System.Globalization;
using System.Windows.Data;

namespace Orcus.Administration.Converter
{
    public class StringComparisonConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var value1 = values[0] as string;
            var value2 = values[1] as string;

            return value1 == value2;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}