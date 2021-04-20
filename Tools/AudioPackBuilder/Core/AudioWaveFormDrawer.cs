using System;
using System.Drawing;
using System.Windows.Media.Imaging;
using AudioPackBuilder.Utilities;
using CSCore;

namespace AudioPackBuilder.Core
{
    class AudioWaveFormDrawer
    {
        public static BitmapSource DrawAudio(IWaveSource waveSource)
        {
            var bmp = new Bitmap(60, 35);

            var bytesPerSample = waveSource.WaveFormat.BitsPerSample/8*waveSource.WaveFormat.Channels;
            var samplesPerPixel = 128;
            long startPosition = 0;
            var BORDER_WIDTH = 1;
            var width = bmp.Width - 2*BORDER_WIDTH;
            var height = bmp.Height - 2*BORDER_WIDTH;

            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                var pen1 = new Pen(Color.FromArgb(204, 17, 158, 218));

                var waveData1 = new byte[samplesPerPixel*bytesPerSample];
                waveSource.Position = startPosition + width*bytesPerSample*samplesPerPixel;

                for (float x = 0; x < width; x++)
                {
                    short low = 0;
                    short high = 0;
                    var bytesRead1 = waveSource.Read(waveData1, 0, samplesPerPixel*bytesPerSample);
                    if (bytesRead1 == 0)
                        break;
                    for (var n = 0; n < bytesRead1; n += 2)
                    {
                        var sample = BitConverter.ToInt16(waveData1, n);
                        if (sample < low) low = sample;
                        if (sample > high) high = sample;
                    }
                    var lowPercent = ((float) low - short.MinValue)/ushort.MaxValue;
                    var highPercent = ((float) high - short.MinValue)/ushort.MaxValue;
                    var lowValue = height*lowPercent;
                    var highValue = height*highPercent;
                    g.DrawLine(pen1, x, lowValue, x, highValue);
                }
            }

            using (bmp)
                return BitmapConverter.ToBitmapSource(bmp);
        }
    }
}