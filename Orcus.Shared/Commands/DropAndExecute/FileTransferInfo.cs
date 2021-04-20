using System;

namespace Orcus.Shared.Commands.DropAndExecute
{
    [Serializable]
    public class FileTransferInfo
    {
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public byte[] Hash { get; set; }
        public int Length { get; set; }
    }
}