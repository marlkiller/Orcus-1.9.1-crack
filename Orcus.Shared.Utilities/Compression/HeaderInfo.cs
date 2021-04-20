using System.Drawing.Imaging;

namespace Orcus.Shared.Utilities.Compression
{
    internal struct HeaderInfo
    {
        public HeaderInfo(ImageMetadata imageMetadata, PixelFormat format, int width, int height)
        {
            ImageMetadata = imageMetadata;
            Format = format;
            Width = width;
            Height = height;
        }

        public ImageMetadata ImageMetadata { get; }
        public PixelFormat Format { get; }
        public int Width { get; }
        public int Height { get; }
    }
}