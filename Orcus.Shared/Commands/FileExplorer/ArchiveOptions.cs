using System;
using System.Collections.Generic;

namespace Orcus.Shared.Commands.FileExplorer
{
    [Serializable]
    public class ArchiveOptions
    {
        public CompressionMethod CompressionMethod { get; set; }
        public bool UseTarPacker { get; set; }
        public string ArchivePath { get; set; }
        public List<EntryInfo> Entries { get; set; }
        public int CompressionLevel { get; set; }
        public string Password { get; set; }
        public bool DeleteAfterArchiving { get; set; }
    }
}