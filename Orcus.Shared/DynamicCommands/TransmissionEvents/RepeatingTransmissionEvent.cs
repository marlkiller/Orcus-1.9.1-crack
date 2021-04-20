using System;

namespace Orcus.Shared.DynamicCommands.TransmissionEvents
{
    /// <summary>
    ///     Execute the command after each period of time
    /// </summary>
    [Serializable]
    public class RepeatingTransmissionEvent : TransmissionEvent
    {
        /// <summary>
        ///     The start of the execution
        /// </summary>
        public DateTime DayZero { get; set; }

        /// <summary>
        ///     The time period after the command should be executed again
        /// </summary>
        public TimeSpan TimeSpan
        {
            get { return TimeSpan.FromTicks(TimeSpanTicks); }
            set { TimeSpanTicks = value.Ticks; }
        }

        /// <summary>
        ///     The time period after the command should be executed again in ticks
        /// </summary>
        public long TimeSpanTicks { get; set; }
    }
}