using System;
using System.Globalization;
using System.Windows.Data;

namespace Sorzus.Wpf.Toolkit.Converter
{
    /// <summary>
    ///     Format the given bytes as a readable string
    /// </summary>
    [ValueConversion(typeof (long), typeof (string))]
    public class FormatBytesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return BytesToString((long) value.ToDouble());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        /// <summary>
        ///     Format the given bytes
        /// </summary>
        /// <param name="byteCount">The bytes to format</param>
        /// <returns>Return readable string</returns>
        public static string BytesToString(long byteCount)
        {
            string[] suf = {"B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB"}; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = System.Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes/Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount)*num).ToString(CultureInfo.CurrentCulture) + " " + suf[place];
        }
    }
}