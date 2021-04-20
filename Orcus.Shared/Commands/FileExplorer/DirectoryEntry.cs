using System;

namespace Orcus.Shared.Commands.FileExplorer
{
    [Serializable]
    public class DirectoryEntry : IFileExplorerEntry
    {
        public string Label { get; set; }
        public string Name { get; set; }
        public IFileExplorerEntry Parent { get; set; }
        public DateTime LastAccess { get; set; }
        public DateTime CreationTime { get; set; }
        public bool HasSubFolder { get; set; }
        public string Path { get; set; }

        public EntryInfo ToEntryInfo()
        {
            return new EntryInfo { Path = Path, IsDirectory = true };
        }

        /// <inheritdoc />
        public bool Equals(IFileExplorerEntry other)
        {
            var directoryEntry = other as DirectoryEntry;
            if (directoryEntry == null)
                return false;

            return string.Equals(other.Path, Path, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            return Path;
        }
    }
}