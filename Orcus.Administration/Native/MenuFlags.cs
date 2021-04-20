using System;

namespace Orcus.Administration.Native
{
    [Flags]
    public enum MenuFlags : uint
    {
        MF_STRING = 0,
        MF_BYPOSITION = 0x400,
        MF_SEPARATOR = 0x800,
        MF_REMOVE = 0x1000,
        MF_POPUP = 0x00000010
    }
}