using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Orcus.Administration.Utilities;

namespace Orcus.Administration.Extensions
{
    public static class TreeViewExtensions
    {
        public static readonly DependencyProperty SelectOnRightClickProperty = DependencyProperty.RegisterAttached(
            "SelectOnRightClick", typeof (bool), typeof (TreeViewExtensions),
            new PropertyMetadata(default(bool), SelectOnRightClickChanged));

        private static void SelectOnRightClickChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var treeView = dependencyObject as TreeView;
            if ((bool) dependencyPropertyChangedEventArgs.NewValue)
            {
                treeView.MouseRightButtonDown += TreeViewOnPreviewMouseRightButtonDown;
            }
            else
            {
                treeView.MouseRightButtonDown -= TreeViewOnPreviewMouseRightButtonDown;
            }
        }

        private static void TreeViewOnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var treeListViewItem =
                WpfExtensions.VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject);
            if (treeListViewItem != null)
            {
                treeListViewItem.Focus();
                treeListViewItem.IsSelected = true;
                e.Handled = true;
            }
        }

        public static void SetSelectOnRightClick(DependencyObject element, bool value)
        {
            element.SetValue(SelectOnRightClickProperty, value);
        }

        public static bool GetSelectOnRightClick(DependencyObject element)
        {
            return (bool) element.GetValue(SelectOnRightClickProperty);
        }
    }
}