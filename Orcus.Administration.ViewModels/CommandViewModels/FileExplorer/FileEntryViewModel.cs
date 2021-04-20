using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using Orcus.Administration.Core.Annotations;
using Orcus.Shared.Commands.FileExplorer;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels.FileExplorer
{
    public class FileEntryViewModel : IEntryViewModel, INotifyPropertyChanged
    {
        private readonly FileEntry _fileEntry;
        private readonly IFileSystem _fileSystem;
        private RelayCommand _beginRenameCommand;
        private bool _isInRenameMode;
        private string _name;
        private ImageSource _bigThumbnail;
        private ImageSource _thumbnail;
        private static readonly string[] ArchiveFileExtensions = {".zip", ".tar", ".gz", ".bz2", ".lzw"};

        public FileEntryViewModel(FileEntry fileEntry, IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _fileSystem.FileExplorerEntryRenamed += FileSystemOnFileExplorerEntryRenamed;
            _fileEntry = fileEntry;
            Name = fileEntry.Name;
            Commands = new EntryViewModelCommands(this, fileSystem);
        }

        public EntryType EntryType { get; } = EntryType.File;
        public bool IsLoadingThumbnail { get; set; }
        public bool IsDirectory { get; } = false;

        public string Label => _name;
        public string SortingName => _name;
        public string NameWithoutExtension { get; private set; }

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NameWithoutExtension = Path.GetFileNameWithoutExtension(value);

                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Label));
                    OnPropertyChanged(nameof(NameWithoutExtension));
                    OnPropertyChanged(nameof(SortingName));
                }
            }
        }

        public ImageSource Thumbnail
        {
            get { return _thumbnail; }
            set
            {
                if (_thumbnail != value)
                {
                    _thumbnail = value;
                    OnPropertyChanged();
                }
            }
        }

        public ImageSource BigThumbnail
        {
            get { return _bigThumbnail ?? _thumbnail; }
            set
            {
                if (_bigThumbnail != value)
                {
                    _bigThumbnail = value;
                    OnPropertyChanged();
                }
            }
        }

        public IFileExplorerEntry Value => _fileEntry;
        public EntryViewModelCommands Commands { get; }

        public ImageSource Icon => _fileSystem.ImageProvider.GetFileImage(_fileEntry);
        public string Description => _fileSystem.ImageProvider.GetFileDescription(_fileEntry);
        public long Size => _fileEntry.Size;

        public bool CanBeExtracted
            => ArchiveFileExtensions.Any(x => _fileEntry.Name.EndsWith(x, StringComparison.OrdinalIgnoreCase));

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

        public ICommand BeginRenameCommand
        {
            get
            {
                return _beginRenameCommand ??
                       (_beginRenameCommand = new RelayCommand(parameter => { IsInRenameMode = true; }));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void FileSystemOnFileExplorerEntryRenamed(object sender, EntryRenamedInfo entryRenamedInfo)
        {
            if (Value.Equals(entryRenamedInfo.FileExplorerEntry))
            {
                Name = entryRenamedInfo.NewName;
                if (entryRenamedInfo.FileExplorerEntry != Value)
                {
                    Value.Name = entryRenamedInfo.NewName;
                    Value.Path = entryRenamedInfo.NewPath;
                }
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}