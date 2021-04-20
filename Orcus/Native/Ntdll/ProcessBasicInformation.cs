using System;
using System.Runtime.InteropServices;

namespace Orcus.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ProcessBasicInformation
    {
        internal IntPtr Reserved1;
        internal IntPtr PebBaseAddress;
        internal IntPtr Reserved2_0;
        internal IntPtr Reserved2_1;
        internal IntPtr UniqueProcessId;
        internal IntPtr InheritedFromUniqueProcessId;
    }
}