using System;
using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming

namespace Orcus.Native
{
    internal static partial class NativeMethods
    {
        [DllImport("iphlpapi.dll", SetLastError = true)]
        internal static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int dwOutBufLen, bool sort, int ipVersion,
            TCP_TABLE_CLASS tblClass, int reserved);

        [DllImport("iphlpapi.dll", SetLastError = true)]
        internal static extern uint GetExtendedUdpTable(IntPtr pTcpTable, ref int dwOutBufLen, bool sort, int ipVersion,
            UDP_TABLE_CLASS tblClass, int reserved);
    }
}