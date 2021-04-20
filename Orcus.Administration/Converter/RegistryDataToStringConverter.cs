using System;
using System.Globalization;
using System.Windows.Data;

namespace Orcus.Administration.Converter
{
    [ValueConversion(typeof (object), typeof (string))]
    internal class RegistryDataToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is uint)
                return ((uint) value).ToString();
            if (value is ulong)
                return ((ulong) value).ToString();
            if (value is byte[])
                return BitConverter.ToString((byte[]) value).Replace("-", " ");
            if (value is string)
                return value;
            if (value is string[])
                return string.Join("\t", (string[]) value);

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}