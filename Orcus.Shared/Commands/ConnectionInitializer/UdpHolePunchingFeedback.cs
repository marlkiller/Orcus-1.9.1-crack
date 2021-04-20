using System;
using System.Net;

namespace Orcus.Shared.Commands.ConnectionInitializer
{
    [Serializable]
    public class UdpHolePunchingFeedback
    {
        public Guid ConnectionGuid { get; set; }
        public IPEndPoint PublicEndPoint { get; set; }
    }
}