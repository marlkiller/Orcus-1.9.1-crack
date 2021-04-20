using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Threading;
using Orcus.Administration.FileExplorer.Utilities;
using Orcus.Administration.Utilities;
using Orcus.Administration.ViewModels.ViewObjects;

namespace Orcus.Administration.Behavior
{
    public class VisibleItemsListBoxBehavior : Behavior<ListBox>
    {
        private ScrollViewer _fileListScrollViewer;
        private INotifyCollectionChanged _notifyCollection;

        public static readonly DependencyProperty VisibleItemsProperty = DependencyProperty.Register(
            "VisibleItems", typeof(RangeInfo), typeof(VisibleItemsListBoxBehavior), new PropertyMetadata(default(RangeInfo)));

        public RangeInfo VisibleItems
        {
            get { return (RangeInfo) GetValue(VisibleItemsProperty); }
            set { SetValue(VisibleItemsProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.AddValueChanged(ItemsControl.ItemsSourceProperty, ItemsSourceChanged);
            AssociatedObject.Loaded += AssociatedObjectOnLoaded;
        }

        private void AssociatedObjectOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            _fileListScrollViewer = WpfExtensions.GetFirstChildOfType<ScrollViewer>(AssociatedObject);
            _fileListScrollViewer.ScrollChanged += FileListScrollViewerOnScrollChanged;
        }

        private async void ItemsSourceChanged(object sender, EventArgs eventArgs)
        {
            if (_notifyCollection != null)
                _notifyCollection.CollectionChanged -= NotifyCollectionOnCollectionChanged;

            _notifyCollection = AssociatedObject.ItemsSource as INotifyCollectionChanged;

            if (_notifyCollection != null)
                _notifyCollection.CollectionChanged += NotifyCollectionOnCollectionChanged;

            await Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { }));

            OnUpdateVisibleItems();
        }

        private async void NotifyCollectionOnCollectionChanged(object sender,
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            await Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { }));
            OnUpdateVisibleItems();
        }

        private void OnUpdateVisibleItems()
        {
            var startItem = (int) Math.Floor(_fileListScrollViewer.VerticalOffset);
            var itemCount = (int) Math.Ceiling(_fileListScrollViewer.ViewportHeight);
            if (startItem < 0)
                startItem = 0;

            var rangeInfo = new RangeInfo(startItem, itemCount);
            if (!rangeInfo.Equals(VisibleItems))
                VisibleItems = rangeInfo;
        }

        private void FileListScrollViewerOnScrollChanged(object sender, ScrollChangedEventArgs scrollChangedEventArgs)
        {
            OnUpdateVisibleItems();
        }
    }
}