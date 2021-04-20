using System;
using System.Collections.Generic;
using Orcus.Shared.Core;

namespace Orcus.Shared.Settings
{
    [Serializable]
    public class ConnectionBuilderProperty : IBuilderProperty
    {
        public ConnectionBuilderProperty()
        {
            IpAddresses = new List<IpAddressInfo>();
        }

        public List<IpAddressInfo> IpAddresses { get; set; }

        public IBuilderProperty Clone()
        {
            return new ConnectionBuilderProperty {IpAddresses = IpAddresses};
        }
    }
}