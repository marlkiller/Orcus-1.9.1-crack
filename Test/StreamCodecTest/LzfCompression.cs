using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Orcus.Shared.Compression;
using Orcus.Shared.Utilities.Compression;

namespace StreamCodecTest
{
    public class LzfCompression : IImageCompression
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr memcpy(IntPtr dst, IntPtr src, UIntPtr count);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe IntPtr memcpy(void* dst, void* src, UIntPtr count);

        public void Dispose()
        {
        }

        public int Quality { get; set; }
        public CompressionMode CompressionMode { get; } = CompressionMode.ByteArray | CompressionMode.Stream;
        public DecompressionMode DecompressionMode { get; } = DecompressionMode.ByteArray | DecompressionMode.Pointer;

        public unsafe void Decompress(IntPtr dataPtr, uint length, IntPtr outputPtr, int outputLength,
            PixelFormat pixelFormat)
        {
            var buffer = new byte[length];
            fixed (byte* bufferPtr = buffer)
                memcpy(bufferPtr, (byte*) dataPtr, new UIntPtr(length));

            var decompressedBuffer = LZF.Decompress(buffer, 0);
            fixed (byte* decompressedBufferPtr = decompressedBuffer)
                memcpy((byte*) outputPtr, decompressedBufferPtr, (UIntPtr) decompressedBuffer.Length);
        }

        public unsafe byte[] Decompress(IntPtr dataPtr, uint length, PixelFormat pixelFormat)
        {
            var buffer = new byte[length];
            fixed (byte* bufferPtr = buffer)
                memcpy(bufferPtr, (byte*) dataPtr, new UIntPtr(length));

            return LZF.Decompress(buffer, 0);
        }

        public unsafe byte[] Compress(IntPtr scan0, int stride, Size imageSize, PixelFormat pixelFormat)
        {
            var data = new byte[stride * imageSize.Height];
            fixed (byte* dataPtr = data)
                memcpy(dataPtr, (byte*) scan0, (UIntPtr) data.Length);

            return LZF.Compress(data, 0);
        }

        public void Compress(Bitmap bitmap, Stream outStream)
        {
            throw new NotImplementedException();
        }

        public unsafe void Compress(IntPtr scan0, int stride, Size imageSize, PixelFormat pixelFormat, Stream outStream)
        {
            var data = new byte[stride * imageSize.Height];
            fixed (byte* dataPtr = data)
                memcpy(dataPtr, (byte*) scan0, (UIntPtr) data.Length);

            int compressedLength;
            var compressed = LZF.Compress(data, 0, out compressedLength);
            outStream.Write(compressed, 0, compressedLength);
        }
    }
}