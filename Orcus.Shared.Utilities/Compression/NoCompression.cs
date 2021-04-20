using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Orcus.Shared.Utilities.Compression
{
    public class NoCompression : IImageCompression
    {
        public void Dispose()
        {
        }

        public int Quality { get; set; }
        public CompressionMode CompressionMode { get; } = CompressionMode.Stream | CompressionMode.ByteArray;
        public DecompressionMode DecompressionMode { get; } = DecompressionMode.ByteArray | DecompressionMode.Pointer;

        public unsafe void Compress(IntPtr scan0, int stride, Size imageSize, PixelFormat pixelFormat, Stream outStream)
        {
            using (var unmanagedStream = new UnmanagedMemoryStream((byte*) scan0, stride*imageSize.Height))
                unmanagedStream.CopyToEx(outStream);
        }

        public void Compress(Bitmap bitmap, Stream outStream)
        {
            throw new NotImplementedException();
        }

        public unsafe byte[] Compress(IntPtr scan0, int stride, Size imageSize, PixelFormat pixelFormat)
        {
            var data = new byte[stride*imageSize.Height];
            fixed (byte* dataPtr = data)
                CoreMemoryApi.memcpy(dataPtr, (byte*) scan0, (UIntPtr) data.Length);
            return data;
        }

        public unsafe byte[] Decompress(IntPtr dataPtr, uint length, PixelFormat pixelFormat)
        {
            var buffer = new byte[length];
            fixed (byte* bufferPtr = buffer)
                CoreMemoryApi.memcpy(bufferPtr, (byte*) dataPtr, new UIntPtr(length));
            return buffer;
        }

        public void Decompress(IntPtr dataPtr, uint length, IntPtr outputPtr, int outputLength, PixelFormat pixelFormat)
        {
            CoreMemoryApi.memcpy(outputPtr, dataPtr, new UIntPtr(length));
        }
    }
}