using System;

namespace Orcus.Shared.Core
{
    /// <summary>
    ///     Represents an ip address/dns with a port
    /// </summary>
    [Serializable]
    public class IpAddressInfo
    {
        /// <summary>
        ///     A connection target (ip address, dns, ...)
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        ///     The port
        /// </summary>
        public int Port { get; set; }

        public override string ToString()
        {
            return $"{Ip}:{Port}";
        }
    }
}