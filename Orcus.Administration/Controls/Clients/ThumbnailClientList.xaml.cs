using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Orcus.Administration.Core.ClientManagement;
using Orcus.Administration.Core.Utilities;
using Orcus.Administration.Extensions;

namespace Orcus.Administration.Controls.Clients
{
    /// <summary>
    /// Interaction logic for ThumbnailClientList.xaml
    /// </summary>
    public partial class ThumbnailClientList : IClientPresenter
    {
        private readonly ClientProvider _clientProvider;
        private FilterParser _currentFilter;
        private readonly DispatcherTimer _thumbnailDispatcherTimer;
        private static BitmapSource _placeholderBitmapSource;
        private SpecialUniformGrid _specialUniformGrid;

        public ThumbnailClientList(ClientProvider clientProvider)
        {
            _clientProvider = clientProvider;
            CollectionView = new CollectionViewSource { Source = clientProvider.Clients };
            CollectionView.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
            CollectionView.LiveGroupingProperties.Add("Group");
            CollectionView.IsLiveGroupingRequested = true;

            CollectionView.LiveFilteringProperties.Add("IsOnline");
            CollectionView.IsLiveFilteringRequested = true;

            CollectionView.Filter += CollectionViewOnFilter;
            InitializeComponent();

            _thumbnailDispatcherTimer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(2)};
            _thumbnailDispatcherTimer.Tick += ThumbnailDispatcherTimerOnTick;
        }

        public static BitmapSource PlaceholderSource
        {
            get
            {
                if (_placeholderBitmapSource == null)
                {
                    using (var bmp = new Bitmap(300, 169))
                        _placeholderBitmapSource = BitmapConverter.ToBitmapSource(bmp);
                }

                return _placeholderBitmapSource;
            }
        }

        private async void ThumbnailDispatcherTimerOnTick(object sender, EventArgs eventArgs)
        {
            if (CollectionView.View.IsEmpty)
                return;

            int firstVisibleItem;
            int lastVisibleItem;
            _specialUniformGrid.GetVisibleRange(out firstVisibleItem, out lastVisibleItem);
            lastVisibleItem += 1;
            var items =
                CollectionView.View.Cast<ClientViewModel>()
                    .Skip(firstVisibleItem > 1 ? firstVisibleItem - 1 : 0)
                    .Take(lastVisibleItem).ToList();

            var totalImagesInMemory = _clientProvider.Clients.Where(x => x.Thumbnail != null && !items.Contains(x)).ToList();
            if(totalImagesInMemory.Count > 60)
                foreach (var clientViewModel in totalImagesInMemory.OrderBy(x => x.ThumbnailTimestamp).Take(75 - items.Count)
                    )
                    clientViewModel.Thumbnail = null;

            try
            {
                await Task.Run(() => _clientProvider.GetClientScreens(items.Select(x => x.Id).ToList()));
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void CollectionViewOnFilter(object sender, FilterEventArgs filterEventArgs)
        {
            var item = (ClientViewModel) filterEventArgs.Item;
            filterEventArgs.Accepted = item.IsOnline;

            if (_currentFilter == null)
                return;

            filterEventArgs.Accepted = _currentFilter.IsAccepted(item) && filterEventArgs.Accepted;
        }

        public CollectionViewSource CollectionView { get; set; }
        public List<ClientViewModel> VisibleClients { get; }

        public ContextMenu ItemContextMenu { get; set; }
        public ICommands Commands { get; set; }

        public IList SelectedItems => ClientListBox.SelectedItems;

        public void UpdateSearchText(FilterParser filterParser)
        {
            _currentFilter = filterParser;
            CollectionView.View.Refresh();
        }

        public void Enable(FilterParser filterParser)
        {
            _currentFilter = filterParser;
            CollectionView.View.Refresh();
            _thumbnailDispatcherTimer.Start();
        }

        public void Disable()
        {
            _thumbnailDispatcherTimer.Stop();
        }

        private void UniformGrid_OnLoaded(object sender, RoutedEventArgs e)
        {
            _specialUniformGrid = (SpecialUniformGrid) sender;
        }
    }
}