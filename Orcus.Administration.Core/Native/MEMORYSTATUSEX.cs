using System.Runtime.InteropServices;

namespace Orcus.Administration.Core.Native
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullAvailExtendedVirtual;
        public ulong ullAvailPageFile;
        public ulong ullAvailPhys;
        public ulong ullAvailVirtual;
        public ulong ullTotalPageFile;
        public ulong ullTotalPhys;
        public ulong ullTotalVirtual;

        public MEMORYSTATUSEX()
        {
            dwLength = (uint) Marshal.SizeOf(typeof (MEMORYSTATUSEX));
        }
    }
}