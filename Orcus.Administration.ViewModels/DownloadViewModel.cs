using System;
using System.ComponentModel;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels
{
    public class DownloadViewModel : PropertyChangedBase
    {
        private readonly bool _preventClose;
        private long _bytesLoaded;
        private double _progress;
        private bool _canClose;

        public DownloadViewModel(long totalBytes, string fileName, bool preventClose)
        {
            _preventClose = preventClose;
            TotalBytes = totalBytes;
            FileName = fileName;
        }

        public Action CloseWindow { get; set; }

        public long TotalBytes { get; }
        public string FileName { get; }

        public long BytesLoaded
        {
            get { return _bytesLoaded; }
            set
            {
                if (SetProperty(value, ref _bytesLoaded))
                    Progress = value/TotalBytes;
            }
        }

        public double Progress
        {
            get { return _progress; }
            set { SetProperty(value, ref _progress); }
        }

        public void Close()
        {
            _canClose = true;
            CloseWindow.Invoke();
        }

        public void OnWindowClosing(CancelEventArgs e)
        {
            e.Cancel = _preventClose && !_canClose;
        }
    }
}