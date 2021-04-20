using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Orcus.Shared.Connection;

namespace Orcus.Administration.Converter
{
    [ValueConversion(typeof (double), typeof (OSType))]
    public class OperatingSystemEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double) (OSType) value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (OSType) (double) value;
        }
    }

    public class OperatingSystemStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GetDescription((OSType) (double) value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private string GetDescription(object enumValue)
        {
            var descriptionAttribute = typeof(OSType)
                .GetField(enumValue.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .FirstOrDefault() as DescriptionAttribute;


            return descriptionAttribute != null
                ? descriptionAttribute.Description
                : enumValue.ToString();
        }
    }
}