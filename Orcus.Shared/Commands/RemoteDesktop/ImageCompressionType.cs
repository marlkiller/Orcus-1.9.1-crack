using System;

namespace Orcus.Shared.Commands.RemoteDesktop
{
    [Flags]
    public enum ImageCompressionType
    {
        ManagedJpg = 1 << 0,
        TurboJpg = 1 << 1,
        NoCompression = 1 << 2
    }
}