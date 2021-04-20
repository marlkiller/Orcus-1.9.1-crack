using System;
using System.Globalization;
using System.Windows.Data;

namespace Orcus.Administration.Converter
{
    [ValueConversion(typeof (int), typeof (bool))]
    public class IsSmallerThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int)
                return (int) value < (double) parameter;
            if (value is double)
                return (double) value < (double) parameter;

            throw new ArgumentException("Unsupported type");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}