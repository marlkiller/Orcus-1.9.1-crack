using System;

namespace Orcus.Shared.DynamicCommands.StopEvents
{
    /// <summary>
    ///     Stop the execution at a specific date and time
    /// </summary>
    public class DateTimeStopEventBuilder : IStopEventBuilder
    {
        /// <summary>
        ///     The time as UTC
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        ///     The id of the DurationStopEvent is 2
        /// </summary>
        public uint Id { get; } = 2;

        /// <summary>
        ///     Get the parameter of this <see cref="StopEvent" />
        /// </summary>
        /// <returns>The parameter of this execution event which provides all neccessary options</returns>
        public byte[] GetParameter()
        {
            return BitConverter.GetBytes(DateTime.ToBinary());
        }
    }
}