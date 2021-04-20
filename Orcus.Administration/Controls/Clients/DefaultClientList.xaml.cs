using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Orcus.Administration.Core;
using Orcus.Administration.Core.Annotations;
using Orcus.Administration.Core.ClientManagement;
using Orcus.Administration.Utilities;
using Sorzus.Wpf.Toolkit.Extensions;
using Settings = Orcus.Administration.Core.Settings;

namespace Orcus.Administration.Controls.Clients
{
    /// <summary>
    ///     Interaction logic for DefaultClientList.xaml
    /// </summary>
    public partial class DefaultClientList : IClientPresenter, INotifyPropertyChanged
    {
        private const int ItemHeight = 30;
        private readonly ClientProvider _clientProvider;
        private FilterParser _currentFilter;
        private ContextMenu _itemContextMenu;
        private ScrollViewer _listViewScrollViewer;
        private CancellationTokenSource _scrollChangedCancellationTokenSource;
        private readonly DispatcherTimer _activeWindowRefreshTimer;
        private ICommands _commands;

        public DefaultClientList(ClientProvider clientProvider)
        {
            _clientProvider = clientProvider;
            CollectionView = new CollectionViewSource {Source = clientProvider.Clients};
            CollectionView.Filter += CollectionViewOnFilter;
            InitializeComponent();

            MainListView.Loaded += MainListViewOnLoaded;
            Regroup(Settings.Current.DefaultListGroupBy);
            ColumnData = Settings.Current.DefaultListColumnData;

            _activeWindowRefreshTimer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(2)};
            _activeWindowRefreshTimer.Tick += ActiveWindowRefreshTimerOnTick;
        }

        public bool IsOnlineColumnVisible
        {
            get { return Settings.Current.DefaultListColumnData.Visible.Contains("online"); }
            set
            {
                if (value)
                    Settings.Current.DefaultListColumnData.AddColumn("online");
                else
                    Settings.Current.DefaultListColumnData.RemoveColumn("online");

                OnPropertyChanged();
            }
        }

        public bool UserNameColumnVisible
        {
            get { return Settings.Current.DefaultListColumnData.Visible.Contains("username"); }
            set
            {
                if (value)
                    Settings.Current.DefaultListColumnData.AddColumn("username");
                else
                    Settings.Current.DefaultListColumnData.RemoveColumn("username");

                OnPropertyChanged();
            }
        }

        public bool IpAddressLastSeenColumnVisible
        {
            get { return Settings.Current.DefaultListColumnData.Visible.Contains("ipaddress"); }
            set
            {
                if (value)
                    Settings.Current.DefaultListColumnData.AddColumn("ipaddress");
                else
                    Settings.Current.DefaultListColumnData.RemoveColumn("ipaddress");

                OnPropertyChanged();
            }
        }

        public bool IdColumnVisible
        {
            get { return Settings.Current.DefaultListColumnData.Visible.Contains("id"); }
            set
            {
                if (value)
                    Settings.Current.DefaultListColumnData.AddColumn("id");
                else
                    Settings.Current.DefaultListColumnData.RemoveColumn("id");

                OnPropertyChanged();
            }
        }

        public bool OperatingSystemColumnVisible
        {
            get { return Settings.Current.DefaultListColumnData.Visible.Contains("ostype"); }
            set
            {
                if (value)
                    Settings.Current.DefaultListColumnData.AddColumn("ostype");
                else
                    Settings.Current.DefaultListColumnData.RemoveColumn("ostype");

                OnPropertyChanged();
            }
        }

        public bool VersionColumnVisible
        {
            get { return Settings.Current.DefaultListColumnData.Visible.Contains("version"); }
            set
            {
                if (value)
                    Settings.Current.DefaultListColumnData.AddColumn("version");
                else
                    Settings.Current.DefaultListColumnData.RemoveColumn("version");

                OnPropertyChanged();
            }
        }

        public bool ActiveWindowColumnVisible
        {
            get { return Settings.Current.DefaultListColumnData.Visible.Contains("activewindow"); }
            set
            {
                if (value)
                    Settings.Current.DefaultListColumnData.AddColumn("activewindow");
                else
                    Settings.Current.DefaultListColumnData.RemoveColumn("activewindow");

                OnPropertyChanged();
                _activeWindowRefreshTimer.IsEnabled = value;
            }
        }

        public bool AdministratorColumnVisible
        {
            get { return Settings.Current.DefaultListColumnData.Visible.Contains("administrator"); }
            set
            {
                if (value)
                    Settings.Current.DefaultListColumnData.AddColumn("administrator");
                else
                    Settings.Current.DefaultListColumnData.RemoveColumn("administrator");

                OnPropertyChanged();
            }
        }

        public bool ServiceColumnVisible
        {
            get { return Settings.Current.DefaultListColumnData.Visible.Contains("service"); }
            set
            {
                if (value)
                    Settings.Current.DefaultListColumnData.AddColumn("service");
                else
                    Settings.Current.DefaultListColumnData.RemoveColumn("service");

                OnPropertyChanged();
            }
        }

