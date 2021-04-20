using System.Collections.Generic;
using Orcus.Shared.Core;

namespace Orcus.Shared.Server
{
    /// <summary>
    ///     The configuration file for an Orcus server
    /// </summary>
    public class ServerConfig
    {
        public ServerConfig()
        {
            IpAddresses = new List<IpAddressInfo>();
            IsAutomaticServerUpdateEnabled = true;
            ConnectionTimeout = 10000;
        }

        public List<IpAddressInfo> IpAddresses { get; set; }
        public string Password { get; set; }

        public bool IsDnsUpdaterEnabled { get; set; }
        public string DnsUpdaterSettings { get; set; }
        public string DnsUpdaterType { get; set; }

        public int ConnectionTimeout { get; set; }
        public bool IsAutomaticServerUpdateEnabled { get; set; }

        public string SslCertificatePath { get; set; }
        public string SslCertificatePassword { get; set; }

        public bool IsGeoIpLocationEnabled { get; set; }
        public string Ip2LocationEmailAddress { get; set; }
        public string Ip2LocationPassword { get; set; }
    }
}