using System;

namespace Orcus.Shared.Commands.FileExplorer
{
    [Serializable]
    public class ProcessingEntryUpdate
    {
        public string Path { get; set; }
        public long Size { get; set; }
        //If value = 1 then finished, if value = -1 then error/cancelled
        public float Progress { get; set; }
    }

    [Serializable]
    public class ProcessingEntry : IFileExplorerEntry
    {
        public long Size { get; set; }
        public ProcessingEntryAction Action { get; set; }
        public float Progress { get; set; }
        public bool IsInterminate { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public IFileExplorerEntry Parent { get; set; }
        public DateTime LastAccess { get; set; }
        public DateTime CreationTime { get; set; }
        public bool IsDirectory { get; set; }

        public EntryInfo ToEntryInfo()
        {
            return new EntryInfo {Path = Path, IsDirectory = false};
        }

        public FileEntry ToFileEntry()
        {
            return new FileEntry
            {
                CreationTime = CreationTime,
                LastAccess = LastAccess,
                Name = Name,
                Path = Path,
                Size = Size
            };
        }

        public PackedDirectoryEntry ToDirectoryEntry()
        {
            return new PackedDirectoryEntry
            {
                CreationTime = CreationTime,
                LastAccess = LastAccess,
                Name = Name,
                Path = Path,
                HasSubFolder = true
            };
        }

        public IFileExplorerEntry ToFileExplorerEntry()
        {
            return IsDirectory ? (IFileExplorerEntry) ToDirectoryEntry() : ToFileEntry();
        }

        /// <inheritdoc />
        public bool Equals(IFileExplorerEntry other)
        {
            var fileEntry = other as ProcessingEntry;
            if (fileEntry == null)
                return false;

            return string.Equals(other.Path, Path, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            return Path;
        }
    }

    public enum ProcessingEntryAction
    {
        Packing,
        Extracting,
        Downloading
    }
}