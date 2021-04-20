using System;
using System.Collections.Generic;
using System.Windows;
using Size = System.Drawing.Size;

namespace Orcus.Shared.Utilities.Compression
{
    public interface IWriteableBitmapModifierTask
    {
        void PreProcessing(IntPtr backBuffer, int stride, int pixelSize, Size size, List<Int32Rect> updatedAreas);
        void PostProcessing(IntPtr backBuffer, int stride, int pixelSize, Size size, List<Int32Rect> updatedAreas);
    }
}