using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Orcus.Administration.Core.Annotations;
using Orcus.Administration.FileExplorer.Helpers;
using Orcus.Administration.FileExplorer.Models;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.FileExplorer;

namespace Orcus.Administration.ViewModels.CommandViewModels.FileExplorer
{
    //Credits go to FileExplorer
    public class DirectoryTreeViewModel : ISupportTreeSelector<DirectoryNodeViewModel, IFileExplorerEntry>, INotifyPropertyChanged
    {
        private readonly IFileSystem _fileSystem;
        private readonly Func<IWindowService> _getWindow;
        private PackedDirectoryEntry[] _rootModels;
        private ObservableCollection<DirectoryNodeViewModel> _autoCompleteEntries;

        public DirectoryTreeViewModel(IFileSystem fileSystem, Func<IWindowService> getWindow)
        {
            _fileSystem = fileSystem;
            _getWindow = getWindow;
            Entries = new EntriesHelper<DirectoryNodeViewModel>();
            Selection = new TreeRootSelector<DirectoryNodeViewModel, IFileExplorerEntry>(Entries)
            {
                Comparers = new[] {new FileExplorerPathComparer()}
            };
        }

        public DirectoryNodeViewModel[] RootViewModels { get; private set; }

        public PackedDirectoryEntry[] RootModels
        {
            get { return _rootModels; }
            set
            {
                _rootModels = value;
                var rootViewModels = value
                    .Select(r => new DirectoryNodeViewModel(this, r, null, _fileSystem, _getWindow)).ToArray();

                RootViewModels = rootViewModels;

                Entries.SetEntries(UpdateMode.Update, rootViewModels);
                OnPropertyChanged();
                OnPropertyChanged(nameof(RootViewModels));
            }
        }

        public async Task InitializeAllRoots(params PackedDirectoryEntry[] includeSubDirectories)
        {
            var list = new List<DirectoryNodeViewModel>(Entries.All);

            foreach (var directoryEntry in includeSubDirectories)
            {
                var entry = Entries.All.First(x => x.Value == directoryEntry);
                await entry.Entries.LoadAsync();
                list.AddRange(entry.Entries.All);
            }

            AutoCompleteEntries = new ObservableCollection<DirectoryNodeViewModel>(list);
        }

        public ObservableCollection<DirectoryNodeViewModel> AutoCompleteEntries
        {
            get { return _autoCompleteEntries; }
            set
            {
                if (_autoCompleteEntries != value)
                {
                    _autoCompleteEntries = value;
                    OnPropertyChanged();
                }
            }
        }

        public IEntriesHelper<DirectoryNodeViewModel> Entries { get; set; }
        public ITreeSelector<DirectoryNodeViewModel, IFileExplorerEntry> Selection { get; set; }

        public async Task SelectAsync(IFileExplorerEntry value)
        {
            await Selection.LookupAsync(value,
                RecrusiveSearch<DirectoryNodeViewModel, IFileExplorerEntry>.LoadSubentriesIfNotLoaded,
                SetSelected<DirectoryNodeViewModel, IFileExplorerEntry>.WhenSelected,
                SetExpanded<DirectoryNodeViewModel, IFileExplorerEntry>.WhenChildSelected);
        }

        public void ExpandRootEntryModels()
        {
            foreach (var rvm in Entries.AllNonBindable)
                rvm.Entries.IsExpanded = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}