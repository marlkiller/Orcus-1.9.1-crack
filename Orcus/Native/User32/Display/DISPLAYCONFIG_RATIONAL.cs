using System.Runtime.InteropServices;

namespace Orcus.Native.Display
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_RATIONAL
    {
        public uint Numerator;
        public uint Denominator;
    }
}