using SharpDX.Mathematics.Interop;

namespace Orcus.Commands.RemoteDesktop.Capture.DesktopDuplication
{
    public static class RawRectangleExtensions
    {
        public static int GetWidth(this RawRectangle rawRectangle)
        {
            return rawRectangle.Right - rawRectangle.Left;
        }

        public static int GetHeight(this RawRectangle rawRectangle)
        {
            return rawRectangle.Bottom - rawRectangle.Top;
        }
    }
}