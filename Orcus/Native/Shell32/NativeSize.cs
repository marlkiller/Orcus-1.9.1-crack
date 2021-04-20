using System.Runtime.InteropServices;

namespace Orcus.Native.Shell
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeSize
    {
        private int width;
        private int height;

        public int Width
        {
            set { width = value; }
        }

        public int Height
        {
            set { height = value; }
        }
    }
}