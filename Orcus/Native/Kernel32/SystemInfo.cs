using System;
using System.Runtime.InteropServices;

namespace Orcus.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct SYSTEM_INFO
    {
        internal int dwOemId;    // This is a union of a DWORD and a struct containing 2 WORDs.
        internal int dwPageSize;
        internal IntPtr lpMinimumApplicationAddress;
        internal IntPtr lpMaximumApplicationAddress;
        internal IntPtr dwActiveProcessorMask;
        internal int dwNumberOfProcessors;
        internal int dwProcessorType;
        internal int dwAllocationGranularity;
        internal short wProcessorLevel;
        internal short wProcessorRevision;
    }
}