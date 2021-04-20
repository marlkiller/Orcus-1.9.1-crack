using System;

namespace Orcus.Shared.Commands.FileExplorer
{
    [Serializable]
    public class FileEntry : IFileExplorerEntry
    {
        public string Name { get; set; }
        public IFileExplorerEntry Parent { get; set; }
        public DateTime LastAccess { get; set; }
        public DateTime CreationTime { get; set; }
        public long Size { get; set; }
        public string Path { get; set; }

        public EntryInfo ToEntryInfo()
        {
            return new EntryInfo {Path = Path};
        }

        /// <inheritdoc />
        public bool Equals(IFileExplorerEntry other)
        {
            var fileEntry = other as FileEntry;
            if (fileEntry == null)
                return false;

            return Path == null || other.Path == null
                ? string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase)
                : string.Equals(other.Path, Path, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            return Path;
        }
    }
}