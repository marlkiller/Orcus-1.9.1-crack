using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Orcus.Business.Manager.Core;
using Orcus.Business.Manager.Core.Data;

namespace Orcus.Business.Manager.Views
{
    /// <summary>
    ///     Interaction logic for DownloadingDataWindow.xaml
    /// </summary>
    public partial class DownloadingDataWindow : INotifyPropertyChanged, ICurrentStatusReporter
    {
        private double _currentProgress;
        private string _currentStatus;

        public DownloadingDataWindow()
        {
            InitializeComponent();
            Loaded += DownloadingDataWindow_Loaded;
        }

        public DatabaseInfo DatabaseInfo { get; private set; }

        public string CurrentStatus
        {
            get { return _currentStatus; }
            set
            {
                if (_currentStatus != value)
                {
                    _currentStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        public double CurrentProgress
        {
            get { return _currentProgress; }
            set
            {
                if (_currentProgress != value)
                {
                    _currentProgress = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async void DownloadingDataWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            DatabaseInfo = await WebServerConnection.DownloadData(this);
            foreach (var license in DatabaseInfo.licenses)
            {
                license.RegisteredComputers = DatabaseInfo.computers.Count(x => x.licenseId == license.id);
            }
            DialogResult = true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}