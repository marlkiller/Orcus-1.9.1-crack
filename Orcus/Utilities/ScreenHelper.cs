using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Orcus.Utilities
{
    public class ScreenHelper
    {
        public static Bitmap TakeScreenshot()
        {
            var primaryScreen = Screen.PrimaryScreen;
            var ratio = (double) primaryScreen.Bounds.Width/primaryScreen.Bounds.Height;
            var width = 300;
            var height = (int)Math.Round(width / ratio);

            var finalImage = new Bitmap(width, height);

            using (var bmpScreenshot = new Bitmap(primaryScreen.Bounds.Width,
                               primaryScreen.Bounds.Height,
                               PixelFormat.Format32bppArgb))
            {
                using (var gfxScreenshot = Graphics.FromImage(bmpScreenshot))
                {
                    gfxScreenshot.CopyFromScreen(primaryScreen.Bounds.X,
                            primaryScreen.Bounds.Y,
                            0,
                            0,
                            primaryScreen.Bounds.Size,
                            CopyPixelOperation.SourceCopy);
                }

                using (var graphics = Graphics.FromImage(finalImage))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    using (var wrapMode = new ImageAttributes())
                    {
                        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                        graphics.DrawImage(bmpScreenshot, new Rectangle(0, 0, width, height), 0, 0, bmpScreenshot.Width,
                            bmpScreenshot.Height, GraphicsUnit.Pixel, wrapMode);
                    }
                }
            }

            return finalImage;
        }
    }
}