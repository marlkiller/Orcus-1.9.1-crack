using System;

namespace Orcus.Shared.DynamicCommands.StopEvents
{
    /// <summary>
    ///     Stop the execution after a certain duration
    /// </summary>
    public class DurationStopEventBuilder : IStopEventBuilder
    {
        /// <summary>
        ///     The duration the command should execute
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        ///     The id of the DurationStopEvent is 1
        /// </summary>
        public uint Id { get; } = 1;

        /// <summary>
        ///     Get the parameter of this <see cref="StopEvent" />
        /// </summary>
        /// <returns>The parameter of this execution event which provides all neccessary options</returns>
        public byte[] GetParameter()
        {
            return BitConverter.GetBytes(Duration.Ticks);
        }
    }
}