using System.Windows;
using System.Windows.Media;

namespace Orcus.Administration.FileExplorer.Utilities
{
    public static class VisualTreeHelperExtensions
    {
        public static T FindParent<T>(this DependencyObject child) where T : DependencyObject
        {
            while (true)
            {
                //get parent item
                DependencyObject parentObject = VisualTreeHelper.GetParent(child);

                //we've reached the end of the tree
                if (parentObject == null) return null;

                //check if the parent matches the type we're looking for
                T parent = parentObject as T;
                if (parent != null)
                    return parent;
                child = parentObject;
            }
        }
    }
}