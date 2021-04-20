using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Orcus.Administration.FileExplorer.Models;

namespace Orcus.Administration.FileExplorer.Helpers
{
    public interface ITreeSelector<VM, T> : INotifyPropertyChanged
    {
        /// <summary>
        /// Used by a tree node to report to it's root it's selected.
        /// </summary>
        /// <param name="path"></param>
        void ReportChildSelected(Stack<ITreeSelector<VM, T>> path);

        /// <summary>
        /// Used by a tree node to report to it's parent it's deselected.
        /// </summary>
        /// <param name="path"></param>
        void ReportChildDeselected(Stack<ITreeSelector<VM, T>> path);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="pathAction">Run when lookup along the path (e.g. when HierarchicalResult = Child or Current)</param>
        /// <param name="nextNodeOnly"></param>
        /// <returns></returns>
        Task LookupAsync(T value, ITreeLookup<VM, T> lookupProc,
            params ITreeLookupProcessor<VM, T>[] processors);

        /// <summary>
        /// Whether current view model is selected.
        /// </summary>
        bool IsSelected { get; set; }

        /// <summary>
        /// This is marked by TreeRootSelector, for overflow menu support.
        /// </summary>
        bool IsRoot { get; set; }

        /// <summary>
        /// Whether a child of current view model is selected.
        /// </summary>
        bool IsChildSelected { get; }

        /// <summary>
        /// Based on IsRoot and IsChildSelected
        /// </summary>
        bool IsRootAndIsChildSelected { get; }

        /// <summary>
        /// The selected child of current view model.
        /// </summary>
        T SelectedChild { get; set; }

        void SetIsSelected(bool value);
        void SetSelectedChild(T value);

        /// <summary>
        /// The owner view model of this selection helper.
        /// </summary>
        VM ViewModel { get; }

        /// <summary>
        /// The represented value of this selection helper.
        /// </summary>
        T Value { get; }

        ITreeSelector<VM, T> ParentSelector { get; }
        ITreeRootSelector<VM, T> RootSelector { get; }
        IEntriesHelper<VM> EntryHelper { get; }

    }


    public interface ISupportTreeSelector<VM, T> : ISupportEntriesHelper<VM>       
    {
        ITreeSelector<VM, T> Selection { get; set; }
    }

 

    /// <summary>
    /// Implemented in tree node view model, to provide selection support.
    /// </summary>
    /// <typeparam name="VM">ViewModel.</typeparam>
    /// <typeparam name="T">Value</typeparam>
    public interface ITreeRootSelector<VM, T> : ITreeSelector<VM, T>, ICompareHierarchy<T>
    {

        /// <summary>
        /// Select a tree node by value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        Task SelectAsync(T value);


        /// <summary>
        /// Raised when a node is selected, use SelectedValue/ViewModel to return the selected item.
        /// </summary>
        event EventHandler SelectionChanged;

        /// <summary>
        /// Selected node.
        /// </summary>
        VM SelectedViewModel { get;  }

        ITreeSelector<VM, T> SelectedSelector { get; }

        /// <summary>
        /// Value of SelectedViewModel.
        /// </summary>
        T SelectedValue { get; set; }

        

        /// <summary>
        /// Compare Hierarchy of two value.
        /// </summary>
        IEnumerable<ICompareHierarchy<T>> Comparers { get; set; }        

        ObservableCollection<VM> OverflowedAndRootItems { get; set; }


    }
}
