using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace Orcus.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_TCPTABLE_OWNER_PID
    {
        public uint dwNumEntries;
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private MIB_TCPROW_OWNER_PID table;
    }
}