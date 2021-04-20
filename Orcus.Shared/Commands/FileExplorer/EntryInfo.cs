using System;

namespace Orcus.Shared.Commands.FileExplorer
{
    [Serializable]
    public class EntryInfo
    {
        public string Path { get; set; }
        public bool IsDirectory { get; set; }
    }
}