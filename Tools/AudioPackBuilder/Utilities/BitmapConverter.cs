using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using AudioPackBuilder.Native;

namespace AudioPackBuilder.Utilities
{
    class BitmapConverter
    {
        /// <summary>
        ///     Convert an IImage to a WPF BitmapSource. The result can be used in the Set Property of Image.Source
        /// </summary>
        /// <param name="image">The Emgu CV Image</param>
        /// <returns>The equivalent BitmapSource</returns>
        public static BitmapSource ToBitmapSource(Bitmap image)
        {
            var ptr = image.GetHbitmap(); //obtain the Hbitmap

            var bs = Imaging.CreateBitmapSourceFromHBitmap(
                ptr,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            NativeMethods.DeleteObject(ptr); //release the HBitmap
            bs.Freeze();
            return bs;
        }
    }
}