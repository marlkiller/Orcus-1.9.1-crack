using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace OrcusPluginStudio.Core
{
    public class LibraryWatcher : IDisposable
    {
        private readonly object _fileLock = new object();
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly string _path;
        private byte[] _currentFile;

        public LibraryWatcher(string path)
        {
            _path = path;
            _fileSystemWatcher = new FileSystemWatcher(Path.GetDirectoryName(path), Path.GetFileName(path))
            {
                IncludeSubdirectories = false
            };
            _fileSystemWatcher.Changed += FileSystemWatcherOnChanged;
            _fileSystemWatcher.Created += _fileSystemWatcher_Created;
            _fileSystemWatcher.Renamed += _fileSystemWatcher_Renamed;
            _fileSystemWatcher.Deleted += _fileSystemWatcher_Deleted;

            if (File.Exists(path))
                _currentFile = GetHashFromFile(path);

            _fileSystemWatcher.EnableRaisingEvents = true;
        }

        public void Dispose()
        {
            _fileSystemWatcher.Dispose();
        }

        private void _fileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            RefreshFile();
        }

        private void _fileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            RefreshFile();
        }

        private void _fileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            RefreshFile();
        }

        private void FileSystemWatcherOnChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            RefreshFile();
        }

        public event EventHandler ReloadFile;

        private byte[] GetHashFromFile(string path)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            using (var fs = WaitForFile(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return md5.ComputeHash(fs);
            }
        }

        FileStream WaitForFile(string fullPath, FileMode mode, FileAccess access, FileShare share)
        {
            for (int numTries = 0; numTries < 10; numTries++)
            {
                try
                {
                    FileStream fs = new FileStream(fullPath, mode, access, share);

                    fs.ReadByte();
                    fs.Seek(0, SeekOrigin.Begin);

                    return fs;
                }
                catch (IOException)
                {
                    Thread.Sleep(50);
                }
            }

            return null;
        }

        private void RefreshFile()
        {
            lock (_fileLock)
            {
                if (File.Exists(_path))
                {
                    var fileHash = GetHashFromFile(_path);
                    if (_currentFile == null || !fileHash.SequenceEqual(_currentFile))
                    {
                        _currentFile = fileHash;
                        ReloadFile?.Invoke(this, EventArgs.Empty);
                    }
                }
                else if (_currentFile != null)
                {
                    _currentFile = null;
                    ReloadFile?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}