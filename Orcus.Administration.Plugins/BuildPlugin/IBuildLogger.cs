namespace Orcus.Administration.Plugins.BuildPlugin
{
    /// <summary>
    ///     Used to add messages to the current build log
    /// </summary>
    public interface IBuildLogger
    {
        /// <summary>
        ///     Something was successfully finished
        /// </summary>
        /// <param name="message"></param>
        void Success(string message);

        /// <summary>
        ///     A status update about the current progress
        /// </summary>
        void Status(string message);

        /// <summary>
        ///     Something went wrong but doesn't interrupt the action
        /// </summary>
        void Warn(string message);

        /// <summary>
        ///     Something went terrible wrong and it can't continue
        /// </summary>
        /// <param name="message"></param>
        void Error(string message);
    }
}