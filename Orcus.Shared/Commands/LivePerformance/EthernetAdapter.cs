using System;

namespace Orcus.Shared.Commands.LivePerformance
{
    [Serializable]
    public class EthernetAdapter
    {
        public string AdapterName { get; set; }
        public string DnsName { get; set; }
        public string ConnectionType { get; set; }
        public string Ipv4Address { get; set; }
        public string Ipv6Address { get; set; }
        public string Description { get; set; }
    }
}