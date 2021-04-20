using System;

namespace Orcus.Shared.DynamicCommands.TransmissionEvents
{
    /// <summary>
    ///     Transmit the command at a specific date and time
    /// </summary>
    [Serializable]
    public class DateTimeTransmissionEvent : TransmissionEvent
    {
        /// <summary>
        ///     The time as UTC
        /// </summary>
        public DateTime DateTime { get; set; }
    }
}