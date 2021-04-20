using System;
using System.Diagnostics;
using System.Reflection;
using nUpdate.UpdateEventArgs;
using Orcus.Administration.Core;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels
{
    public class UpdateViewModel : PropertyChangedBase
    {
        private long _bytesDownloaded;
        private RelayCommand _cancelUpdateCommand;
        private double _currentDownloadSpeed;
        private double _currentProgress;
        private bool _isUpdating;
        private long _totalBytes;
        private RelayCommand _updateCommand;

        public UpdateViewModel(UpdateService updateService)
        {
            UpdateService = updateService;
            UpdateService.DownloadProgressChanged += UpdateService_DownloadProgressChanged;
        }

        public UpdateService UpdateService { get; }

        public string CurrentVersion
        {
            get
            {
                var fvi = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
                return $"{fvi.ProductMajorPart}.{fvi.ProductMinorPart}.{fvi.ProductBuildPart}";
            }
        }

        public bool IsUpdating
        {
            get { return _isUpdating; }
            set { SetProperty(value, ref _isUpdating); }
        }

        public double CurrentProgress
        {
            get { return _currentProgress; }
            set { SetProperty(value, ref _currentProgress); }
        }

        public long BytesDownloaded
        {
            get { return _bytesDownloaded; }
            set { SetProperty(value, ref _bytesDownloaded); }
        }

        public long TotalBytes
        {
            get { return _totalBytes; }
            set { SetProperty(value, ref _totalBytes); }
        }

        public double CurrentDownloadSpeed
        {
            get { return _currentDownloadSpeed; }
            set { SetProperty(value, ref _currentDownloadSpeed); }
        }

        public RelayCommand UpdateCommand
        {
            get
            {
                return _updateCommand ?? (_updateCommand = new RelayCommand(parameter =>
                {
                    IsUpdating = true;
                    UpdateService.Update();
                }));
            }
        }

        public RelayCommand CancelUpdateCommand
        {
            get
            {
                return _cancelUpdateCommand ?? (_cancelUpdateCommand = new RelayCommand(parameter =>
                {
                    try
                    {
                        UpdateService.CancelUpdate();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    IsUpdating = false;
                    CurrentProgress = 0;
                }));
            }
        }

        private void UpdateService_DownloadProgressChanged(object sender, UpdateDownloadProgressChangedEventArgs e)
        {
            CurrentProgress = e.Percentage/100f;
            TotalBytes = e.TotalBytesToReceive;
            BytesDownloaded = e.BytesReceived;
            CurrentDownloadSpeed = Math.Round(e.DownloadSpeed/1024);
        }
    }
}