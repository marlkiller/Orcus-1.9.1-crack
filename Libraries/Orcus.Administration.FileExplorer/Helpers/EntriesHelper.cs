using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Orcus.Administration.FileExplorer.Models;
using Orcus.Administration.FileExplorer.Utilities;

namespace Orcus.Administration.FileExplorer.Helpers
{
    public class EntriesHelper<VM> : IEntriesHelper<VM>, INotifyPropertyChanged
    {
        private readonly AsyncLock _loadingLock = new AsyncLock();
        private bool _clearBeforeLoad;
        private bool _isExpanded;
        //private bool _isLoading = false;
        private bool _isLoaded;
        private bool _isLoading;

        private CancellationTokenSource _lastCancellationToken = new CancellationTokenSource();
        private DateTime _lastRefreshTimeUtc = DateTime.MinValue;
        protected Func<bool, object, Task<IEnumerable<VM>>> _loadSubEntryFunc;
        private IEnumerable<VM> _subItemList = new List<VM>();
        private ObservableCollection<VM> _subItems;

        public EntriesHelper(Func<bool, object, Task<IEnumerable<VM>>> loadSubEntryFunc)
        {
            _loadSubEntryFunc = loadSubEntryFunc;

            All = new FastObservableCollection<VM>();
            All.Add(default(VM));
        }

        public EntriesHelper(Func<bool, Task<IEnumerable<VM>>> loadSubEntryFunc)
            : this((b, __) => loadSubEntryFunc(b))
        {
        }

        public EntriesHelper(Func<Task<IEnumerable<VM>>> loadSubEntryFunc)
            : this(_ => loadSubEntryFunc())
        {
        }

        public EntriesHelper(params VM[] entries)
        {
            _isLoaded = true;
            All = new FastObservableCollection<VM>();
            _subItemList = entries;
            (All as FastObservableCollection<VM>).AddItems(entries);
            //foreach (var entry in entries)
            //    All.Add(entry);
        }

        public bool ClearBeforeLoad
        {
            get { return _clearBeforeLoad; }
            set { _clearBeforeLoad = value; }
        }

        public DateTime LastRefreshTimeUtc
        {
            get { return _lastRefreshTimeUtc; }
        }

        public ObservableCollection<VM> All
        {
            get { return _subItems; }
            private set { _subItems = value; }
        }

        public AsyncLock LoadingLock
        {
            get { return _loadingLock; }
        }

        public async Task UnloadAsync()
        {
            _lastCancellationToken.Cancel(); //Cancel previous load.                
            using (var releaser = await _loadingLock.LockAsync())
            {
                _subItemList = new List<VM>();
                All.Clear();
                _isLoaded = false;
            }
        }

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value && !_isExpanded)
                    LoadAsync().Forget();
                _isExpanded = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoaded
        {
            get { return _isLoaded; }
            set
            {
                _isLoaded = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public event EventHandler EntriesChanged;


        public IEnumerable<VM> AllNonBindable
        {
            get { return _subItemList; }
        }

        public async Task<IEnumerable<VM>> LoadAsync(UpdateMode updateMode = UpdateMode.Replace, bool force = false,
            object parameter = null)
        {
            if (_loadSubEntryFunc != null) //Ignore if contructucted using entries but not entries func
            {
                _lastCancellationToken.Cancel(); //Cancel previous load.                
                using (var releaser = await _loadingLock.LockAsync())
                {
                    _lastCancellationToken = new CancellationTokenSource();
                    if (!_isLoaded || force)
                    {
                        if (_clearBeforeLoad)
                            All.Clear();

                        try
                        {
                            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
                            IsLoading = true;
                            await _loadSubEntryFunc(_isLoaded, parameter).ContinueWith((prevTask, _) =>
                            {
                                IsLoaded = true;
                                IsLoading = false;
                                if (!prevTask.IsCanceled && !prevTask.IsFaulted)
                                {
                                    SetEntries(updateMode, prevTask.Result.ToArray());
                                    _lastRefreshTimeUtc = DateTime.UtcNow;
                                }
                            }, _lastCancellationToken, scheduler);
                        }
                        catch (InvalidOperationException)
                        {
                        }
                    }
                }
            }
            return _subItemList;
        }


        private void updateEntries(params VM[] viewModels)
        {
            FastObservableCollection<VM> all = All as FastObservableCollection<VM>;
            all.SuspendCollectionChangeNotification();

            var removeItems = all.Where(vm => !viewModels.Contains(vm)).ToList();
            var addItems = viewModels.Where(vm => !all.Contains(vm)).ToList();

            if (addItems.Count == 0 && removeItems.Count == 0)
                return; //nothing to do here

            foreach (var vm in removeItems)
                all.Remove(vm);

            foreach (var vm in addItems)
                all.Add(vm);

            _subItemList = all.ToArray().ToList();
            all.NotifyChanges();

            EntriesChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetEntries(UpdateMode updateMode = UpdateMode.Replace, params VM[] viewModels)
        {
            switch (updateMode)
            {
                case UpdateMode.Update:
                    updateEntries(viewModels);
                    break;
                case UpdateMode.Replace:
                    setEntries(viewModels);
                    break;
                default:
                    throw new NotSupportedException("UpdateMode");
            }
        }

        private void setEntries(params VM[] viewModels)
        {
            _subItemList = viewModels.ToList();
            FastObservableCollection<VM> all = All as FastObservableCollection<VM>;
            all.SuspendCollectionChangeNotification();
            all.Clear();
            all.NotifyChanges();
            all.AddItems(viewModels);
            all.NotifyChanges();

            if (EntriesChanged != null)
                EntriesChanged(this, EventArgs.Empty);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}