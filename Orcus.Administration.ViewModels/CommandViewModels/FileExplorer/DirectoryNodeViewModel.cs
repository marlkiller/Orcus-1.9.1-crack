using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orcus.Administration.Commands.FileExplorer;
using Orcus.Administration.Core.Annotations;
using Orcus.Administration.FileExplorer.Helpers;
using Orcus.Administration.FileExplorer.Models;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.FileExplorer;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels.FileExplorer
{
    public class DirectoryNodeViewModel : ISupportTreeSelector<DirectoryNodeViewModel, IFileExplorerEntry>,
        INotifyPropertyChanged, IIntoViewBringable, IEntryViewModel, IEquatable<IEntryViewModel>
    {
        private readonly IFileSystem _fileSystem;
        private readonly bool _isNodeViewModel;
        private readonly DirectoryTreeViewModel _rootModel;
        private readonly Func<IWindowService> _getWindow;
        private readonly string _orderName;
        private BitmapImage _icon;
        private bool _isBreadcrumExpanded;
        private bool _isBringIntoView;
        private bool _isUpdating;
        private string _label;
        private string _name;
        private bool _isInRenameMode;

        private RelayCommand _toggleIsExpandedCommand;
        private RelayCommand _removeCommand;
        private RelayCommand _updateEntriesCommand;
        private RelayCommand _beginRenameCommand;

        public DirectoryNodeViewModel(PackedDirectoryEntry packedDirectoryEntry, IFileSystem fileSystem, int orderNumber)
            : this(packedDirectoryEntry, fileSystem)
        {
            _orderName = orderNumber.ToString("0000");
        }

        public DirectoryNodeViewModel(PackedDirectoryEntry packedDirectoryEntry, IFileSystem fileSystem)
        {
            Value = packedDirectoryEntry;
            _fileSystem = fileSystem;
            var driveDirectoryEntry = packedDirectoryEntry as DriveDirectoryEntry;
            if (driveDirectoryEntry != null)
            {
                IsDrive = true;
                Size = driveDirectoryEntry.UsedSpace;
                switch (driveDirectoryEntry.DriveType)
                {
                    case DriveDirectoryType.Unknown:
                        Description = (string) Application.Current.Resources["Unknown"];
                        break;
                    case DriveDirectoryType.Removable:
                        Description = (string) Application.Current.Resources["UsbDrive"];
                        break;
                    case DriveDirectoryType.Fixed:
                        Description = (string) Application.Current.Resources["LocalDisk"];
                        break;
                    case DriveDirectoryType.Network:
                        Description = (string) Application.Current.Resources["NetworkDrive"];
                        break;
                    case DriveDirectoryType.CDRom:
                        Description = (string) Application.Current.Resources["CdDrive"];
                        break;
                    case DriveDirectoryType.Ram:
                        Description = "RAM " + (string) Application.Current.Resources["Drive"];
                        break;
                    default:
                        Description = (string) Application.Current.Resources["Drive"];
                        break;
                }
            }
            else
                Description = (string) Application.Current.Resources["Directory"];

            Name = packedDirectoryEntry.Name;
            fileSystem.DirectoryEntriesUpdated += FileSystemOnDirectoryEntriesUpdated;
            fileSystem.FileExplorerEntryRemoved += FileSystemOnFileExplorerEntryRemoved;
            fileSystem.FileExplorerEntryRenamed += FileSystemOnFileExplorerEntryRenamed;
            fileSystem.FileExplorerEntryAdded += FileSystemOnFileExplorerEntryAdded;

            Commands = new EntryViewModelCommands(this, fileSystem);
        }

        public DirectoryNodeViewModel(DirectoryTreeViewModel rootTreeViewModel, PackedDirectoryEntry currentEntry,
            DirectoryNodeViewModel parentModel, IFileSystem fileSystem, Func<IWindowService> getWindow) : this(currentEntry, fileSystem)
        {
            _rootModel = rootTreeViewModel;
            _getWindow = getWindow;

            Parent = parentModel;
            Entries = new EntriesHelper<DirectoryNodeViewModel>(LoadEntriesTask);
            Selection = new TreeSelector<DirectoryNodeViewModel, IFileExplorerEntry>(Value, this,
                parentModel == null ? rootTreeViewModel.Selection : parentModel.Selection, Entries);

            _isNodeViewModel = true;

            if (!Value.HasSubFolder)
                Entries.SetEntries(UpdateMode.Update);
        }

        public EntryType EntryType { get; } = EntryType.Directory;
        public PackedDirectoryEntry Value { get; private set; }
        public string Label => _label ?? (_label = Value.GetLabel());
        public string SortingName => _orderName ?? Label;
        public BitmapImage Icon => _icon ?? (_icon = _fileSystem.ImageProvider.GetFolderImage(Value));
        public string Description { get; }
        public long Size { get; }
        public bool IsDrive { get; }
        public bool IsDirectory { get; } = true;

        public ObservableCollection<DirectoryNodeViewModel> AutoCompleteEntries => Entries.All;
        public DirectoryNodeViewModel Parent { get; private set; }
        public IEntriesHelper<DirectoryNodeViewModel> Entries { get; set; }
        public ITreeSelector<DirectoryNodeViewModel, IFileExplorerEntry> Selection { get; set; }

        IFileExplorerEntry IEntryViewModel.Value => Value;
        ImageSource IEntryViewModel.Icon => Icon;

        public bool IsInRenameMode
        {
            get { return _isInRenameMode; }
            set
            {
                if (_isInRenameMode != value)
                {
                    _isInRenameMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsBreadcrumbExpanded
        {
            get { return _isBreadcrumExpanded; }
            set
            {
                if (value != _isBreadcrumExpanded)
                {
                    if (value)
                        Entries.LoadAsync();

                    _isBreadcrumExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

        public RelayCommand ToogleIsExpandedCommand
        {
            get
            {
                return _toggleIsExpandedCommand ??
                       (_toggleIsExpandedCommand =
                           new RelayCommand(parameter => { Entries.IsExpanded = !Entries.IsExpanded; }));
            }
        }

        public RelayCommand UpdateEntriesCommand
        {
            get
            {
                return _updateEntriesCommand ?? (_updateEntriesCommand = new RelayCommand(async parameter =>
                {
                    _isUpdating = true;
                    var entries = await _fileSystem.GetDirectories(Value, true);
                    _isUpdating = false;
                    if (Entries.All.Count != 0)
                    {
                        foreach (var packedDirectoryEntry in entries)
                        {
                            var existingViewModel = Entries.All.FirstOrDefault(x => x.Value.Equals(packedDirectoryEntry));
                            existingViewModel?.Update(packedDirectoryEntry);
                        }
                    }
                    Entries.SetEntries(UpdateMode.Update, entries.Select(GetViewModel).ToArray());
                }));
            }
        }

        public RelayCommand RemoveCommand
        {
            get
            {
                return _removeCommand ?? (_removeCommand = new RelayCommand(async parameter =>
                {
                    if (
                        _getWindow()
                            .ShowMessageBox(
                                string.Format((string) Application.Current.Resources["AreYouSureRemoveOneDirectory"],
                                    Name),
                                (string) Application.Current.Resources["Warning"],
                                MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
                        return;

                    await _fileSystem.Remove(Value);
                }));
            }
        }

        public ICommand BeginRenameCommand
        {
            get
            {
                return _beginRenameCommand ?? (_beginRenameCommand = new RelayCommand(parameter =>
                {
                    IsInRenameMode = true;
                }));
            }
        }

        public EntryViewModelCommands Commands { get; }

        public bool IsBringIntoView
        {
            get { return _isBringIntoView; }
            set
            {
                if (_isBringIntoView != value)
                {
                    _isBringIntoView = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public async void Update(PackedDirectoryEntry directoryEntry)
        {
            Value = directoryEntry;
            OnPropertyChanged(nameof(Value));
            if (directoryEntry.HasSubFolder && Entries.All.Count == 0)
            {
                await Entries.UnloadAsync();
                Entries.SetEntries(UpdateMode.Replace, new DirectoryNodeViewModel[1]);
            }
            Name = directoryEntry.Name;
        }

        private void FileSystemOnDirectoryEntriesUpdated(object sender, DirectoryEntriesUpdate directoryEntriesUpdate)
        {
            if (!_isNodeViewModel || _isUpdating || Value.Path != directoryEntriesUpdate.DirectoryPath)
                return;

            var directories = directoryEntriesUpdate.Entries.OfType<PackedDirectoryEntry>().ToList();
            Entries.SetEntries(UpdateMode.Update,
                directories.Select(GetViewModel).ToArray());
            Entries.IsLoaded = true;
        }

        private void FileSystemOnFileExplorerEntryRemoved(object sender, IFileExplorerEntry fileExplorerEntry)
        {
            if (_isNodeViewModel && Entries.IsLoaded)
            {
                var viewModel = Entries.All.FirstOrDefault(x => x.Value == fileExplorerEntry);
                if (viewModel != null)
                {
                    if (viewModel.Selection.IsSelected)
                    {
                        viewModel.Selection.IsSelected = false;
                        Selection.IsSelected = true;
                    }
                    Entries.All.Remove(viewModel);
                }
            }
        }

        private void FileSystemOnFileExplorerEntryRenamed(object sender, EntryRenamedInfo entryRenamedInfo)
        {
            if (Value.Equals(entryRenamedInfo.FileExplorerEntry))
            {
                Name = entryRenamedInfo.NewName;
                if (_label == entryRenamedInfo.FileExplorerEntry.Name)
                {
                    _label = entryRenamedInfo.NewName;
                    OnPropertyChanged(nameof(Label));
                }
                if (entryRenamedInfo.FileExplorerEntry != Value)
                    Value.Name = entryRenamedInfo.NewName;
            }
        }

        private void FileSystemOnFileExplorerEntryAdded(object sender, EntryAddedInfo entryAddedInfo)
        {
            if (!_isNodeViewModel)
                return;

            var directoryEntry = entryAddedInfo.AddedEntry as PackedDirectoryEntry;
            if (directoryEntry != null && entryAddedInfo.Path.Equals(Value.Path, StringComparison.OrdinalIgnoreCase))
            {
                if (Entries.IsLoaded)
                {
                    Entries.All.Add(GetViewModel(directoryEntry));
                }
            }
        }

        private async Task<IEnumerable<DirectoryNodeViewModel>> LoadEntriesTask()
        {
            if (!Value.HasSubFolder)
                return new DirectoryNodeViewModel[0];

            _isUpdating = true;
            var entries = await _fileSystem.GetDirectories(Value);
            _isUpdating = false;
            return entries.Select(GetViewModel);
        }

        private DirectoryNodeViewModel GetViewModel(PackedDirectoryEntry directoryEntry)
        {
            return new DirectoryNodeViewModel(_rootModel, directoryEntry, this, _fileSystem, _getWindow);
        }

        public bool Equals(IEntryViewModel other)
        {
            return Equals((object) other);
        }

        public override bool Equals(object obj)
        {
            var otherDirectoryViewModel = obj as DirectoryNodeViewModel;
            if (otherDirectoryViewModel == null)
                return false;

            return otherDirectoryViewModel.Value.Equals(Value);
        }

        protected bool Equals(DirectoryNodeViewModel other)
        {
            return Equals(Value, other.Value) && string.Equals(Description, other.Description) && Size == other.Size;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Value?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (Description?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ Size.GetHashCode();
                return hashCode;
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //this is just always null
        public ImageSource Thumbnail { get; } = null;
        public ImageSource BigThumbnail { get; } = null;
    }
}