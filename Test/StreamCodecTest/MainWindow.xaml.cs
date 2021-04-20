using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Orcus.Commands.RemoteDesktop.Capture.DesktopDuplication;
using Orcus.Commands.RemoteDesktop.Capture.FrontBuffer;
using Orcus.Commands.RemoteDesktop.Compression;
using Orcus.Shared.Utilities.Compression;

namespace StreamCodecTest
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _currentFps;
        private DispatcherTimer _dispatcherTimer;
        private bool _isClosed;

        public MainWindow()
        {
            InitializeComponent();
            //BenchmarkCompressionSpeed();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            _isClosed = true;
        }

        private void BenchmarkCompressionSpeed()
        {
            using (Bitmap bmpScreenCapture = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                Screen.PrimaryScreen.Bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bmpScreenCapture))
                {
                    g.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                        Screen.PrimaryScreen.Bounds.Y,
                        0, 0,
                        bmpScreenCapture.Size,
                        CopyPixelOperation.SourceCopy);
                }

                using (var jpgCompressor = new JpgCompression(70))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        var sw = Stopwatch.StartNew();
                        GC.Collect();
                        GC.WaitForPendingFinalizers();

                        using (var outStream = new MemoryStream(10000))
                        {
                            jpgCompressor.Compress(bmpScreenCapture, outStream);
                            var data = outStream.ToArray();
                            Debug.Print("Managed JPG: Time neeeded: " + sw.ElapsedMilliseconds + ", size: " + data.Length);
                        }
                    }
                }

                using (var turboJpg = new NoCompression())
                {
                    turboJpg.Quality = 70;

                    var lockBits =
                        bmpScreenCapture.LockBits(new Rectangle(0, 0, bmpScreenCapture.Width, bmpScreenCapture.Height),
                            ImageLockMode.ReadOnly, bmpScreenCapture.PixelFormat);

                    for (int i = 0; i < 5; i++)
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();

                        var sw = Stopwatch.StartNew();
                        var data = turboJpg.Compress(lockBits.Scan0, lockBits.Stride,
                            new System.Drawing.Size(lockBits.Width, lockBits.Height), lockBits.PixelFormat);

                        Debug.Print("LZF: Time neeeded: " + sw.ElapsedMilliseconds + ", size: " + data.Length);
                    }

                    bmpScreenCapture.UnlockBits(lockBits);
                }

            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            _dispatcherTimer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
            _dispatcherTimer.Tick += DispatcherTimerOnTick;
            _dispatcherTimer.Start();

            new Thread(() =>
            {
                using (var streamCodec = new UnsafeStreamCodec(new JpgCompression(70),
                    UnsafeStreamCodecParameters.None))
                using (var decoderCodec = new UnsafeStreamCodec(new JpgCompression(70),
                    UnsafeStreamCodecParameters.None))
                using (var cursorCodec = new CursorStreamCodec())
                using(var decodeCursorCodec = new CursorStreamCodec())
                using (var screenService = new FrontBufferService())
                {
                    streamCodec.ImageQuality = 70;
                    screenService.Initialize(0);
                    WriteableBitmap currentWriteableBitmap = null;

                    while (!_isClosed)
                    {
                        var sw = Stopwatch.StartNew();
                        var data = screenService.CaptureScreen(streamCodec, cursorCodec, true);
                        if (data == null)
                            continue;
                        //Debug.Print($"Screen captured ({sw.ElapsedMilliseconds})");

                        unsafe
                        {
                            byte[] bytes;
                            using (var ms = new MemoryStream())
                            {
                                data.WriteIntoStream(ms);
                                bytes = ms.ToArray();
                            }

                            var cursorData = cursorCodec.CodeCursor();

                            fixed (byte* bytePtr = bytes)
                            {
                                sw.Reset();
                                //Debug.Print("Decode " + bytes.Length);
                                var newBitmap =
                                    decoderCodec.AppendModifier(decodeCursorCodec.CreateModifierTask(cursorData, 0,
                                        cursorData.Length)).DecodeData(bytePtr, (uint) bytes.Length, Dispatcher);
                                //Debug.Print($"Screen decoded ({sw.ElapsedMilliseconds})");
                                if (newBitmap != currentWriteableBitmap)
                                {
                                    currentWriteableBitmap = newBitmap;
                                    Dispatcher.Invoke(() => ImageAsd.Source = currentWriteableBitmap);
                                }
                                Interlocked.Increment(ref _currentFps);
                            }
                        }
                    }
                }
            }).Start();
        }

        private void DispatcherTimerOnTick(object sender, EventArgs eventArgs)
        {
            Title = "FPS: " + _currentFps;
            Interlocked.Exchange(ref _currentFps, 0);
        }
    }
}