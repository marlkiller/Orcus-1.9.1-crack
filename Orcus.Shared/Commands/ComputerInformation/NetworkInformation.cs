using System;
using System.Collections.Generic;

namespace Orcus.Shared.Commands.ComputerInformation
{
    [Serializable]
    public class NetworkInformation
    {
        public List<IpAddress> IpAddresses { get; set; }
        public string MacAddress { get; set; }
        public string PublicIp { get; set; }
    }
}