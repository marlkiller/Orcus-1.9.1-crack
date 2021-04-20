using System;
using System.Globalization;
using System.Windows.Data;

namespace Orcus.Administration.Converter
{
    [ValueConversion(typeof (int), typeof (string))]
    internal class FormatSecondsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var timeSpan = TimeSpan.FromSeconds(long.Parse(value.ToString()));
            return $"{timeSpan.Days}d : {timeSpan.Hours}h : {timeSpan.Minutes}m : {timeSpan.Seconds}s";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}