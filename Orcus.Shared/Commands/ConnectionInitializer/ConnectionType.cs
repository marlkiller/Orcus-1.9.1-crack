using System;

namespace Orcus.Shared.Commands.ConnectionInitializer
{
    [Flags]
    public enum ConnectionProtocol
    {
        Udp = 1,
        Tcp = 2
    }

    [Flags]
    public enum ConnectionType
    {
        UdpLan = 1 << 2 | ConnectionProtocol.Udp,
        UdpPortforward = 1 << 3 | ConnectionProtocol.Udp,
        UdpHolePunching = 1 << 4 | ConnectionProtocol.Udp,
        Server = 1 << 5 | ConnectionProtocol.Tcp,
        TcpLan = 1 << 6 | ConnectionProtocol.Tcp
    }
}