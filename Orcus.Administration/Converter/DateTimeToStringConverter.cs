using System;
using System.Globalization;
using System.Windows.Data;
using Orcus.Administration.Core;

namespace Orcus.Administration.Converter
{
    [ValueConversion(typeof (DateTime), typeof (string))]
    public class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var dateTime = (DateTime) value;
            var format = (DateTimeFormat?) parameter ?? DateTimeFormat.GeneralLong;
            var cultureInfo = Settings.Current.Language.CultureInfo;
            switch (format)
            {
                case DateTimeFormat.LongDate:
                    return dateTime.ToString("D", cultureInfo);
                case DateTimeFormat.LongTime:
                    return dateTime.ToString("T", cultureInfo);
                case DateTimeFormat.ShortDate:
                    return dateTime.ToString("d", cultureInfo);
                case DateTimeFormat.ShortTime:
                    return dateTime.ToString("t", cultureInfo);
                case DateTimeFormat.GeneralShort:
                    return dateTime.ToString("g", cultureInfo);
                case DateTimeFormat.GeneralLong:
                    return dateTime.ToString("G", cultureInfo);
                case DateTimeFormat.FullDateLong:
                    return dateTime.ToString("F", cultureInfo);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public enum DateTimeFormat
    {
        GeneralLong,
        GeneralShort,
        LongDate,
        LongTime,
        ShortDate,
        ShortTime,
        FullDateLong
    }
}