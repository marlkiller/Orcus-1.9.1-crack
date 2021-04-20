using System.Drawing;

namespace Orcus.Shared.Utilities.Compression
{
    public struct FrameInfo
    {
        public FrameInfo(Rectangle updatedArea, FrameFlags frameFlags)
        {
            UpdatedArea = updatedArea;
            FrameFlags = frameFlags;
        }

        public Rectangle UpdatedArea { get; set; }
        public FrameFlags FrameFlags { get; set; }
    }
}