using System;
using System.Globalization;
using System.Windows.Data;

namespace Orcus.Administration.Converter
{
    [ValueConversion(typeof (double), typeof (double))]
    internal class SubtractMenuConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double) value - 31; //one for the seperator
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}