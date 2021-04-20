using System;

namespace Orcus.Shared.Utilities.Compression
{
    [Flags]
    public enum FrameFlags
    {
        UpdatedRegion = 1 << 0,
        MovedRegion = 1 << 1
    }
}