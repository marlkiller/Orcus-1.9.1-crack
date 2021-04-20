using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Orcus.Plugins;
using OrcusPluginStudio.Core;
using OrcusPluginStudio.Core.Settings;
using OrcusPluginStudio.Core.Test;
using OrcusPluginStudio.Views;
using Sorzus.Wpf.Toolkit;

namespace OrcusPluginStudio.ViewModels
{
    public class MainViewModel : PropertyChangedBase
    {
        private RelayCommand _buildCommand;
        private RelayCommand _editPropertiesCommand;
        private RelayCommand _exitCommand;
        private LibraryWatcher _library1Watcher;
        private LibraryWatcher _library2Watcher;

        private RelayCommand _openNewCommand;
        private string _path;
        private string _pluginName;

        private RelayCommand _saveAsCommand;
        private RelayCommand _saveFileCommand;
        private RelayCommand _selectLibrary1Command;
        private RelayCommand _selectLibrary2Command;
        private RelayCommand _selectThumbnailCommand;

        private Tester _tester;
        private BitmapImage _thumbnailImage;

        public MainViewModel(OrcusPluginProject project, string path)
        {
            _path = path;
            PluginProject = project;
            RequireTwoLibraries = project.PluginType == PluginType.CommandFactory ||
                                  project.PluginType == PluginType.CommandView;

            if (!string.IsNullOrEmpty(project.ThumbnailPath) && File.Exists(project.ThumbnailPath))
                ThumbnailImage = new BitmapImage(new Uri(project.ThumbnailPath, UriKind.Absolute));

            if (!string.IsNullOrEmpty(project.Library1Path))
            {
                _library1Watcher = new LibraryWatcher(project.Library1Path);
                _library1Watcher.ReloadFile += LibraryWatcher_ReloadFile;
            }
            if (RequireTwoLibraries && !string.IsNullOrEmpty(project.Library2Path))
            {
                _library2Watcher = new LibraryWatcher(project.Library2Path);
                _library2Watcher.ReloadFile += LibraryWatcher_ReloadFile;
            }
            ReloadTest();
        }

        public bool RequireTwoLibraries { get; }
        public OrcusPluginProject PluginProject { get; }

        public string PluginName
        {
            get { return _pluginName; }
            set { SetProperty(value, ref _pluginName); }
        }

        public BitmapImage ThumbnailImage
        {
            get { return _thumbnailImage; }
            set { SetProperty(value, ref _thumbnailImage); }
        }

        public RelayCommand SelectLibrary1Command
        {
            get
            {
                return _selectLibrary1Command ?? (_selectLibrary1Command = new RelayCommand(parameter =>
                {
                    string path;
                    if (GetLibraryPath(out path))
                    {
                        PluginProject.Library1Path = path;
                        OnPropertyChanged(nameof(PluginProject));
                        PluginProject.WriteToFile(_path);

                        _library1Watcher?.Dispose();
                        _library1Watcher = new LibraryWatcher(path);
                        _library1Watcher.ReloadFile += LibraryWatcher_ReloadFile;
                        ReloadTest();
                    }
                }));
            }
        }

