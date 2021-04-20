using System.Collections.Generic;
using Orcus.Shared.Core;

namespace Orcus.Administration.Plugins.CommandViewPlugin
{
    /// <summary>
    ///     The connection manager
    /// </summary>
    public interface IConnectionManager
    {
        /// <summary>
        ///     The Ip the administration connected to
        /// </summary>
        string Ip { get; }

        /// <summary>
        ///     The port the administration connected to
        /// </summary>
        int Port { get; }

        /// <summary>
        ///     The ip addresses the server runs on
        /// </summary>
        List<IpAddressInfo> IpAddresses { get; }
    }
}