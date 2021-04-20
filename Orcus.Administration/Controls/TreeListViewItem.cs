using System.Windows;
using System.Windows.Controls;

namespace Orcus.Administration.Controls
{
    public class TreeListViewItem : TreeViewItem
    {
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeListViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeListViewItem;
        }
    }
}