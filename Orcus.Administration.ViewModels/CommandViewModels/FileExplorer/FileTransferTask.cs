using System;
using System.Diagnostics;
using System.IO;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels.FileExplorer
{
    public class FileTransferTask : PropertyChangedBase
    {
        private RelayCommand _cancelCommand;
        private TimeSpan _estimatedTime;
        private byte[] _hashValue;
        private bool _isFinished;
        private bool _isWorking;
        private long _processedSize;
        private double _progress;
        private long _size;
        private double _speed;
        private FileProcessEntryState _state;
        private RelayCommand _openFolderCommand;

        public FileTransferTask(string path, string targetPath, bool isUpload, bool isDirectory)
        {
            Path = path;
            TargetPath = targetPath;
            IsUpload = isUpload;
            IsDirectory = isDirectory;
            if (isDirectory)
            {
                Name = new DirectoryInfo(path).Name;
            }
            else
            {
                var fileInfo = new FileInfo(path);
                if (isUpload)
                    Size = fileInfo.Length;
                Name = fileInfo.Name;
            }
            _state = FileProcessEntryState.Waiting;
        }

        public event EventHandler ProgressChanged;

        public string Name { get; }
        public bool IsDirectory { get; }
        public bool IsUpload { get; }
        public string Path { get; }
        public string TargetPath { get; }
        public bool IsCanceled { get; set; }

        public bool IsWorking
        {
            get { return _isWorking; }
            set { SetProperty(value, ref _isWorking); }
        }

        public long Size
        {
            get { return _size; }
            set { SetProperty(value, ref _size); }
        }

        public long ProcessedSize
        {
            get { return _processedSize; }
            set { SetProperty(value, ref _processedSize); }
        }

        public double Progress
        {
            get { return _progress; }
            set { SetProperty(value, ref _progress); }
        }

        public bool IsFinished
        {
            get { return _isFinished; }
            set { SetProperty(value, ref _isFinished); }
        }

        public FileProcessEntryState State
        {
            get { return _state; }
            set { SetProperty(value, ref _state); }
        }

        public byte[] HashValue
        {
            get { return _hashValue; }
            set
            {
                if (SetProperty(value, ref _hashValue))
                {
                    HashValueString = value == null
                        ? string.Empty
                        : BitConverter.ToString(value).Replace("-", string.Empty);
                    OnPropertyChanged(nameof(HashValueString));
                }
            }
        }

        public string HashValueString { get; private set; }

        public double Speed
        {
            get { return _speed; }
            set { SetProperty(value, ref _speed); }
        }

        public TimeSpan EstimatedTime
        {
            get { return _estimatedTime; }
            set { SetProperty(value, ref _estimatedTime); }
        }

        public void UpdateProgress()
        {
            ProgressChanged?.Invoke(this, EventArgs.Empty);
        }

        public RelayCommand CancelCommand
        {
            get { return _cancelCommand ?? (_cancelCommand = new RelayCommand(parameter => { Cancel(); })); }
        }

        public RelayCommand OpenFolderCommand
        {
            get
            {
                return _openFolderCommand ?? (_openFolderCommand = new RelayCommand(parameter =>
                {
                    if (State != FileProcessEntryState.Succeed)
                        return;

                    Process.Start("explorer.exe", $"/select, \"{(IsUpload ? Path : TargetPath)}\"");
                }));
            }
        }

        public event EventHandler CancelRequest;

        public void Cancel()
        {
            IsCanceled = true;
            CancelRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}