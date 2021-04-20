using System.Drawing.Imaging;

namespace Orcus.Shared.Utilities.Compression
{
    public struct ImageInfo
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public PixelFormat PixelFormat { get; set; }
    }
}