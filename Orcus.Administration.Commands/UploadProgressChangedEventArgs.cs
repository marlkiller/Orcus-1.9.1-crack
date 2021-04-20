using System;

namespace Orcus.Administration.Commands
{
    public class UploadProgressChangedEventArgs : EventArgs
    {
        public UploadProgressChangedEventArgs(long bytesSend, long totalBytesToSend)
        {
            BytesSend = bytesSend;
            TotalBytesToSend = totalBytesToSend;
        }

        public long BytesSend { get; }
        public long TotalBytesToSend { get; }
    }
}