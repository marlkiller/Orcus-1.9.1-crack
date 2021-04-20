using System;
using System.Globalization;
using System.Windows.Data;

namespace Sorzus.Wpf.Toolkit.Converter
{
    /// <summary>
    ///     Clone the array of the given values. Useful for CommandParameters and MultiBindings
    /// </summary>
    [ValueConversion(typeof (object), typeof (object))]
    public class CloneConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Clone();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}