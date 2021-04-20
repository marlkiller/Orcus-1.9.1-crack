using System;

namespace Orcus.Shared.Commands.FileExplorer
{
    public interface IFileExplorerEntry : IEquatable<IFileExplorerEntry>
    {
        string Name { get; set; }
        string Path { get; set; }
        IFileExplorerEntry Parent { get; set; }
        DateTime LastAccess { get; set; }
        DateTime CreationTime { get; set; }

        EntryInfo ToEntryInfo();
    }
}