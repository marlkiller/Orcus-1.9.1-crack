using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Orcus.Administration.FileExplorer.Models;
using Orcus.Administration.FileExplorer.Utilities;

namespace Orcus.Administration.FileExplorer.Helpers
{
    public class TreeSelector<VM, T> : ITreeSelector<VM, T>
    {
        private readonly T _currentValue;
        private readonly VM _currentViewModel;
        private readonly AsyncLock _lookupLock = new AsyncLock();
        private bool _isRoot;
        private bool _isSelected;
        private ITreeSelector<VM, T> _prevSelected;
        private T _selectedValue;

        protected TreeSelector(IEntriesHelper<VM> entryHelper)
        {
            EntryHelper = entryHelper;
            RootSelector = this as ITreeRootSelector<VM, T>;
        }

        public TreeSelector(T currentValue, VM currentViewModel,
            ITreeSelector<VM, T> parentSelector,
            IEntriesHelper<VM> entryHelper)
        {
            RootSelector = parentSelector.RootSelector;
            ParentSelector = parentSelector;
            EntryHelper = entryHelper;
            _currentValue = currentValue;
            _currentViewModel = currentViewModel;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Bubble up to TreeSelectionHelper for selection.
        /// </summary>
        /// <param name="path"></param>
        public virtual void ReportChildSelected(Stack<ITreeSelector<VM, T>> path)
        {
            Debug.Print("Child selected: " + Value);
            if (path.Any())
            {
                _selectedValue = path.Peek().Value;

                OnPropertyChanged(nameof(SelectedChild));
                OnPropertyChanged(nameof(IsChildSelected));
            }

            path.Push(this);
            ParentSelector?.ReportChildSelected(path);
        }

        public virtual void ReportChildDeselected(Stack<ITreeSelector<VM, T>> path)
        {
            Debug.Print("Child deselected: " + Value);

            path.Push(this);
            ParentSelector?.ReportChildDeselected(path);

            if (EntryHelper.IsLoaded)
            {
                //Clear child node selection.
                SetSelectedChild(default(T));
                //And just in case if the new selected value is child of this node.
                /*if (RootSelector.SelectedValue != null)
                    LookupAsync(RootSelector.SelectedValue,
                        new SearchNextUsingReverseLookup<VM, T>(RootSelector.SelectedSelector),
                        new TreeLookupProcessor<VM, T>(HierarchicalResult.All, (hr, p, c) =>
                        {
                            SetSelectedChild(c == null ? default(T) : c.Value);
                            return true;
                        })
                        ).Forget();*/
                //SetSelectedChild(lookupResult == null ? default(T) : lookupResult.Value);
                //OnPropertyChanged(nameof(IsChildSelected));
                //OnPropertyChanged(nameof(SelectedChild));
            }
        }

        /// <summary>
        ///     Tunnel down to select the specified item.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="currentAction"></param>
        /// <returns></returns>
        public async Task LookupAsync(T value,
            ITreeLookup<VM, T> lookupProc,
            params ITreeLookupProcessor<VM, T>[] processors)
        {
            using (await _lookupLock.LockAsync())
            {
                await lookupProc.Lookup(value, this, RootSelector, processors);
            }
        }

        public void SetIsSelected(bool value)
        {
            _isSelected = value;
            OnPropertyChanged(nameof(IsSelected));
            SetSelectedChild(default(T));
        }

        public void SetSelectedChild(T newValue)
        {
            //Debug.WriteLine(String.Format("SetSelectedChild of {0} to {1}", this.Value, newValue));

            if (newValue == null && EntryHelper.IsLoaded && _selectedValue != null)
            {
                //foreach (var node in _entryHelper.AllNonBindable)
                //    if ((node as ISupportNodeSelectionHelper<VM, T>).Selection.IsChildSelected)
                //        (node as ISupportNodeSelectionHelper<VM, T>).Selection.SelectedChild = default(T);
                //var selectedNode = AsyncUtils.RunSync(() => this.LookupAsync(_selectedValue, true));
                //if (selectedNode != null && selectedNode.IsChildSelected)
                //    selectedNode.SetSelectedChild(default(T));
            }

            _selectedValue = newValue;

            OnPropertyChanged(nameof(SelectedChild));
            OnPropertyChanged(nameof(IsChildSelected));
            OnPropertyChanged(nameof(IsRootAndIsChildSelected));
        }

        public T Value
        {
            get { return _currentValue; }
        }

        public VM ViewModel
        {
            get { return _currentViewModel; }
        }

        public ITreeSelector<VM, T> ParentSelector { get; private set; }
        public ITreeRootSelector<VM, T> RootSelector { get; private set; }
        public IEntriesHelper<VM> EntryHelper { get; private set; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    SetIsSelected(value);
                    OnSelected(value);
                }
            }
        }

        public bool IsRoot
        {
            get { return _isRoot; }
            set
            {
                _isRoot = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsRootAndIsChildSelected));
            }
        }

        public virtual bool IsChildSelected
        {
            get { return _selectedValue != null; }
        }

        public virtual bool IsRootAndIsChildSelected
        {
            get { return IsRoot && IsChildSelected; }
        }

        public T SelectedChild
        {
            get { return _selectedValue; }
            set
            {
                SetIsSelected(false);
                OnPropertyChanged(nameof(IsSelected));
                OnChildSelected(value);
                OnPropertyChanged(nameof(IsChildSelected));
            }
        }

        public override string ToString()
        {
            return _currentValue == null ? "" : _currentValue.ToString();
        }

        public void OnSelected(bool selected)
        {
            if (selected)
                ReportChildSelected(new Stack<ITreeSelector<VM, T>>());
            else ReportChildDeselected(new Stack<ITreeSelector<VM, T>>());
        }

        public void OnChildSelected(T newValue)
        {
            if (_selectedValue == null || !_selectedValue.Equals(newValue))
            {
                if (_prevSelected != null)
                {
                    _prevSelected.SetIsSelected(false);
                }

                SetSelectedChild(newValue);

                if (newValue != null)
                {
                    LookupAsync(newValue, SearchNextLevel<VM, T>.LoadSubentriesIfNotLoaded,
                        new TreeLookupProcessor<VM, T>(HierarchicalResult.Related, (hr, p, c) =>
                        {
                            c.IsSelected = true;
                            _prevSelected = c;
                            return true;
                        })).Forget();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}