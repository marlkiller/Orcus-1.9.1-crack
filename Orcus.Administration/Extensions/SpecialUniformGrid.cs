using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Orcus.Administration.Utilities;

namespace Orcus.Administration.Extensions
{
    public class SpecialUniformGrid : UniformGrid
    {
        public void GetVisibleRange(out int firstVisibleItemIndex, out int lastVisibleItemIndex)
        {
            firstVisibleItemIndex = 0;
            lastVisibleItemIndex = -1;

            var scrollViewer = WpfExtensions.FindParent<ScrollViewer>(this);
            if (scrollViewer == null)
                return;

            var itemsControl = ItemsControl.GetItemsOwner(this);
            if (!itemsControl.HasItems)
                return;

            var container = (FrameworkElement) itemsControl.ItemContainerGenerator.ContainerFromIndex(0);
            firstVisibleItemIndex = (int)Math.Floor(scrollViewer.VerticalOffset / container.ActualHeight) * Columns;
            lastVisibleItemIndex =
                (int) Math.Ceiling((scrollViewer.VerticalOffset + scrollViewer.ViewportHeight)/container.ActualHeight)*
                Columns - 1;

            var itemCount = itemsControl.HasItems ? itemsControl.Items.Count : 0;
            if (lastVisibleItemIndex >= itemCount)
                lastVisibleItemIndex = itemCount - 1;
        }
    }
}