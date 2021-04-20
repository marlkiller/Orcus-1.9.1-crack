using System.Runtime.InteropServices;

namespace Orcus.Native.Display
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LUID
    {
        public uint LowPart;
        public int HighPart;
    }
}