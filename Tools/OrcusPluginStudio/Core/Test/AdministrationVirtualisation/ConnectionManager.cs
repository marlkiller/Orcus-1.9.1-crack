using System.Collections.Generic;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Core;

namespace OrcusPluginStudio.Core.Test.AdministrationVirtualisation
{
    public class ConnectionManager : IConnectionManager
    {
        public string Ip { get; } = "127.0.0.1";
        public int Port { get; } = 1045;

        public List<IpAddressInfo> IpAddresses { get; } = new List<IpAddressInfo>
        {
            new IpAddressInfo {Ip = "127.0.0.1", Port = 1045}
        };
    }
}