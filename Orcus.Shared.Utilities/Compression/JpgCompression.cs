using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Orcus.Shared.Utilities.Compression
{
    public class JpgCompression : IImageCompression
    {
        private readonly ImageCodecInfo _encoderInfo;
        private EncoderParameters _encoderParams;
        private int _quality;

        public JpgCompression(int quality)
        {
            _encoderInfo = GetEncoderInfo("image/jpeg");
            InitializeEncoderParameter(quality);
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        public int Quality
        {
            get { return _quality; }
            set
            {
                if (_quality != value)
                {
                    _quality = value;
                    _encoderParams.Dispose();
                    InitializeEncoderParameter(value);
                }
            }
        }

        public CompressionMode CompressionMode { get; } = CompressionMode.Stream | CompressionMode.Bitmap;
        public DecompressionMode DecompressionMode { get; } = DecompressionMode.Bitmap;

        public void Compress(IntPtr scan0, int stride, Size imageSize, PixelFormat pixelFormat, Stream outStream)
        {
            using (var tmpBmp = new Bitmap(imageSize.Width, imageSize.Height, stride, pixelFormat, scan0))
                tmpBmp.Save(outStream, _encoderInfo, _encoderParams);
        }

        public void Compress(Bitmap bitmap, Stream outStream)
        {
            bitmap.Save(outStream, _encoderInfo, _encoderParams);
        }

        public byte[] Compress(IntPtr scan0, int stride, Size imageSize, PixelFormat pixelFormat)
        {
            throw new NotImplementedException();
        }

        public byte[] Decompress(IntPtr dataPtr, uint length, PixelFormat pixelFormat)
        {
            throw new NotImplementedException();
        }

        public void Decompress(IntPtr dataPtr, uint length, IntPtr outputPtr, int outputLength, PixelFormat pixelFormat)
        {
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                _encoderParams?.Dispose();
        }

        private void InitializeEncoderParameter(int quality)
        {
            var parameter = new EncoderParameter(Encoder.Quality, quality);
            _encoderParams = new EncoderParameters(2)
            {
                Param =
                {
                    [0] = parameter,
                    [1] = new EncoderParameter(Encoder.Compression, (long) EncoderValue.CompressionRle)
                }
            };
        }

        private ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            ImageCodecInfo[] imageEncoders = ImageCodecInfo.GetImageEncoders();
            int num2 = imageEncoders.Length - 1;
            for (int i = 0; i <= num2; i++)
            {
                if (imageEncoders[i].MimeType == mimeType)
                {
                    return imageEncoders[i];
                }
            }
            return null;
        }
    }
}