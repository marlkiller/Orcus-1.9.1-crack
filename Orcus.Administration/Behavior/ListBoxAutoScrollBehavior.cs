using System;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Interactivity;
using Orcus.Administration.Utilities;
using Orcus.Administration.FileExplorer.Utilities;

namespace Orcus.Administration.Behavior
{
    public class ListBoxAutoScrollBehavior : Behavior<ListBox>
    {
        private ScrollViewer _scrollViewer;
        private INotifyCollectionChanged _oldItemsSource;

        protected override void OnAttached()
        {
            base.OnAttached();
            _scrollViewer = WpfExtensions.GetDescendantByType<ScrollViewer>(AssociatedObject);
            AssociatedObject.AddValueChanged(ItemsControl.ItemsSourceProperty, ItemsSourceChanged);
            _oldItemsSource = AssociatedObject.ItemsSource as INotifyCollectionChanged;
            if (_oldItemsSource != null)
                _oldItemsSource.CollectionChanged += ItemsSourceOnCollectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (_oldItemsSource != null)
                _oldItemsSource.CollectionChanged -= ItemsSourceOnCollectionChanged;
        }

        private void ItemsSourceChanged(object sender, EventArgs eventArgs)
        {
            if (_oldItemsSource != null)
                _oldItemsSource.CollectionChanged -= ItemsSourceOnCollectionChanged;

            _oldItemsSource = AssociatedObject.ItemsSource as INotifyCollectionChanged;

            if (_oldItemsSource != null)
                _oldItemsSource.CollectionChanged += ItemsSourceOnCollectionChanged;
        }

        private void ItemsSourceOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            _scrollViewer.ScrollToBottom();
        }
    }
}