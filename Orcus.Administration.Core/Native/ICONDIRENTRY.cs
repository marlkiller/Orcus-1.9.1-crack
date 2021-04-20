using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace Orcus.Administration.Core.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ICONDIRENTRY
    {
        /// <summary>
        ///     The width, in pixels, of the image.
        /// </summary>
        public byte Width;

        /// <summary>
        ///     The height, in pixels, of the image.
        /// </summary>
        public byte Height;

        /// <summary>
        ///     The number of colors in the image; (0 if >= 8bpp)
        /// </summary>
        public byte ColorCount;

        /// <summary>
        ///     Reserved (must be 0).
        /// </summary>
        public byte Reserved;

        /// <summary>
        ///     Color planes.
        /// </summary>
        public ushort Planes;

        /// <summary>
        ///     Bits per pixel.
        /// </summary>
        public ushort BitCount;

        /// <summary>
        ///     The length, in bytes, of the pixel data.
        /// </summary>
        public int BytesInRes;

        /// <summary>
        ///     The offset in the file where the pixel data starts.
        /// </summary>
        public int ImageOffset;
    }
}