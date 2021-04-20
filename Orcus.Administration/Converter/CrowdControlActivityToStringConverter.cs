using System;
using System.Globalization;
using System.Windows.Data;
using Orcus.Administration.ViewModels;
using Orcus.Shared.DynamicCommands;

namespace Orcus.Administration.Converter
{
    [ValueConversion(typeof (ActivityType), typeof (string))]
    public class CrowdControlActivityToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return CrowdControlEventsViewModel.ActivityToString((ActivityType) value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}