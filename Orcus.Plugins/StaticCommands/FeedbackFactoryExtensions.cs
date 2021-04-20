namespace Orcus.Plugins.StaticCommands
{
    /// <summary>
    ///     Some extensions for <see cref="IFeedbackFactory" />
    /// </summary>
    public static class FeedbackFactoryExtensions
    {
        /// <summary>
        ///     Send an error message
        /// </summary>
        /// <param name="feedbackFactory">The FeedbackFactory</param>
        /// <param name="message">The message which should get sent</param>
        public static void ErrorMessage(this IFeedbackFactory feedbackFactory, string message)
        {
            feedbackFactory.SendMessage(message, MessageType.Error);
        }

        /// <summary>
        ///     Send a warning message
        /// </summary>
        /// <param name="feedbackFactory">The FeedbackFactory</param>
        /// <param name="message">The message which should get sent</param>
        public static void WarningMessage(this IFeedbackFactory feedbackFactory, string message)
        {
            feedbackFactory.SendMessage(message, MessageType.Warning);
        }

        /// <summary>
        ///     Send a  message
        /// </summary>
        /// <param name="feedbackFactory">The FeedbackFactory</param>
        /// <param name="message">The message which should get sent</param>
        public static void StatusMessage(this IFeedbackFactory feedbackFactory, string message)
        {
            feedbackFactory.SendMessage(message, MessageType.Status);
        }
    }
}