using System;
using Orcus.Server.Core.Database.FileSystem;

namespace Orcus.Server.Core.Args
{
    public class FilePushEventArgs : EventArgs
    {
        public FilePushEventArgs(FilePushPackageType packageType, byte[] data, Guid fileTransferGuid)
        {
            PackageType = packageType;
            Data = data;
            FileTransferGuid = fileTransferGuid;
        }

        public Guid FileTransferGuid { get; }
        public FilePushPackageType PackageType { get; }
        public byte[] Data { get; }
    }
}