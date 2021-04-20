using System;
using System.Runtime.InteropServices;

namespace Orcus.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct _SID_AND_ATTRIBUTES
    {
        public IntPtr Sid;
        public int Attributes;
    }
}