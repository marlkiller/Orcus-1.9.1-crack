using System;

namespace Orcus.Administration.Plugins
{
    /// <summary>
    ///     A new message was added to the log
    /// </summary>
    public class NewLogMesssageEventArgs : EventArgs
    {
        public NewLogMesssageEventArgs(string message, LogLevel logLevel)
        {
            Message = message;
            LogLevel = logLevel;
        }

        /// <summary>
        ///     The message
        /// </summary>
        public string Message { get; }

        /// <summary>
        ///     The level of this log message
        /// </summary>
        public LogLevel LogLevel { get; }
    }
}