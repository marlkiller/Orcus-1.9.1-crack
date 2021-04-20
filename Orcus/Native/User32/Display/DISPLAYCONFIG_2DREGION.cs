using System.Runtime.InteropServices;

namespace Orcus.Native.Display
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_2DREGION
    {
        public uint cx;
        public uint cy;
    }
}