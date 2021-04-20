using System.Runtime.InteropServices;

namespace Orcus.Native.Display
{
    // See: https://msdn.microsoft.com/de-de/library/windows/desktop/dd162807(v=vs.85).aspx
    [StructLayout(LayoutKind.Sequential)]
    public struct POINTL
    {
        private int x;
        private int y;
    }
}