using System;

namespace Orcus.Shared.Commands.FileExplorer
{
    [Serializable]
    public abstract class PropertiesInfo
    {
        public DateTime CreationTime { get; set; }
        public DateTime LastAccessTime { get; set; }
        public DateTime LastWriteTime { get; set; }
        public FileAttributes Attributes { get; set; }
    }

    [Flags]
    public enum FileAttributes
    {
        ReadOnly = 0x1,
        Hidden = 0x2,
        System = 0x4,
        Directory = 0x10,
        Archive = 0x20,
        Device = 0x40,
        Normal = 0x80,
        Temporary = 0x100,
        SparseFile = 0x200,
        ReparsePoint = 0x400,
        Compressed = 0x800,
        Offline = 0x1000,
        NotContentIndexed = 0x2000,
        Encrypted = 0x4000
    }
}