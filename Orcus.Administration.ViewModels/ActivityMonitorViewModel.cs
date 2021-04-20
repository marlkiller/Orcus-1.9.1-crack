using System;
using System.Windows;
using System.Windows.Threading;
using Orcus.Administration.Core;
using Orcus.Administration.Core.Utilities;
using Orcus.Administration.FileExplorer.Utilities;
using Orcus.Administration.ViewModels.ActivityMonitor;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels
{
    public class ActivityMonitorViewModel : PropertyChangedBase
    {
        private readonly ConnectionManager _connectionManager;
        private BrakingCollectionManager<PackageInformation> _brakingCollectionManager;
        private double _cpuUsage;
        private CpuUsageMonitor _cpuUsageMonitor;
        private long _downloadData;
        private double _downloadSpeed;
        private bool _isOpen;
        private DateTime _lastDataUpdate;
        private long _memoryUsage;
        private FastObservableCollection<PackageInformation> _packages;
        private DispatcherTimer _refreshTimer;
        private RelayCommand _toggleActivityMonitorCommand;
        private long _uploadData;
        private double _uploadSpeed;

        public ActivityMonitorViewModel(ConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
            TotalMemory = Computer.TotalMemory;
            if (TotalMemory == 0)
                TotalMemory = 8589934592; //8 GiB default
        }

        public ulong TotalMemory { get; }

        public bool IsOpen
        {
            get { return _isOpen; }
            set
            {
                if (SetProperty(value, ref _isOpen))
                {
                    if (value)
                        Start();
                    else
                        Stop();
                }
            }
        }

        public double CpuUsage
        {
            get { return _cpuUsage; }
            set { SetProperty(value, ref _cpuUsage); }
        }

        public long MemoryUsage
        {
            get { return _memoryUsage; }
            set { SetProperty(value, ref _memoryUsage); }
        }

        public FastObservableCollection<PackageInformation> Packages
        {
            get { return _packages; }
            set { SetProperty(value, ref _packages); }
        }

        public double UploadSpeed
        {
            get { return _uploadSpeed; }
            set { SetProperty(value, ref _uploadSpeed); }
        }

        public double DownloadSpeed
        {
            get { return _downloadSpeed; }
            set { SetProperty(value, ref _downloadSpeed); }
        }

        public RelayCommand ToggleActivityMonitorCommand
        {
            get
            {
                return _toggleActivityMonitorCommand ??
                       (_toggleActivityMonitorCommand = new RelayCommand(parameter => { IsOpen = !_isOpen; }));
            }
        }

        private void Start()
        {
            if (_refreshTimer == null)
            {
                _refreshTimer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
                _refreshTimer.Tick += RefreshTimerOnTick;
            }
            _refreshTimer.Start();
            _cpuUsageMonitor = new CpuUsageMonitor();
            _lastDataUpdate = DateTime.UtcNow;

            if (Packages == null)
                Packages = new FastObservableCollection<PackageInformation>();
            else
                Packages.Add(new PackageInformation
                {
                    Description = "Activity logging started",
                    Timestamp = DateTime.Now
                });

            _brakingCollectionManager = new BrakingCollectionManager<PackageInformation>(Packages);

            _connectionManager.PackageReceived += ConnectionManagerOnPackageReceived;
            _connectionManager.PackageSent += ConnectionManagerOnPackageSent;
        }

        private void Stop()
        {
            _refreshTimer.Stop();

            _cpuUsageMonitor.Dispose();
            _cpuUsageMonitor = null;

            _connectionManager.PackageReceived -= ConnectionManagerOnPackageReceived;
        }

        private void ConnectionManagerOnPackageReceived(object sender, PackageInformation packageInformation)
        {
            _downloadData += packageInformation.Size;

            Application.Current.Dispatcher.BeginInvoke(
                new Action(() => { _brakingCollectionManager.AddItem(packageInformation); }));
        }

        private void ConnectionManagerOnPackageSent(object sender, PackageInformation packageInformation)
        {
            _uploadData += packageInformation.Size;

            Application.Current.Dispatcher.BeginInvoke(
                new Action(() => { _brakingCollectionManager.AddItem(packageInformation); }));
        }

        private void RefreshTimerOnTick(object sender, EventArgs eventArgs)
        {
            CpuUsage = _cpuUsageMonitor.GetCurrentCpuUsage();
            MemoryUsage = GC.GetTotalMemory(false);

            var difference = DateTime.UtcNow - _lastDataUpdate;
            DownloadSpeed = _downloadData/difference.TotalSeconds;
            UploadSpeed = _uploadData/difference.TotalSeconds;

            _downloadData = 0;
            _uploadData = 0;
            _lastDataUpdate = DateTime.UtcNow;
        }
    }
}