using System;
using System.Net;

namespace Orcus.Shared.Commands.ConnectionInitializer
{
    [Serializable]
    public class RemoteConnectionInformation
    {
        //Core information
        public IPAddress LocalIpAddress { get; set; }

        public ConnectionProtocol ConnectionProtocol { get; set; }
        public UdpConnectionInformation UdpConnectionInformation { get; set; }
        //public TcpConnectionInformation TcpConnectionInformation { get; set; }
    }

    [Serializable]
    public class TcpConnectionInformation
    {
        
    }

    [Serializable]
    public class UdpConnectionInformation
    {
        //Hole punching
        public bool IsHolePunchingPossible { get; set; }
    }
}