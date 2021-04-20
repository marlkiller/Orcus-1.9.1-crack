using System;

namespace Orcus.Shared.Utilities.Compression
{
    [Flags]
    public enum UnsafeStreamCodecParameters
    {
        None = 0,
        UpdateImageEveryTwoSeconds = 1 << 1,
        DontDisposeImageCompressor
    }
}