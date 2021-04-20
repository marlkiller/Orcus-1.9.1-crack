using System;
using System.Collections.Generic;

namespace Orcus.Shared.Commands.FileExplorer
{
    [Serializable]
    public class RootEntryCollection
    {
        public List<PackedDirectoryEntry> RootDirectories { get; set; }
        public PackedDirectoryEntry ComputerDirectory { get; set; }
        public List<IFileExplorerEntry> ComputerDirectoryEntries { get; set; }
    }
}