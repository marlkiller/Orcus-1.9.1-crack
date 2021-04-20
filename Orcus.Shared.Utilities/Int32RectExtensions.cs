using System.Windows;

namespace Orcus.Shared.Utilities
{
    public static class Int32RectExtensions
    {
        //Source: https://referencesource.microsoft.com/#System.Drawing/commonui/System/Drawing/Rectangle.cs,366
        public static bool Contains(this Int32Rect @this, Int32Rect rect)
        {
            return (@this.X <= rect.X) &&
                   ((rect.X + rect.Width) <= (@this.X + @this.Width)) &&
                   (@this.Y <= rect.Y) &&
                   ((rect.Y + rect.Height) <= (@this.Y + @this.Height));
        }
    }
}