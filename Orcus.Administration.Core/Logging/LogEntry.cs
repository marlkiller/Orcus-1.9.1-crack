using System;

namespace Orcus.Administration.Core.Logging
{
    public class LogEntry
    {
        public LogEntry(LogLevel logLevel, string message)
        {
            LogLevel = logLevel;
            Message = message;
            Timestamp = DateTime.Now;
        }

        public LogLevel LogLevel { get; }
        public string Message { get; }
        public DateTime Timestamp { get; }

        public override string ToString()
        {
            return LogLevel == LogLevel.None || LogLevel == LogLevel.Logo
                ? Message
                : $"[{Timestamp.ToString("dd-MM-yyyy HH:mm:ss.ffff")}\t{LogLevel.ToString().ToUpper()}]\t\t{Message}";
        }
    }

    public enum LogLevel
    {
        Warn,
        Error,
        Fatal,
        Info,
        Send,
        Receive,
        None,
        Plugin,
        Logo
    }
}