using System.Drawing;
using System.Windows;

namespace Orcus.Shared.Utilities.Compression
{
    internal static class RectangleExtensions
    {
        public static Int32Rect ToInt32Rect(this Rectangle rectangle)
        {
            return new Int32Rect(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }
    }
}