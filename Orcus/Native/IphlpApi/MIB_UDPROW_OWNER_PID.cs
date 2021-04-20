using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace Orcus.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_UDPROW_OWNER_PID
    {
        public uint state;
        public uint localAddr;
        public byte localPort1;
        public byte localPort2;
        public byte localPort3;
        public byte localPort4;
        public uint remoteAddr;
        public byte remotePort1;
        public byte remotePort2;
        public byte remotePort3;
        public byte remotePort4;
        public int owningPid;

        public ushort LocalPort => BitConverter.ToUInt16(
            new byte[2] {localPort2, localPort1}, 0);

        public ushort RemotePort => BitConverter.ToUInt16(
            new byte[2] {remotePort2, remotePort1}, 0);
    }
}