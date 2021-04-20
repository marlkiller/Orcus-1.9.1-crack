using System;
using System.Collections.Generic;
using Orcus.Shared.Core;

namespace Orcus.Shared.Connection
{
    /// <summary>
    ///     Some basic information about the server, the first package the administration receives after a successful
    ///     connection
    /// </summary>
    [Serializable]
    public class WelcomePackage
    {
        /// <summary>
        ///     The number of exceptions
        /// </summary>
        public int ExceptionCount { get; set; }

        /// <summary>
        ///     The IP addresses the server runs on
        /// </summary>
        public List<IpAddressInfo> IpAddresses { get; set; }
    }
}