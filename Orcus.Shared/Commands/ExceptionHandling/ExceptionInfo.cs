using System;

namespace Orcus.Shared.Commands.ExceptionHandling
{
    [Serializable]
    public class ExceptionInfo
    {
        public DateTime Timestamp { get; set; }
        public ulong TotalMemory { get; set; }
        public ulong AvailableMemory { get; set; }
        public long ProcessMemory { get; set; }
        public bool Is64BitSystem { get; set; }
        public bool Is64BitProcess { get; set; }
        public string ProcessPath { get; set; }
        public bool IsAdministrator { get; set; }
        public bool IsServiceRunning { get; set; }
        public string OsName { get; set; }
        public string RuntimeVersion { get; set; }

        public string ErrorType { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string State { get; set; }
        public int ClientVersion { get; set; }
    }
}