        public RelayCommand BuildCommand
        {
            get
            {
                return _buildCommand ?? (_buildCommand = new RelayCommand(parameter =>
                {
                    if (string.IsNullOrWhiteSpace(PluginProject.PluginInformation.Name))
                    {
                        MessageBoxEx.Show(Application.Current.MainWindow, "Name can't empty", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(PluginProject.PluginInformation.Description))
                    {
                        MessageBoxEx.Show(Application.Current.MainWindow, "Description can't be empty", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(PluginProject.PluginInformation.Author))
                    {
                        MessageBoxEx.Show(Application.Current.MainWindow, "Author can't be empty", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(PluginProject.PluginInformation.AuthorUrl))
                    {
                        MessageBoxEx.Show(Application.Current.MainWindow, "Author url can't be empty", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (PluginProject.PluginInformation.Guid == Guid.Empty)
                    {
                        MessageBoxEx.Show(Application.Current.MainWindow, "Guid can't be empty", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(PluginProject.Library1Path))
                    {
                        MessageBoxEx.Show(Application.Current.MainWindow, "Please select the path to your library",
                            "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    if (!File.Exists(PluginProject.Library1Path))
                    {
                        MessageBoxEx.Show(Application.Current.MainWindow, "The library does not exist", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (RequireTwoLibraries && string.IsNullOrWhiteSpace(PluginProject.Library2Path))
                    {
                        MessageBoxEx.Show(Application.Current.MainWindow, "Please select the path to your library 2",
                            "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    if (RequireTwoLibraries && !File.Exists(PluginProject.Library2Path))
                    {
                        MessageBoxEx.Show(Application.Current.MainWindow, "The library 2 does not exist", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (string.IsNullOrEmpty(PluginProject.ThumbnailPath))
                    {
                        MessageBoxEx.Show(Application.Current.MainWindow, "Please select a thumbnail", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    if (!File.Exists(PluginProject.ThumbnailPath))
                    {
                        MessageBoxEx.Show(Application.Current.MainWindow, "The thumbnail does not exist", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var sfd = new SaveFileDialog
                    {
                        Filter = "Orcus Plugin|*.orcplg|All files|*.*",
                        FileName = PluginProject.PluginInformation.Name.Replace(" ", null)
                    };

                    if (sfd.ShowDialog(Application.Current.MainWindow) == true)
                    {
                        Builder.BuildPlugin(PluginProject, sfd.FileName);
                        Process.Start("explorer.exe", $"/select, \"{sfd.FileName}\"");
                    }
                }));
            }
        }

        public RelayCommand SelectLibrary2Command
        {
            get
            {
                return _selectLibrary2Command ?? (_selectLibrary2Command = new RelayCommand(parameter =>
                {
                    string path;
                    if (GetLibraryPath(out path))
                    {
                        PluginProject.Library2Path = path;
                        OnPropertyChanged(nameof(PluginProject));
                        PluginProject.WriteToFile(_path);

                        _library2Watcher?.Dispose();
                        _library2Watcher = new LibraryWatcher(path);
                        _library2Watcher.ReloadFile += LibraryWatcher_ReloadFile;
                        ReloadTest();
                    }
                }));
            }
        }

        public RelayCommand SelectThumbnailCommand
        {
            get
            {
                return _selectThumbnailCommand ?? (_selectThumbnailCommand = new RelayCommand(parameter =>
                {
                    var ofd = new OpenFileDialog {Filter = "Image|*.png;*.jpg"};
                    if (ofd.ShowDialog(Application.Current.MainWindow) == true)
                    {
                        try
                        {
                            ThumbnailImage = new BitmapImage(new Uri(ofd.FileName, UriKind.Absolute));
                        }
                        catch (Exception)
                        {
                            return;
                        }
                        PluginProject.ThumbnailPath = ofd.FileName;
                        PluginProject.WriteToFile(_path);
                    }
                }));
            }
        }

        public RelayCommand SaveFileCommand
        {
            get
            {
                return _saveFileCommand ??
                       (_saveFileCommand = new RelayCommand(parameter => { PluginProject.WriteToFile(_path); }));
            }
        }

        public RelayCommand EditPropertiesCommand
        {
            get
            {
                return _editPropertiesCommand ?? (_editPropertiesCommand = new RelayCommand(parameter =>
                {
                    var window = new ProjectPropertiesWindow(PluginProject.PluginInformation)
                    {
                        Owner = Application.Current.MainWindow
                    };
                    window.ShowDialog();
                    PluginProject.WriteToFile(_path);
                }));
            }
        }

        public RelayCommand ExitCommand
        {
            get
            {
                return _exitCommand ??
                       (_exitCommand = new RelayCommand(parameter => { Application.Current.MainWindow.Close(); }));
            }
        }

        public RelayCommand SaveAsCommand
        {
            get
            {
                return _saveAsCommand ?? (_saveAsCommand = new RelayCommand(parameter =>
                {
                    var sfd = new SaveFileDialog
                    {
                        Filter = "Orcus Plugin Project|*.opp",
                        AddExtension = true
                    };

                    if (sfd.ShowDialog(Application.Current.MainWindow) == true)
                    {
                        PluginProject.WriteToFile(sfd.FileName);
                        OrcusPluginStudioSettings.Current.RecentEntries.First(x => x.Path == _path).Path =
                            sfd.FileName;
                        _path = sfd.FileName;
                    }
                }));
            }
        }

        public RelayCommand OpenNewCommand
        {
            get
            {
                return _openNewCommand ??
                       (_openNewCommand =
                           new RelayCommand(parameter => { NewOpenEvent?.Invoke(this, EventArgs.Empty); }));
            }
        }

        public Tester Tester
        {
            get { return _tester; }
            set { SetProperty(value, ref _tester); }
        }

        private void LibraryWatcher_ReloadFile(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(ReloadTest));
        }

        public event EventHandler NewOpenEvent;

        public void ReloadTest()
        {
            Tester?.Dispose();
            Tester = new Tester();
            if (string.IsNullOrWhiteSpace(PluginProject.Library1Path))
            {
                Tester.TestResultEntries.Add(new TestResultEntry {Failed = true, Message = "Please select the library"});
                return;
            }

            if (RequireTwoLibraries && string.IsNullOrWhiteSpace(PluginProject.Library2Path))
            {
                Tester.TestResultEntries.Add(new TestResultEntry {Failed = true, Message = "Please select library 2"});
                return;
            }

            if (!File.Exists(PluginProject.Library1Path))
            {
                Tester.TestResultEntries.Add(new TestResultEntry
                {
                    Failed = true,
                    Message = $"The file \"{PluginProject.Library1Path}\" does not exist"
                });
                return;
            }

            if (RequireTwoLibraries && !File.Exists(PluginProject.Library2Path))
            {
                Tester.TestResultEntries.Add(new TestResultEntry
                {
                    Failed = true,
                    Message = $"The file \"{PluginProject.Library2Path}\" does not exist"
                });
                return;
            }

            Tester = null;
            var tester = new Tester
            {
                Library1 = PluginProject.Library1Path,
                Library2 = PluginProject.Library2Path
            };

            tester.Test(PluginProject.PluginType);
            Tester = tester;
        }

        private bool GetLibraryPath(out string path)
        {
            path = null;
            var ofd = new OpenFileDialog {Filter = "Library|*.dll"};
            if (ofd.ShowDialog(Application.Current.MainWindow) == true)
            {
                path = ofd.FileName;
                return true;
            }

            return false;
        }
    }
}