using System.Collections.Generic;
using System.Threading.Tasks;
using Orcus.Administration.FileExplorer.Models;

namespace Orcus.Administration.FileExplorer.Helpers
{
    public interface ITreeLookup<VM, T>
    {
        Task Lookup(T value, ITreeSelector<VM, T> parentSelector,
            ICompareHierarchy<T> comparer, params ITreeLookupProcessor<VM, T>[] processors);
    }

    public class SearchNextLevel<VM, T> : ITreeLookup<VM, T>
    {
        public static SearchNextLevel<VM, T> LoadSubentriesIfNotLoaded = new SearchNextLevel<VM, T>();

        public async Task Lookup(T value, ITreeSelector<VM, T> parentSelector,
            ICompareHierarchy<T> comparer, params ITreeLookupProcessor<VM, T>[] processors)
        {
            foreach (VM current in await parentSelector.EntryHelper.LoadAsync())
                if (current is ISupportTreeSelector<VM, T>)
                {
                    var currentSelectionHelper = (current as ISupportTreeSelector<VM, T>).Selection;
                    var compareResult = comparer.CompareHierarchy(currentSelectionHelper.Value, value);
                    switch (compareResult)
                    {
                        case HierarchicalResult.Current:
                        case HierarchicalResult.Child:
                            processors.Process(compareResult, parentSelector, currentSelectionHelper);
                            return;
                    }
                }
        }
    }

    public class BroadcastNextLevel<VM, T> : ITreeLookup<VM, T>
    {
        public static BroadcastNextLevel<VM, T> LoadSubentriesIfNotLoaded = new BroadcastNextLevel<VM, T>();

        public async Task Lookup(T value, ITreeSelector<VM, T> parentSelector,
            ICompareHierarchy<T> comparer, params ITreeLookupProcessor<VM, T>[] processors)
        {
            foreach (VM current in await parentSelector.EntryHelper.LoadAsync())
                if (current is ISupportTreeSelector<VM, T>)
                {
                    var currentSelectionHelper = (current as ISupportTreeSelector<VM, T>).Selection;
                    var compareResult = comparer.CompareHierarchy(currentSelectionHelper.Value, value);
                    processors.Process(compareResult, parentSelector, currentSelectionHelper);
                }
        }
    }

    public class SearchNextUsingReverseLookup<VM, T> : ITreeLookup<VM, T>
    {
        public SearchNextUsingReverseLookup(ITreeSelector<VM, T> targetSelector)
        {
            _targetSelector = targetSelector;
            _hierarchy = new Stack<ITreeSelector<VM, T>>();
            var current = targetSelector;
            while (current != null)
            {
                _hierarchy.Push(current);
                current = current.ParentSelector;
            }
        }

        Stack<ITreeSelector<VM, T>> _hierarchy;
        private ITreeSelector<VM, T> _targetSelector;
        public async Task Lookup(T value, ITreeSelector<VM, T> parentSelector,
            ICompareHierarchy<T> comparer, params ITreeLookupProcessor<VM, T>[] processors)
        {
            if (parentSelector.EntryHelper.IsLoaded)
                foreach (VM current in parentSelector.EntryHelper.AllNonBindable)
                    if (current is ISupportTreeSelector<VM, T> && current is ISupportEntriesHelper<VM>)
                    {
                        var currentSelectionHelper = (current as ISupportTreeSelector<VM, T>).Selection;
                        var compareResult = comparer.CompareHierarchy(currentSelectionHelper.Value, value);
                        switch (compareResult)
                        {
                            case HierarchicalResult.Child:
                            case HierarchicalResult.Current:
                                if (_hierarchy.Contains(currentSelectionHelper))
                                {
                                    processors.Process(compareResult, parentSelector, currentSelectionHelper);
                                    return;
                                }
                                break;
                        }
                    }
        }
    }

    public class RecrusiveSearch<VM, T> : ITreeLookup<VM, T>
    {
        public static RecrusiveSearch<VM, T> LoadSubentriesIfNotLoaded = new RecrusiveSearch<VM, T>(true);
        public static RecrusiveSearch<VM, T> SkipIfNotLoaded = new RecrusiveSearch<VM, T>(false);

        bool _loadSubEntries;

        public RecrusiveSearch(bool loadSubEntries)
        {
            _loadSubEntries = loadSubEntries;
        }

        public async Task Lookup(T value, ITreeSelector<VM, T> parentSelector,
           ICompareHierarchy<T> comparer, params ITreeLookupProcessor<VM, T>[] processors)
        {
            IEnumerable<VM> subentries = _loadSubEntries ?
                await parentSelector.EntryHelper.LoadAsync() :
                 parentSelector.EntryHelper.AllNonBindable;

            if (subentries != null)
                foreach (VM current in subentries)
                    if (current is ISupportTreeSelector<VM, T> && current is ISupportEntriesHelper<VM>)
                    {
                        var currentSelectionHelper = (current as ISupportTreeSelector<VM, T>).Selection;
                        var compareResult = comparer.CompareHierarchy(currentSelectionHelper.Value, value);
                        switch (compareResult)
                        {
                            case HierarchicalResult.Current:
                                processors.Process(compareResult, parentSelector, currentSelectionHelper);
                                return;

                            case HierarchicalResult.Child:
                                if (processors.Process(compareResult, parentSelector, currentSelectionHelper))
                                {
                                    await Lookup(value, currentSelectionHelper, comparer, processors);

                                    return;
                                }

                                break;
                        }
                    }
        }
    }


    public class RecrusiveBroadcast<VM, T> : ITreeLookup<VM, T>
    {
        public static RecrusiveBroadcast<VM, T> LoadSubentriesIfNotLoaded = new RecrusiveBroadcast<VM, T>(false);
        public static RecrusiveBroadcast<VM, T> SkipIfNotLoaded = new RecrusiveBroadcast<VM, T>(false);

        bool _loadSubEntries;
        public RecrusiveBroadcast(bool loadSubEntries)
        {
            _loadSubEntries = loadSubEntries;
        }

        public async Task Lookup(T value, ITreeSelector<VM, T> parentSelector,
          ICompareHierarchy<T> comparer, params ITreeLookupProcessor<VM, T>[] processors)
        {

            IEnumerable<VM> subentries = _loadSubEntries ?
                await parentSelector.EntryHelper.LoadAsync() :
                 parentSelector.EntryHelper.AllNonBindable;

            foreach (VM current in subentries)
                if (current is ISupportTreeSelector<VM, T> && current is ISupportEntriesHelper<VM>)
                {
                    var currentSelectionHelper = (current as ISupportTreeSelector<VM, T>).Selection;
                    var compareResult = comparer.CompareHierarchy(currentSelectionHelper.Value, value);
                    if (processors.Process(compareResult, parentSelector, currentSelectionHelper))
                    {
                        await Lookup(value, currentSelectionHelper, comparer, processors);
                        return;
                    }
                    break;
                }
        }
    }
}
