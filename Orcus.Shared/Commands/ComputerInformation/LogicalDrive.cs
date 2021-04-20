using System;

namespace Orcus.Shared.Commands.ComputerInformation
{
    [Serializable]
    public class LogicalDrive
    {
        public string VolumeLabel { get; set; }
        public string Name { get; set; }
        public string DriveFormat { get; set; }
        public string DriveType { get; set; }
        public bool IsReady { get; set; }
        public string RootDirectory { get; set; }
        public long TotalSize { get; set; }
        public long AvailableFreeSpace { get; set; }
    }
}