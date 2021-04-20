using System;
using System.IO;
using System.Threading;

namespace Orcus.Administration.ViewModels.CommandViewModels.FileExplorer
{
    public class Downloader : IDisposable
    {
        private readonly FileTransferTask _fileTransferTask;
        private readonly string _path;
        private long _dataDownloadedSinceLastUpdate;
        private bool _isDisposed;
        private DateTime _lastProgressUpdate;
        private DateTime _lastSlowValuesUpdate;
        private long _totalDataDownloaded;
        private readonly object _finisherLock = new object();

        public Downloader(string path, FileTransferTask fileTransferTask)
        {
            _path = path;
            _fileTransferTask = fileTransferTask;
            FileStream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
            Waiter = new AutoResetEvent(false);

            _lastProgressUpdate = DateTime.UtcNow;
            _lastSlowValuesUpdate = DateTime.UtcNow;
            _totalDataDownloaded = 0;
        }

        public long Size { get; set; }
        public FileStream FileStream { get; }
        public bool Success { get; private set; }
        public AutoResetEvent Waiter { get; }

        public void Dispose()
        {
            if (_isDisposed)
                return;
            _isDisposed = true;

            Waiter.Dispose();
            var delete = FileStream.Length == 0;
            FileStream.Dispose();
            if (delete)
                File.Delete(_path);
        }

        public event EventHandler ProgressChanged;

        public void ReceiveData(byte[] data, int index)
        {
            if (Success || _isDisposed)
                return;

            Interlocked.Add(ref _totalDataDownloaded, data.Length - index);
            FileStream.Write(data, index, data.Length - index);
            CheckFinished();

            _dataDownloadedSinceLastUpdate += data.Length - index;

            if (DateTime.UtcNow - _lastProgressUpdate > TimeSpan.FromMilliseconds(50))
            {
                _fileTransferTask.Progress = _fileTransferTask.ProcessedSize/(double) Size;
                _fileTransferTask.ProcessedSize = FileStream.Length;
                ProgressChanged?.Invoke(this, EventArgs.Empty);

                _lastProgressUpdate = DateTime.UtcNow;
            }

            var slowUpdateTimeDifference = DateTime.UtcNow - _lastSlowValuesUpdate;
            if (slowUpdateTimeDifference > TimeSpan.FromMilliseconds(400))
            {
                _fileTransferTask.Speed = _dataDownloadedSinceLastUpdate/slowUpdateTimeDifference.TotalSeconds;
                _fileTransferTask.EstimatedTime =
                    TimeSpan.FromSeconds((Size - FileStream.Length)/_fileTransferTask.Speed);

                _lastSlowValuesUpdate = DateTime.UtcNow;
                _dataDownloadedSinceLastUpdate = 0;
            }
        }

        public bool CheckFinished()
        {
            if (Success)
                return true;

            if (Interlocked.Read(ref _totalDataDownloaded) == Size)
            {
                if (Success)
                    return true;

                lock (_finisherLock)
                {
                    if (Success)
                        return true;

                    Success = true;
                    FileStream.Position = 0;
                    Waiter.Set();
                }

                return true;
            }

            return false;
        }

        public void Fail()
        {
            Success = false;
            Waiter.Set();
        }
    }
}