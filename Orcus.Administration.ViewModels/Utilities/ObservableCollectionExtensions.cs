using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Orcus.Administration.FileExplorer.Utilities;

namespace Orcus.Administration.ViewModels.Utilities
{
    public static class ObservableCollectionExtensions
    {
        public static void Update<T>(this ObservableCollection<T> source, IEnumerable<T> update)
        {
            var items = update.ToList();
            var currentItems = source.ToList();

            foreach (var item in items)
            {
                if (source.Contains(item))
                {
                    items.Remove(item);
                    currentItems.Remove(item);
                }
            }

            foreach (var item in currentItems)
                source.Remove(item);

            foreach (var item in items)
                source.Add(item);
        }

        public static void Update<T>(this FastObservableCollection<T> source, IEnumerable<T> update)
        {
            source.SuspendCollectionChangeNotification();
            try
            {
                Update((ObservableCollection<T>) source, update);
            }
            finally 
            {
                source.NotifyChanges();
            }
        }
    }
}