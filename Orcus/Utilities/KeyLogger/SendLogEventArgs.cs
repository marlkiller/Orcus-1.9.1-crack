using System;

namespace Orcus.Utilities.KeyLogger
{
    internal class SendLogEventArgs : EventArgs
    {
        public SendLogEventArgs(byte[] logData)
        {
            LogData = logData;
        }

        public bool IsHandled { get; set; }
        public byte[] LogData { get; }
    }
}