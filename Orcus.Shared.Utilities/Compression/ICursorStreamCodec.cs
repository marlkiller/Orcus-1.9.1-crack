using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Orcus.Shared.Utilities.Compression
{
    public interface ICursorStreamCodec : IDisposable
    {
        void UpdateCursorInfo(int x, int y, bool visible);
        void UpdateCursorImage(Bitmap bitmap);
        void UpdateCursorImage(IntPtr data, int stride, int width, int height, PixelFormat pixelFormat);
        bool HasCursorImage { get; }
    }
}