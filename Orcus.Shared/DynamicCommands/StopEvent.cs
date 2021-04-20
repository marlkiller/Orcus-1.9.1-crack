using System;
using Orcus.Shared.DynamicCommands.StopEvents;

namespace Orcus.Shared.DynamicCommands
{
    [Serializable]
    public class StopEvent
    {
        /// <summary>
        ///     The id of the stopping event
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        ///     The parameter of the stopping event
        /// </summary>
        public byte[] Parameter { get; set; }

        /// <summary>
        /// The default stop event is the event of the <see cref="ShutdownStopEventBuilder"/>
        /// </summary>
        public static StopEvent Default = new StopEvent {Id = 3, Parameter = null};
    }

    /// <summary>
    ///     Build an stopping event
    /// </summary>
    public interface IStopEventBuilder
    {
        /// <summary>
        ///     The id of the stopping event
        /// </summary>
        uint Id { get; }

        /// <summary>
        ///     The parameter of the stopping event
        /// </summary>
        /// <returns>Returns a byte array which has to be given to the execution event at initialization</returns>
        byte[] GetParameter();
    }
}