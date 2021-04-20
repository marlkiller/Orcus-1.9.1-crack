namespace Orcus.Plugins.StaticCommands
{
    /// <summary>
    ///     The type of the feedback message
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        ///     Something went no as expected but it still may work
        /// </summary>
        Warning,

        /// <summary>
        ///     An error occurred
        /// </summary>
        Error,

        /// <summary>
        ///     Report current status
        /// </summary>
        Status
    }
}