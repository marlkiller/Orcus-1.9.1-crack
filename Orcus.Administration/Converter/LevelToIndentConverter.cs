using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Orcus.Administration.Converter
{
    internal class LevelToIndentConverter : IValueConverter
    {
        private const double c_IndentSize = 19.0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Thickness((int)value * c_IndentSize, 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}