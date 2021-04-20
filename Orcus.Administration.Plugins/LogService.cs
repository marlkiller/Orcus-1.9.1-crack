using System;

namespace Orcus.Administration.Plugins
{
    /// <summary>
    ///     Add log messages to the administration log
    /// </summary>
    public static class LogService
    {
        /// <summary>
        ///     This event is meant for the administration to react to the messages
        /// </summary>
        public static event EventHandler<NewLogMesssageEventArgs> NewLogMessage;

        /// <summary>
        ///     If something went wrong but it didn't fail
        /// </summary>
        /// <param name="message"></param>
        public static void Warn(string message)
        {
            Log(LogLevel.Warn, message);
        }

        /// <summary>
        ///     Just an information
        /// </summary>
        /// <param name="message"></param>
        public static void Info(string message)
        {
            Log(LogLevel.Info, message);
        }

        /// <summary>
        ///     If something went terrible wrong. Please only use this if it's really critical, else use <see cref="Error" />
        /// </summary>
        /// <param name="message"></param>
        public static void Fatal(string message)
        {
            Log(LogLevel.Fatal, message);
        }

        /// <summary>
        ///     A command sent to the client
        /// </summary>
        /// <param name="message"></param>
        public static void Send(string message)
        {
            Log(LogLevel.Send, message);
        }

        /// <summary>
        ///     A response received by the client
        /// </summary>
        /// <param name="message"></param>
        public static void Receive(string message)
        {
            Log(LogLevel.Receive, message);
        }

        /// <summary>
        ///     When something went wrong
        /// </summary>
        /// <param name="message"></param>
        public static void Error(string message)
        {
            Log(LogLevel.Error, message);
        }

        /// <summary>
        ///     Add a new log message
        /// </summary>
        /// <param name="logLevel">The type of the message</param>
        /// <param name="message"></param>
        public static void Log(LogLevel logLevel, string message)
        {
            NewLogMessage?.Invoke(null, new NewLogMesssageEventArgs(message, logLevel));
        }
    }

    /// <summary>
    ///     The level of the log message
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        ///     A warning: Nothing really failed, but it didn't completely succeed or just a little notice to the user
        /// </summary>
        Warn,

        /// <summary>
        ///     Something went wrong
        /// </summary>
        Error,

        /// <summary>
        ///     Something went terribly wrong. Please just use this in very rare cases
        /// </summary>
        Fatal,

        /// <summary>
        ///     Just a neutral information to the user
        /// </summary>
        Info,

        /// <summary>
        ///     Something was sent to a client
        /// </summary>
        Send,

        /// <summary>
        ///     Something was received by a client
        /// </summary>
        Receive,

        /// <summary>
        ///     No specific status
        /// </summary>
        None
    }
}