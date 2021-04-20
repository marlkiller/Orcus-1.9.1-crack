using System;
using System.Globalization;
using System.Windows.Data;

namespace Orcus.Administration.Converter
{
    [ValueConversion(typeof (int), typeof (VersionCheckResult))]
    public class ApiVersionChecker : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var version = (int) value;
            if (version == App.ClientApiVersion)
                return VersionCheckResult.Equal;
            if (version > App.ClientApiVersion)
                return VersionCheckResult.UpdateOfAdministrationRequired;

            return VersionCheckResult.UpdateRequired;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public enum VersionCheckResult
    {
        Equal,
        UpdateRequired,
        UpdateOfAdministrationRequired
    }
}