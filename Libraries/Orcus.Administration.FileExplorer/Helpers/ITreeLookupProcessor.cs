using System;
using Orcus.Administration.FileExplorer.Models;

namespace Orcus.Administration.FileExplorer.Helpers
{
    public interface ITreeLookupProcessor<VM, T>
    {
        bool Process(HierarchicalResult hr, ITreeSelector<VM,T> parentSelector, ITreeSelector<VM,T> selector);
    }

    public static class ITreeSelectionProcessorExtension
    {
        public static bool Process<VM, T>(this ITreeLookupProcessor<VM, T>[] processors,
            HierarchicalResult hr, ITreeSelector<VM,T> parentSelector, ITreeSelector<VM,T> selector)
        {
            foreach (var p in processors)
                if (!p.Process(hr, parentSelector, selector))
                    return false;
            return true;
        }
    }

    public class TreeLookupProcessor<VM, T> : ITreeLookupProcessor<VM, T>
    {

        public TreeLookupProcessor(HierarchicalResult appliedResult, 
            Func<HierarchicalResult, ITreeSelector<VM, T>, ITreeSelector<VM, T>, bool> processFunc)
        {
            _processFunc = processFunc;
            _appliedResult = appliedResult;
        }

        private Func<HierarchicalResult, ITreeSelector<VM, T>, ITreeSelector<VM, T>, bool> _processFunc;
        private HierarchicalResult _appliedResult;

        public bool Process(HierarchicalResult hr, ITreeSelector<VM,T> parentSelector, ITreeSelector<VM,T> selector)
        {
            if (_appliedResult.HasFlag(hr))
                return _processFunc(hr, parentSelector, selector);
            return true;
        }
    }

    public class SetSelected<VM, T> : ITreeLookupProcessor<VM, T>
    {
        public static SetSelected<VM, T> WhenSelected = new SetSelected<VM, T>();

        public bool Process(HierarchicalResult hr, ITreeSelector<VM,T> parentSelector, ITreeSelector<VM,T> selector)
        {
            if (hr == HierarchicalResult.Current)
                selector.IsSelected = true;
            return true;
        }
    }

    public class SetNotSelected<VM, T> : ITreeLookupProcessor<VM, T>
    {
        public static SetNotSelected<VM, T> WhenCurrent = new SetNotSelected<VM, T>(HierarchicalResult.Current);
        public static SetNotSelected<VM, T> WhenNotCurrent = new SetNotSelected<VM, T>(
            HierarchicalResult.Child | HierarchicalResult.Parent | HierarchicalResult.Unrelated);

        public SetNotSelected(HierarchicalResult hr)
        {
            _hr = hr;
        }

        private HierarchicalResult _hr;

        public bool Process(HierarchicalResult hr, ITreeSelector<VM,T> parentSelector, ITreeSelector<VM,T> selector)
        {
            if (_hr.HasFlag(hr))                
                    selector.IsSelected = false;
            return true;
        }
    }

    public class SetCollapsed<VM, T> : ITreeLookupProcessor<VM, T>
    {
        public static SetCollapsed<VM, T> WhenChildSelected = new SetCollapsed<VM, T>(HierarchicalResult.Child);
        public static SetCollapsed<VM, T> WhenNotRelated = new SetCollapsed<VM, T>(HierarchicalResult.Unrelated);

        public SetCollapsed(HierarchicalResult matchResult)
        {
            MatchResult = matchResult;
        }

        private HierarchicalResult MatchResult { get; set; }

        public bool Process(HierarchicalResult hr, ITreeSelector<VM,T> parentSelector, ITreeSelector<VM,T> selector)
        {
            if (MatchResult.HasFlag(hr))
                selector.EntryHelper.IsExpanded = false;
            return true;
        }
    }

    public class SetChildSelected<VM, T> : ITreeLookupProcessor<VM, T>
    {
        public static SetChildSelected<VM, T> ToSelectedChild = new SetChildSelected<VM, T>();

        public bool Process(HierarchicalResult hr, ITreeSelector<VM,T> parentSelector, ITreeSelector<VM,T> selector)
        {
            if (hr == HierarchicalResult.Child || hr == HierarchicalResult.Current)
               parentSelector.SetSelectedChild(selector.Value);                
            return true;
        }
    }

    //public class HideRootItem<VM, T> : ITreeLookupProcessor<VM, T>
    //{
    //    public static HideRootItem<VM, T> IfIsChild = new HideRootItem<VM, T>();

    //    public bool Process(HierarchicalResult hr, ITreeSelector<VM, T> parentSelector, ITreeSelector<VM, T> selector)
    //    {
    //        if (selector.IsRoot)
    //            selector.IsVisible = hr == HierarchicalResult.Child;            
    //        return true;
    //    }
    //}

    public class SetChildNotSelected<VM, T> : ITreeLookupProcessor<VM, T>
    {
        public static SetChildNotSelected<VM, T> WhenChild = new SetChildNotSelected<VM, T>(HierarchicalResult.Child);
        public static SetChildNotSelected<VM, T> WhenNotChild = new SetChildNotSelected<VM, T>(HierarchicalResult.Current |
            HierarchicalResult.Parent | HierarchicalResult.Unrelated);


        public SetChildNotSelected(HierarchicalResult hr)
        {
            _hr = hr;
        }

        private HierarchicalResult _hr;

        public bool Process(HierarchicalResult hr, ITreeSelector<VM,T> parentSelector, ITreeSelector<VM,T> selector)
        {
            if (_hr.HasFlag(hr))
                
                    selector.SetSelectedChild(default(T));
            return true;
        }
    }

    public class SetExpanded<VM, T> : ITreeLookupProcessor<VM, T>
    {
        public static SetExpanded<VM, T> WhenChildSelected = new SetExpanded<VM, T>(HierarchicalResult.Child);
        public static SetExpanded<VM, T> WhenSelected = new SetExpanded<VM, T>(HierarchicalResult.Current);

        public SetExpanded(HierarchicalResult matchResult)
        {
            MatchResult = matchResult;
        }

        private HierarchicalResult MatchResult { get; set; }

        public bool Process(HierarchicalResult hr, ITreeSelector<VM, T> parentSelector, ITreeSelector<VM, T> selector)
        {
            if (MatchResult.HasFlag(hr))
                selector.EntryHelper.IsExpanded = true;
            if (hr == HierarchicalResult.Current)
                ((IIntoViewBringable) selector.ViewModel).IsBringIntoView = true;
            return true;
        }
    }

    public interface IIntoViewBringable
    {
        bool IsBringIntoView { get; set; }
    }
}
