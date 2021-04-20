using System;

namespace Orcus.Shared.Commands.FileExplorer
{
    [Serializable]
    public class DriveDirectoryEntry : PackedDirectoryEntry
    {
        public long TotalSize { get; set; }
        public long UsedSpace { get; set; }
        public DriveDirectoryType DriveType { get; set; }
    }

    public enum DriveDirectoryType : byte
    {
        Unknown = 0,
        NoRootDirectory = 1,
        Removable = 2,
        Fixed = 3,
        Network = 4,
        CDRom = 5,
        Ram = 6
    }
}