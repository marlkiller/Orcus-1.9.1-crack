using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Orcus.Shared.Utilities.Compression
{
    public interface IStreamCodec : IDisposable, IModifiedDecoder
    {
        int ImageQuality { get; set; }
        CodecOption CodecOptions { get; }
        IImageCompression ImageCompression { get; }

        RemoteDesktopDataInfo CodeImage(IntPtr scan0, Rectangle scanArea, Size imageSize, PixelFormat pixelFormat);
        RemoteDesktopDataInfo CodeImage(IntPtr scan0, Rectangle[] updatedAreas, MovedRegion[] movedRegions, Size imageSize, PixelFormat pixelFormat);
    }

    public interface IImageDecoder
    {
        unsafe WriteableBitmap DecodeData(byte* codecBuffer, uint length, Dispatcher dispatcher);
    }
}