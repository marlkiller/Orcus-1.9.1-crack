using System;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.Commands.DropAndExecute
{
    public class UploadTask : PropertyChangedBase
    {
        private long _bytesUploaded;
        private bool _executeFile;
        private long _fileLength;
        private bool _isCanceled;
        private bool _isUploaded;
        private string _name;
        private double _progress;
        private string _sourceFile;

        public Guid Guid { get; internal set; }
        public bool IsSent { get; internal set; }

        public string SourceFile
        {
            get { return _sourceFile; }
            set { SetProperty(value, ref _sourceFile); }
        }

        public double Progress
        {
            get { return _progress; }
            set { SetProperty(value, ref _progress); }
        }

        public long BytesUploaded
        {
            get { return _bytesUploaded; }
            set { SetProperty(value, ref _bytesUploaded); }
        }

        public long FileLength
        {
            get { return _fileLength; }
            set { SetProperty(value, ref _fileLength); }
        }

        public bool ExecuteFile
        {
            get { return _executeFile; }
            set { SetProperty(value, ref _executeFile); }
        }

        public string Name
        {
            get { return _name; }
            set { SetProperty(value, ref _name); }
        }

        public bool IsCanceled
        {
            get { return _isCanceled; }
            set { SetProperty(value, ref _isCanceled); }
        }

        public bool IsUploaded
        {
            get { return _isUploaded; }
            set { SetProperty(value, ref _isUploaded); }
        }
    }
}