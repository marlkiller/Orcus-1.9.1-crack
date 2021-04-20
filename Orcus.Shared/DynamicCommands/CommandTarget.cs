using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Orcus.Shared.Connection;
using Orcus.Shared.DynamicCommands.CommandTargets;

namespace Orcus.Shared.DynamicCommands
{
    /// <summary>
    ///     Define a target of a <see cref="DynamicCommand" />
    /// </summary>
    [Serializable]
    [XmlInclude(typeof (TargetedClients))]
    [XmlInclude(typeof (TargetedGroups))]
    public abstract class CommandTarget
    {
        /// <summary>
        ///     Types of all implementations of this class which are needed for serialization
        /// </summary>
        public static readonly Type[] AbstractTypes = {typeof (TargetedClients), typeof (TargetedGroups)};

        internal CommandTarget()
        {
        }

        /// <summary>
        ///     Get the command target from a list of clients
        /// </summary>
        /// <param name="clients">The targeted clients</param>
        /// <returns>A <see cref="CommandTarget" /> which represents the given clients</returns>
        public static CommandTarget FromClients(params OnlineClientInformation[] clients)
        {
            return new TargetedClients(clients);
        }

        /// <summary>
        ///     Get the command target from a list of client ids
        /// </summary>
        /// <param name="clients">The ids of the clients</param>
        /// <returns>A <see cref="CommandTarget" /> which represents the given clients</returns>
        public static CommandTarget FromClients(List<int> clients)
        {
            return new TargetedClients(clients);
        }

        /// <summary>
        ///     Get the command target from a list of clients
        /// </summary>
        /// <param name="clients">The targeted clients</param>
        /// <returns>A <see cref="CommandTarget" /> which represents the given clients</returns>
        public static CommandTarget FromClients(IEnumerable<OnlineClientInformation> clients)
        {
            return new TargetedClients(clients);
        }

        /// <summary>
        ///     Get the command target from a list of groups
        /// </summary>
        /// <param name="groups">The targeted groups</param>
        /// <returns>A <see cref="CommandTarget" /> which represents the given groups</returns>
        public static CommandTarget FromGroups(params string[] groups)
        {
            return new TargetedGroups(groups.ToList());
        }

        /// <summary>
        ///     Get the command target from a list of groups
        /// </summary>
        /// <param name="groups">The targeted groups</param>
        /// <returns>A <see cref="CommandTarget" /> which represents the given groups</returns>
        public static CommandTarget FromGroups(IEnumerable<string> groups)
        {
            return new TargetedGroups(groups.ToList());
        }
    }
}