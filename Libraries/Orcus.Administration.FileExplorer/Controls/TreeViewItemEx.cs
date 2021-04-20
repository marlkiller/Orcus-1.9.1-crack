using System.Windows;
using System.Windows.Controls;
using Orcus.Administration.FileExplorer.Utilities;

namespace Orcus.Administration.FileExplorer.Controls
{
    public class TreeViewItemEx : TreeViewItem
    {
        public static readonly DependencyProperty IsBringIntoViewProperty = DependencyProperty.Register(
            "IsBringIntoView", typeof (bool), typeof (TreeViewItemEx),
            new PropertyMetadata(default(bool)));

        public TreeViewItemEx()
        {
            AddHandler(SelectedEvent, new RoutedEventHandler(
                delegate(object obj, RoutedEventArgs args) { (args.OriginalSource as TreeViewItem)?.BringIntoView(); }));

            this.AddValueChanged(IsBringIntoViewProperty, (sender, args) =>
            {
                var treeViewItem = (TreeViewItemEx) sender;
                
                if (treeViewItem.IsBringIntoView)
                {
                    treeViewItem.BringIntoView();
                    treeViewItem.IsBringIntoView = false;
                }
            });
        }

        public bool IsBringIntoView
        {
            get { return (bool) GetValue(IsBringIntoViewProperty); }
            set { SetValue(IsBringIntoViewProperty, value); }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeViewItemEx();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeViewItemEx;
        }
    }
}