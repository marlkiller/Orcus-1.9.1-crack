using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Orcus.Shared.Utilities.Compression
{
    public interface IImageCompression : IDisposable
    {
        int Quality { get; set; }
        CompressionMode CompressionMode { get; }
        DecompressionMode DecompressionMode { get; }

        void Compress(IntPtr scan0, int stride, Size imageSize, PixelFormat pixelFormat, Stream outStream);
        void Compress(Bitmap bitmap, Stream outStream);
        byte[] Compress(IntPtr scan0, int stride, Size imageSize, PixelFormat pixelFormat);

        byte[] Decompress(IntPtr dataPtr, uint length, PixelFormat pixelFormat);
        void Decompress(IntPtr dataPtr, uint length, IntPtr outputPtr, int outputLength, PixelFormat pixelFormat);
    }

    [Flags]
    public enum CompressionMode
    {
        ByteArray = 1 << 0,
        Stream = 1 << 1,
        Bitmap = 1 << 2
    }

    [Flags]
    public enum DecompressionMode
    {
        ByteArray = 1 << 0,
        Bitmap = 1 << 1,
        Pointer = 1 << 2
    }
}