using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.DataManager
{
    public class DownloadTaskViewModel : PropertyChangedBase
    {
        private readonly AutoResetEvent _autoResetEvent;

        private long _bytesReceived;
        private byte[] _fileHash;
        private FileStream _fileStream;
        private double _progress;

        public DownloadTaskViewModel(ViewData viewData, string fileName)
        {
            FileName = fileName;
            ViewData = viewData;
            TotalBytesToReceive = viewData.Size;
            _fileStream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
            _autoResetEvent = new AutoResetEvent(false);
        }

        public event EventHandler DownloadFinished;
        public event EventHandler DownloadFailed;
        public event EventHandler ProgressChanged;

        public ViewData ViewData { get; }
        public string FileName { get; }
        public bool IsFinished { get; private set; }

        public double Progress
        {
            get { return _progress; }
            set { SetProperty(value, ref _progress); }
        }

        public long BytesReceived
        {
            get { return _bytesReceived; }
            set { SetProperty(value, ref _bytesReceived); }
        }

        public double TotalBytesToReceive { get; }

        public void RegisterHash(byte[] bytes)
        {
            _fileHash = bytes;
            _autoResetEvent.Set();
        }

        public async void ReceiveData(byte[] data)
        {
            _fileStream.Write(data, 0, data.Length);
            if (_fileStream.Length == TotalBytesToReceive)
            {
                if (_fileHash == null)
                    await Task.Run(() => _autoResetEvent.WaitOne());

                _fileStream.Position = 0;
                byte[] fileHash;
                using (var sha256 = new SHA256Managed())
                    fileHash = sha256.ComputeHash(_fileStream);

                _fileStream.Close();
                _fileStream = null;

                if (fileHash.SequenceEqual(_fileHash))
                {
                    ViewData.DataManagerType.ModifyDownloadedFile(FileName);
                    DownloadFinished?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    File.Delete(FileName);
                    DownloadFailed?.Invoke(this, null);
                }
                IsFinished = true;
                return;
            }

            BytesReceived = _fileStream.Length;
            Progress = BytesReceived/TotalBytesToReceive;
            ProgressChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}