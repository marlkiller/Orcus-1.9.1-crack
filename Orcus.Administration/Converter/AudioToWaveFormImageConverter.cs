using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using CSCore.Codecs.MP3;
using Orcus.Administration.Core.Utilities;
using Orcus.Administration.Plugins.AudioPlugin;
using Color = System.Windows.Media.Color;

namespace Orcus.Administration.Converter
{
    [ValueConversion(typeof (IAudioFile), typeof (BitmapSource))]
    internal class AudioToWaveFormImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var audio = value as IAudioFile;
            if (audio == null)
                return null;

            var background = (Color) Application.Current.Resources["FlyoutColor"];
            using (var ms = new MemoryStream(audio.Data))
            using (var waveSource = new DmoMp3Decoder(ms))
            using (
                var waveForm = DrawAudio(waveSource,
                    System.Drawing.Color.FromArgb(background.A, background.R, background.G, background.B)))
                return BitmapConverter.ToBitmapSource(waveForm);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static Bitmap DrawAudio(DmoMp3Decoder decoder, System.Drawing.Color background)
        {
            var bmp = new Bitmap(60, 35);

            var bytesPerSample = decoder.WaveFormat.BitsPerSample / 8 * decoder.WaveFormat.Channels;
            var samplesPerPixel = 128;
            long startPosition = 0;
            var BORDER_WIDTH = 1;
            var width = bmp.Width - 2 * BORDER_WIDTH;
            var height = bmp.Height - 2 * BORDER_WIDTH;

            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(background);
                var color = (Color) Application.Current.Resources["DarkColor"];
                var pen1 = new Pen(System.Drawing.Color.FromArgb(204, color.R, color.G, color.B));

                var waveData1 = new byte[samplesPerPixel * bytesPerSample];
                decoder.Position = startPosition + width * bytesPerSample * samplesPerPixel;

                for (float x = 0; x < width; x++)
                {
                    short low = 0;
                    short high = 0;
                    var bytesRead1 = decoder.Read(waveData1, 0, samplesPerPixel * bytesPerSample);
                    if (bytesRead1 == 0)
                        break;
                    for (var n = 0; n < bytesRead1; n += 2)
                    {
                        var sample = BitConverter.ToInt16(waveData1, n);
                        if (sample < low) low = sample;
                        if (sample > high) high = sample;
                    }
                    var lowPercent = ((float)low - short.MinValue) / ushort.MaxValue;
                    var highPercent = ((float)high - short.MinValue) / ushort.MaxValue;
                    var lowValue = height * lowPercent;
                    var highValue = height * highPercent;
                    g.DrawLine(pen1, x, lowValue, x, highValue);
                }
            }

            return bmp;
        }
    }
}