using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Orcus.Administration.Core.Annotations;
using Orcus.Shared.Commands.FileExplorer;
using Sorzus.Wpf.Toolkit;
using Sorzus.Wpf.Toolkit.Converter;

namespace Orcus.Administration.ViewModels.CommandViewModels.FileExplorer
{
    public class ProcessingEntryViewModel : IEntryViewModel, INotifyPropertyChanged
    {
        private readonly IFileSystem _fileSystem;
        private readonly Action _cancelAction;
        private readonly ProcessingEntry _processingEntry;
        private readonly Lazy<string> _uploadingProcessingFileDescription =
            new Lazy<string>(() => (string) Application.Current.Resources["UploadingProcessingFile"]);
        private RelayCommand _cancelCommand;
        private string _description;
        private bool _isInterminate;
        private float _progress;
        private long _size;
        private ImageSource _icon;

        private ProcessingEntryViewModel(ProcessingEntry processingEntry,
            ProcessingEntryActionAdvanced processingEntryActionAdvanced, IFileSystem fileSystem)
        {
            _processingEntry = processingEntry;
            _fileSystem = fileSystem;

            Name = processingEntry.Name;
            Progress = processingEntry.Progress;
            IsInterminate = processingEntry.IsInterminate;
            Size = processingEntry.Size;
            IsDirectory = processingEntry.IsDirectory;
            Action = processingEntryActionAdvanced;
        }

        public ProcessingEntryViewModel(ProcessingEntry processingEntry, Action<ProcessingEntry> cancelAction, IFileSystem fileSystem)
            : this(processingEntry, (ProcessingEntryActionAdvanced) processingEntry.Action, fileSystem)
        {
            _cancelAction = () => cancelAction(processingEntry);

            switch (processingEntry.Action)
            {
                case ProcessingEntryAction.Packing:
                    Description = (string) Application.Current.Resources["PackingFile"];
                    break;
                case ProcessingEntryAction.Extracting:
                    Description = (string) Application.Current.Resources["ExtrackingFile"];
                    break;
                case ProcessingEntryAction.Downloading:
                    Description = (string) Application.Current.Resources["DownloadingFile"];
                    break;
            }
        }

        public ProcessingEntryViewModel(FileTransferTask fileTransferTask, IFileSystem fileSystem) : this(new ProcessingEntry
        {
            Path = fileTransferTask.Path,
            CreationTime = DateTime.Now,
            IsInterminate = true,
            LastAccess = DateTime.Now,
            Size = 0,
            Progress = 0,
            Name = fileTransferTask.Name
        }, ProcessingEntryActionAdvanced.Uploading, fileSystem)
        {
            Description = (string)Application.Current.Resources["Upload"];
            _cancelAction = fileTransferTask.Cancel;
            fileTransferTask.ProgressChanged += FileTransferTaskOnProgressChanged;
        }

        public float Progress
        {
            get { return _progress; }
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsInterminate
        {
            get { return _isInterminate; }
            set
            {
                if (_isInterminate != value)
                {
                    _isInterminate = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool CanCancel => _cancelAction != null;

        public RelayCommand CancelCommand
        {
            get { return _cancelCommand ?? (_cancelCommand = new RelayCommand(parameter => { _cancelAction(); })); }
        }

        public ProcessingEntryActionAdvanced Action { get; }
        public EntryType EntryType { get; } = EntryType.Processing;
        public string Label => Name;
        public string Name { get; }
        public IFileExplorerEntry Value => _processingEntry;
        public string SortingName => Name;

        public ImageSource Icon => _icon ??
                                   (_icon =
                                       _icon =
                                           IsDirectory
                                               ? _fileSystem.ImageProvider.GetFolderImage(Name, 0)
                                               : _fileSystem.ImageProvider.GetFileImage(Name));

        public bool IsDirectory { get; } = false;

        public string Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged();
                }
            }
        }

        public long Size
        {
            get { return _size; }
            set
            {
                if (_size != value)
                {
                    _size = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsInRenameMode { get; set; }
        public ICommand BeginRenameCommand { get; }
        public EntryViewModelCommands Commands { get; }

        //this is just always null
        public ImageSource Thumbnail { get; } = null;
        public ImageSource BigThumbnail { get; } = null;
        public event PropertyChangedEventHandler PropertyChanged;

        public void Update(ProcessingEntryUpdate processingEntryUpdate)
        {
            Size = processingEntryUpdate.Size;
            Progress = processingEntryUpdate.Progress;
            ((ProcessingEntry) Value).Size = processingEntryUpdate.Size;
        }

        private void FileTransferTaskOnProgressChanged(object sender, EventArgs eventArgs)
        {
            var fileTransferTask = (FileTransferTask) sender;
            IsInterminate = false;
            Size = fileTransferTask.ProcessedSize;
            Progress = (float) fileTransferTask.Progress;
            Description = string.Format(_uploadingProcessingFileDescription.Value,
                FormatBytesConverter.BytesToString((long) fileTransferTask.Speed), fileTransferTask.EstimatedTime);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum ProcessingEntryActionAdvanced
    {
        Packing,
        Extracting,
        Downloading,
        Uploading = 16
    }
}