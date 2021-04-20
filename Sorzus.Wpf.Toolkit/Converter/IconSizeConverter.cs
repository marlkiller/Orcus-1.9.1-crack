using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Sorzus.Wpf.Toolkit.Converter
{
    /// <summary>
    ///     Get a specific bitmap with the given size
    /// </summary>
    [ValueConversion(typeof (ImageSource), typeof (ImageSource), ParameterType = typeof (double))]
    public class IconSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bitmapFrame = value as BitmapFrame;
            if (bitmapFrame == null)
                return value;

            var decoder = bitmapFrame.Decoder;
            var desiredSize = parameter.ToDouble();

            var result = decoder.Frames.FirstOrDefault(f => f.Width == desiredSize) ??
                         decoder.Frames.OrderBy(f => f.Width).First();

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}