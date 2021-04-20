using System;

namespace Orcus.Shared.Commands.FileExplorer
{
    [Serializable]
    public class DownloadInformation
    {
        public DownloadInformation(long size, byte[] hash)
        {
            Size = size;
            Hash = hash;
        }

        public DownloadInformation(DownloadResult result)
        {
            Result = result;
        }

        public DownloadInformation()
        {
        }

        public long Size { get; set; }
        public byte[] Hash { get; set; }
        public DownloadResult Result { get; set; }
    }
}