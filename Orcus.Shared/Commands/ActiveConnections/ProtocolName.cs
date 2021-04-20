using System;

namespace Orcus.Shared.Commands.ActiveConnections
{
    [Serializable]
    public enum ProtocolName : byte
    {
        Tcp,
        Udp
    }
}