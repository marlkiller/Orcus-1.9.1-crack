namespace Orcus.Plugins.StaticCommands
{
    /// <summary>
    ///     An interface to send feedback to the server about the execution of the command. The default result is succeeded, if
    ///     an exception was thrown failed
    /// </summary>
    public interface IFeedbackFactory
    {
        /// <summary>
        ///     The command succeeded. This will lock this object, meaning that all methods won't have any affect. This is final.
        /// </summary>
        void Succeeded();

        /// <summary>
        ///     The command failed. This will lock this object, meaning that all methods won't have any affect. This is final.
        /// </summary>
        void Failed();

        /// <summary>
        ///     The command succeeded. This will lock this object, meaning that all methods won't have any affect. This is final.
        /// </summary>
        /// <param name="message">The message which should be sent back to the server</param>
        void Succeeded(string message);

        /// <summary>
        ///     The command failed. This will lock this object, meaning that all methods won't have any affect. This is final.
        /// </summary>
        /// <param name="message">The message which should be sent back to the server</param>
        void Failed(string message);

        /// <param name="message">The content of the message</param>
        /// <param name="messageType">The type of the message</param>
        void SendMessage(string message, MessageType messageType);
    }
}