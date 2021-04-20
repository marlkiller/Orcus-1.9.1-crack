using System;

namespace Orcus.Connection
{
    public class FileTransferEventArgs : EventArgs
    {
        public FileTransferEventArgs(Guid guid)
        {
            Guid = guid;
        }

        public Guid Guid { get; }
    }
}