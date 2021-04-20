using System;

namespace Orcus.Shared.Utilities.Compression
{
    [Flags]
    public enum ImageMetadata
    {
        Frames = 1 << 0,
        FullImage = 1 << 1,
        IncludeCursorImage = 1 << 2,
        CursorPosition = 1 << 3
    }
}