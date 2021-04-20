using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Orcus.Administration.FileExplorer.Converter
{
    public class BooleanToVisibilityCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && (bool) value == ((string) parameter != "invert"))
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (((string) parameter != "invert") && (Visibility) value != Visibility.Collapsed) ||
                   (((string) parameter == "invert") && (Visibility) value != Visibility.Visible);
        }
    }
}