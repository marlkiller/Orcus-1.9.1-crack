using System;

namespace Orcus.Shared.Commands.FileExplorer
{
    [Serializable]
    public class PackedDirectoryEntry : DirectoryEntry
    {
        public int LabelId { get; set; }
        public string LabelPath { get; set; }
        public int IconId { get; set; }
    }
}