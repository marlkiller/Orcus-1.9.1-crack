using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Orcus.Administration.Converter
{
    // ReSharper disable once InconsistentNaming
    [ValueConversion(typeof (string), typeof (BitmapImage))]
    internal class TwoLetterISOToImageConverter : IValueConverter
    {
        private static readonly Dictionary<string, BitmapImage> CachedImages = new Dictionary<string, BitmapImage>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var twoLetterIsoName = (string) value;
            if (string.IsNullOrEmpty(twoLetterIsoName) || twoLetterIsoName.Length != 2)
                return null;

            if (!CachedImages.ContainsKey(twoLetterIsoName))
                CachedImages.Add(twoLetterIsoName,
                    new BitmapImage(new Uri($"/Resources/LanguageIcons/{twoLetterIsoName}.png", UriKind.Relative)));

            return CachedImages[(string) value];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}