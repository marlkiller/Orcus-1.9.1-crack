using System;
using System.Globalization;
using System.Windows.Data;
using Orcus.Shared.Commands.Registry;

namespace Orcus.Administration.Converter
{
    [ValueConversion(typeof (RegistryValueKind), typeof (string))]
    internal class RegistryValueKindToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((RegistryValueKind) value)
            {
                case RegistryValueKind.String:
                    return "REG_SZ";
                case RegistryValueKind.ExpandString:
                    return "REG_EXPAND_SZ";
                case RegistryValueKind.Binary:
                    return "REG_BINARY";
                case RegistryValueKind.DWord:
                    return "REG_DWORD";
                case RegistryValueKind.MultiString:
                    return "REG_MULTI_SZ";
                case RegistryValueKind.QWord:
                    return "REG_QWORD";
                default:
                    return "Unknown";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}