        public bool CountryColumnVisible
        {
            get { return Settings.Current.DefaultListColumnData.Visible.Contains("country"); }
            set
            {
                if (value)
                    Settings.Current.DefaultListColumnData.AddColumn("country");
                else
                    Settings.Current.DefaultListColumnData.RemoveColumn("country");

                OnPropertyChanged();
            }
        }

        public bool LanguageColumnVisible
        {
            get { return Settings.Current.DefaultListColumnData.Visible.Contains("language"); }
            set
            {
                if (value)
                    Settings.Current.DefaultListColumnData.AddColumn("language");
                else
                    Settings.Current.DefaultListColumnData.RemoveColumn("language");

                OnPropertyChanged();
            }
        }

        public ColumnData ColumnData { get; }

        public GroupByProperty GroupByProperty
        {
            get { return Settings.Current.DefaultListGroupBy; }
            set
            {
                if (value != Settings.Current.DefaultListGroupBy)
                {
                    Settings.Current.DefaultListGroupBy = value;
                    Regroup(value);
                    Settings.Current.Save();
                    OnPropertyChanged();
                }
            }
        }

        public CollectionViewSource CollectionView { get; }

        public ContextMenu ItemContextMenu
        {
            get { return _itemContextMenu; }
            set
            {
                if (_itemContextMenu != value)
                {
                    _itemContextMenu = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommands Commands
        {
            get { return _commands; }
            set
            {
                if (_commands != value)
                {
                    _commands = value;
                    OnPropertyChanged();
                }
            }
        }

        public IList SelectedItems => MainListView.SelectedItems;

        public void UpdateSearchText(FilterParser filterParser)
        {
            _currentFilter = filterParser;
            CollectionView.View.Refresh();
        }

        public void Enable(FilterParser filterParser)
        {
            _currentFilter = filterParser;
            if (_currentFilter != null)
                CollectionView.View.Refresh();

            _activeWindowRefreshTimer.IsEnabled = Settings.Current.DefaultListColumnData.Visible.Contains("activewindow");
        }

        public void Disable()
        {
            _activeWindowRefreshTimer.IsEnabled = false;
        }

        public List<ClientViewModel> VisibleClients { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        private void Regroup(GroupByProperty groupBy)
        {
            CollectionView.GroupDescriptions.Clear();
            CollectionView.LiveGroupingProperties.Clear();

            if (groupBy == GroupByProperty.None)
                return;

            CollectionView.GroupDescriptions.Add(new PropertyGroupDescription(groupBy.ToString()));
            CollectionView.LiveGroupingProperties.Add(groupBy.ToString());
            CollectionView.IsLiveGroupingRequested = true;
        }

        private async void ActiveWindowRefreshTimerOnTick(object sender, EventArgs eventArgs)
        {
            try
            {
                await Task.Run(() => _clientProvider.GetActiveWindows(
                    VisibleClients.Where(x => x.IsOnline && x.Version > 2).Select(x => x.Id).ToList()));
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void MainListViewOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            _listViewScrollViewer = WpfExtensions.GetFirstChildOfType<ScrollViewer>(MainListView);
            _listViewScrollViewer.ScrollChanged += ListViewScrollViewerOnScrollChanged;
            RefreshVisibleClients();
        }

        private async void ListViewScrollViewerOnScrollChanged(object sender,
            ScrollChangedEventArgs scrollChangedEventArgs)
        {
            _scrollChangedCancellationTokenSource?.Cancel();

            var tokenSource = new CancellationTokenSource();
            _scrollChangedCancellationTokenSource = tokenSource;

            // ReSharper disable once MethodSupportsCancellation
            await Task.Delay(100);

            using (tokenSource)
            {
                if (_scrollChangedCancellationTokenSource == tokenSource)
                    _scrollChangedCancellationTokenSource = null;

                if (tokenSource.Token.IsCancellationRequested)
                    return;
            }

            RefreshVisibleClients();
        }

        private void RefreshVisibleClients()
        {
            var startItem = (int) Math.Floor(_listViewScrollViewer.VerticalOffset/ItemHeight) - 10;
            var itemCount = (int) Math.Ceiling(_listViewScrollViewer.ViewportHeight/ItemHeight) + 20;
            if (startItem < 0)
                startItem = 0;

            var items = CollectionView.View.Cast<ClientViewModel>().Skip(startItem).Take(itemCount).ToList();
            VisibleClients = items;

            _clientProvider.RequestClientInformation(items.Where(x => !x.InformationGrabbed).ToList());
        }

        private void CollectionViewOnFilter(object sender, FilterEventArgs filterEventArgs)
        {
            filterEventArgs.Accepted = true;

            if (_currentFilter == null)
                return;

            var item = (ClientViewModel) filterEventArgs.Item;
            filterEventArgs.Accepted = _currentFilter.IsAccepted(item);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void MenuItem_OnUnchecked(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem) sender;
            if (GroupByProperty == (GroupByProperty) menuItem.Tag && !((MenuItem) sender).IsChecked)
                OnPropertyChanged(nameof(GroupByProperty));
        }
    }
}