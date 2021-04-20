using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using Orcus.Plugins;
using OrcusPluginStudio.Core;
using OrcusPluginStudio.Core.Settings;
using Sorzus.Wpf.Toolkit;

namespace OrcusPluginStudio.ViewModels
{
    public class WelcomeViewModel : PropertyChangedBase
    {
        private readonly Window _baseWindow;
        private readonly string _defaultProjectLocation;
        private bool _pathModifiedByUser;
        private string _projectName;
        private string _projectPath;

        private RelayCommand _removeProjectCommand;
        private RelayCommand _removeRecentEntryCommand;
        private RelayCommand _selectProjectPathCommand;
        private RelayCommand _createNewProjectCommand;
        private RelayCommand _openProjectCommand;
        private RelayCommand _openProjectPathCommand;

        public WelcomeViewModel(Window baseWindow)
        {
            _baseWindow = baseWindow;
            RecentEntries =
                new ObservableCollection<RecentEntry>(
                    OrcusPluginStudioSettings.Current.RecentEntries.Where(x => File.Exists(x.Path))
                        .OrderByDescending(x => x.LastAccessTimestamp));

            _defaultProjectLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Projects");
            _projectPath = _defaultProjectLocation + "\\";
            Directory.CreateDirectory(_defaultProjectLocation);
        }

        public string ProjectPath
        {
            get { return _projectPath; }
            set
            {
                if (SetProperty(value, ref _projectPath))
                    _pathModifiedByUser = true;
            }
        }

        public string ProjectName
        {
            get { return _projectName; }
            set
            {
                if (SetProperty(value, ref _projectName) && !_pathModifiedByUser)
                {
                    _projectPath = Path.Combine(_defaultProjectLocation, ProjectName + ".opp");
                    OnPropertyChanged(nameof(ProjectPath));
                }
            }
        }

        public ObservableCollection<RecentEntry> RecentEntries { get; }

        public OrcusPluginProject PluginProject { get; private set; }
        public string PluginPath { get; private set; }

        public RelayCommand SelectProjectPathCommand
        {
            get
            {
                return _selectProjectPathCommand ?? (_selectProjectPathCommand = new RelayCommand(parameter =>
                {
                    var sfd = new SaveFileDialog
                    {
                        Filter = "Orcus Plugin Project|*.opp",
                        FileName = ProjectName,
                        AddExtension = true,
                        InitialDirectory = _defaultProjectLocation
                    };
                    if (!string.IsNullOrEmpty(ProjectPath))
                    {
                        try
                        {
                            sfd.FileName = Path.GetFileName(ProjectPath);
                            sfd.InitialDirectory = Path.GetDirectoryName(ProjectPath);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }

                    if (sfd.ShowDialog(_baseWindow) == true)
                        ProjectPath = sfd.FileName;
                }));
            }
        }

        public RelayCommand CreateNewProjectCommand
        {
            get
            {
                return _createNewProjectCommand ?? (_createNewProjectCommand = new RelayCommand(parameter =>
                {
                    var type = (PluginType) parameter;

                    PluginProject = new OrcusPluginProject
                    {
                        PluginType = type,
                        PluginInformation =
                            new PluginInformation
                            {
                                Guid = Guid.NewGuid(),
                                Version = new PluginVersion(1, 0),
                                Name = ProjectName
                            }
                    };

                    Directory.CreateDirectory(Path.GetDirectoryName(ProjectPath));
                    PluginProject.WriteToFile(ProjectPath);
                    PluginPath = ProjectPath;

                    OrcusPluginStudioSettings.Current.RecentEntries.Add(new RecentEntry
                    {
                        Name = ProjectName,
                        Path = ProjectPath,
                        LastAccessTimestamp = DateTime.Now
                    });
                    OrcusPluginStudioSettings.Current.Save();

                    _baseWindow.DialogResult = true;
                }));
            }
        }

        public RelayCommand RemoveRecentEntryCommand
        {
            get
            {
                return _removeRecentEntryCommand ?? (_removeRecentEntryCommand = new RelayCommand(parameter =>
                {
                    var entry = parameter as RecentEntry;
                    if (entry == null)
                        return;

                    RecentEntries.Remove(entry);
                    OrcusPluginStudioSettings.Current.RecentEntries.Remove(entry);
                    OrcusPluginStudioSettings.Current.Save();
                }));
            }
        }

        public RelayCommand RemoveProjectCommand
        {
            get
            {
                return _removeProjectCommand ?? (_removeProjectCommand = new RelayCommand(parameter =>
                {
                    var entry = parameter as RecentEntry;
                    if (entry == null)
                        return;

                    RecentEntries.Remove(entry);
                    OrcusPluginStudioSettings.Current.RecentEntries.Remove(entry);
                    OrcusPluginStudioSettings.Current.Save();
                    try
                    {
                        File.Delete(entry.Path);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Exception when trying to delete the project: " + ex.Message, "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }));
            }
        }

        public RelayCommand OpenProjectPathCommand
        {
            get
            {
                return _openProjectPathCommand ?? (_openProjectPathCommand = new RelayCommand(parameter =>
                {
                    var entry = parameter as RecentEntry;
                    if (entry == null)
                        return;

                    Process.Start("explorer.exe", $"/select,\"{entry.Path}\"");
                }));
            }
        }

        public RelayCommand OpenProjectCommand
        {
            get
            {
                return _openProjectCommand ?? (_openProjectCommand = new RelayCommand(parameter =>
                {
                    var ofd = new OpenFileDialog
                    {
                        Filter = "Orcus Plugin Project|*.opp|All files|*.*",
                        InitialDirectory = _defaultProjectLocation
                    };

                    if (ofd.ShowDialog(_baseWindow) == true)
                        try
                        {
                            var project = OpenProject(ofd.FileName);
                            var entry =
                                OrcusPluginStudioSettings.Current.RecentEntries.FirstOrDefault(
                                    x => x.Path == ofd.FileName);
                            if (entry == null)
                                OrcusPluginStudioSettings.Current.RecentEntries.Add(
                                    entry =
                                        new RecentEntry
                                        {
                                            Name = project.PluginInformation.Name,
                                            Path = ofd.FileName
                                        });

                            entry.LastAccessTimestamp = DateTime.Now;
                            OrcusPluginStudioSettings.Current.Save();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                }));
            }
        }

        public void RecentItemDoubleClicked(RecentEntry entry)
        {
            if (!File.Exists(entry.Path))
            {
                MessageBox.Show("Project doesn't exist");
                return;
            }

            OpenProject(entry.Path);
            entry.LastAccessTimestamp = DateTime.Now;
            OrcusPluginStudioSettings.Current.Save();
        }

        public OrcusPluginProject OpenProject(string path)
        {
            PluginProject = OrcusPluginProjectUtilities.LoadPluginProject(path);
            PluginPath = path;
            _baseWindow.DialogResult = true;
            return PluginProject;
        }
    }
}