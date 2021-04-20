using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using Orcus.Administration.Plugins;

namespace Orcus.Administration.Core.Logging
{
    public static class Logger
    {
        static Logger()
        {
            Entries = new ObservableCollection<LogEntry>();
            LogService.NewLogMessage += LogService_NewLogMessage;
        }

        public static ObservableCollection<LogEntry> Entries { get; }

        public static void Warn(string message)
        {
            Log(LogLevel.Warn, message);
        }

        public static void Info(string message)
        {
            Log(LogLevel.Info, message);
        }

        public static void Fatal(string message)
        {
            Log(LogLevel.Fatal, message);
        }

        public static void Send(string message)
        {
            Log(LogLevel.Send, message);
        }

        public static void Receive(string message)
        {
            Log(LogLevel.Receive, message);
        }

        public static void Plugin(string message)
        {
            Log(LogLevel.Plugin, message);
        }

        public static void Error(string message)
        {
            Log(LogLevel.Error, message);
        }

        public static void None(string message)
        {
            Log(LogLevel.None, message);
        }

        public static void Log(LogLevel logLevel, string message)
        {
            if (Application.Current != null)
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    var entry = new LogEntry(logLevel, message);
                    Entries.Add(entry);
                    Debug.Print(entry.ToString());
                }));
        }

        private static void LogService_NewLogMessage(object sender, NewLogMesssageEventArgs e)
        {
            Log((LogLevel) (int) e.LogLevel, e.Message);
        }
    }
}