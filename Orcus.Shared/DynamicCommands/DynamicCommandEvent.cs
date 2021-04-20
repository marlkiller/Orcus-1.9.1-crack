using System;

namespace Orcus.Shared.DynamicCommands
{
    /// <summary>
    ///     An event of a dynamic command
    /// </summary>
    [Serializable]
    public class DynamicCommandEvent
    {
        /// <summary>
        ///     The id of the command. Every command has it's own unique id
        /// </summary>
        public int DynamicCommand { get; set; }

        /// <summary>
        ///     The type of the activity
        /// </summary>
        public ActivityType Status { get; set; }

        /// <summary>
        ///     The id of the client
        /// </summary>
        public int ClientId { get; set; }

        /// <summary>
        ///     The time (UTC) on which the event was created
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        ///     A custom message of the event
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    ///     The type of the activity
    /// </summary>
    public enum ActivityType
    {
        /// <summary>
        ///     The command was sent
        /// </summary>
        Sent,

        /// <summary>
        ///     Succeeded was returned for the command
        /// </summary>
        Succeeded,

        /// <summary>
        ///     Failed was returned for the command
        /// </summary>
        Failed,

        /// <summary>
        ///     The command was activated
        /// </summary>
        Active,

        /// <summary>
        ///     The command was stopped
        /// </summary>
        Stopped
    }
}