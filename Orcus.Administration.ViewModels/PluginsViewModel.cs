using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Orcus.Administration.Core.Plugins;
using Orcus.Administration.Core.Plugins.Web;
using Orcus.Administration.ViewModels.Plugins;
using Orcus.Administration.ViewModels.ViewInterface;
using Orcus.Plugins;
using Orcus.Shared.Utilities;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels
{
    public class PluginsViewModel : PropertyChangedBase
    {
        public delegate Task<List<PublicPluginData>> DownloadPluginInformationDelegate(IWindow window);

        public static IWebServerConnection WebServerConnection;

        public static DownloadPluginInformationDelegate DownloadPluginInformation;
        private string _browseCollectionSearchText;
        private ICollectionView _browseCollectionView;
        private double _currentProgress;
        private RelayCommand _downloadPluginCommand;
        private string _installedCollectionSearchText;
        private ICollectionView _installedCollectionView;
        private ObservableCollection<IPluginViewModel> _installedPlugins;
        private bool _isDoingSomething;
        private bool _isLoading = true;
        private RelayCommand _openUrlCommand;
        private ObservableCollection<PublicPluginViewModel> _publicPlugins;
        private RelayCommand _uninstallPluginCommand;
        private ICollectionView _updateCollectionView;
        private RelayCommand _updatePluginCommand;

        public PluginsViewModel()
        {
            Load();
        }

        public ObservableCollection<PublicPluginViewModel> PublicPlugins
        {
            get { return _publicPlugins; }
            set { SetProperty(value, ref _publicPlugins); }
        }

        public ObservableCollection<IPluginViewModel> InstalledPlugins
        {
            get { return _installedPlugins; }
            private set { SetProperty(value, ref _installedPlugins); }
        }

        public bool RefreshAdministration { get; private set; }

        public string BrowseCollectionSearchText
        {
            get { return _browseCollectionSearchText; }
            set
            {
                if (SetProperty(value, ref _browseCollectionSearchText))
                    BrowseCollectionView.Refresh();
            }
        }

        public string InstalledCollectionSearchText
        {
            get { return _installedCollectionSearchText; }
            set
            {
                if (SetProperty(value, ref _installedCollectionSearchText))
                    InstalledCollectionView.Refresh();
            }
        }

        public ICollectionView BrowseCollectionView
        {
            get { return _browseCollectionView; }
            private set { SetProperty(value, ref _browseCollectionView); }
        }

        public ICollectionView InstalledCollectionView
        {
            get { return _installedCollectionView; }
            set { SetProperty(value, ref _installedCollectionView); }
        }

        public ICollectionView UpdateCollectionView
        {
            get { return _updateCollectionView; }
            set { SetProperty(value, ref _updateCollectionView); }
        }

        public RelayCommand OpenUrlCommand
        {
            get
            {
                return _openUrlCommand ??
                       (_openUrlCommand = new RelayCommand(parameter =>
                       {
                           try
                           {
                               Process.Start((string) parameter);
                           }
                           catch (Exception)
                           {
                               // ignored
                           }
                       }));
            }
        }

        public RelayCommand DownloadPluginCommand
        {
            get
            {
                return _downloadPluginCommand ?? (_downloadPluginCommand = new RelayCommand(async parameter =>
                {
                    var pluginInfo = (PublicPluginViewModel) parameter;
                    if (pluginInfo == null)
                        return;

                    IsDoingSomething = true;

                    var pluginDownloadFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins",
                        pluginInfo.Name.Replace(" ", null) + ".orcplg");
                    var file = new FileInfo(FileExtensions.MakeUnique(pluginDownloadFile));
                    Directory.CreateDirectory(file.DirectoryName);

                    try
                    {
                        await DownloadPlugin(pluginInfo, file.FullName);

                        pluginInfo.IsInstalled = true;
                        pluginInfo.IsUpdateAvailable = false;
                        pluginInfo.LocalPath = file.FullName;

                        InstalledPlugins.Add(pluginInfo);
                    }
                    catch (Exception ex)
                    {
                        WindowServiceInterface.Current.ShowMessageBox(ex.Message,
                            (string) Application.Current.Resources["Error"], MessageBoxButton.OK, MessageBoxImage.Error);

                        if (file.Exists)
                            file.Delete();
                    }

                    IsDoingSomething = false;
                }));
            }
        }

        public RelayCommand UninstallPluginCommand
        {
            get
            {
                return _uninstallPluginCommand ?? (_uninstallPluginCommand = new RelayCommand(parameter =>
                {
                    var pluginInfo = (IPluginViewModel) parameter;
                    if (pluginInfo == null)
                        return;

                    IsDoingSomething = true;

                    try
                    {
                        UninstallPlugin(pluginInfo);
                        pluginInfo.IsInstalled = false;
                        pluginInfo.IsUpdateAvailable = false;
                        var browsePlugin = PublicPlugins.FirstOrDefault(x => x.Guid == pluginInfo.Guid);
                        if (browsePlugin != null)
                        {
                            browsePlugin.IsInstalled = false;
                            browsePlugin.IsUpdateAvailable = false;
                        }

                        UpdateCollectionView.Refresh();
                        InstalledPlugins.Remove(pluginInfo);
                    }
                    catch (Exception ex)
                    {
                        WindowServiceInterface.Current.ShowMessageBox(ex.Message,
                            (string) Application.Current.Resources["Error"], MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    IsDoingSomething = false;
                }));
            }
        }

        public RelayCommand UpdatePluginCommand
        {
            get
            {
                return _updatePluginCommand ?? (_updatePluginCommand = new RelayCommand(async parameter =>
                {
                    var pluginInfo = (PublicPluginViewModel) parameter;
                    if (pluginInfo == null || !pluginInfo.IsInstalled)
                        return;

                    pluginInfo = PublicPlugins.First(x => x.Guid == pluginInfo.Guid);

                    CurrentProgress = 0;
                    IsDoingSomething = true;

                    var file = Path.GetTempFileName();
                    File.Delete(file);

                    try
                    {
                        await DownloadPlugin(pluginInfo, file);
                    }
                    catch (Exception ex)
                    {
                        if (File.Exists(file))
                            File.Delete(file);

                        WindowServiceInterface.Current.ShowMessageBox(ex.Message,
                            (string) Application.Current.Resources["Error"], MessageBoxButton.OK, MessageBoxImage.Error);

                        IsDoingSomething = false;
                        return;
                    }

                    try
                    {
                        UninstallPlugin(pluginInfo);
                        File.Move(file, pluginInfo.LocalPath);

                        pluginInfo.IsUpdateAvailable = false;
                        var plugin = InstalledPlugins.FirstOrDefault(x => x.Guid == pluginInfo.Guid);

                        if (plugin != null)
                            InstalledPlugins.Remove(plugin);

                        InstalledPlugins.Add(pluginInfo);
                        UpdateCollectionView.Refresh();
                    }
                    catch (Exception ex)
                    {
                        WindowServiceInterface.Current.ShowMessageBox(ex.Message,
                            (string) Application.Current.Resources["Error"], MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    IsDoingSomething = false;
                }));
            }
        }

        public double CurrentProgress
        {
            get { return _currentProgress; }
            set { SetProperty(value, ref _currentProgress); }
        }

        public bool IsDoingSomething
        {
            get { return _isDoingSomething; }
            set { SetProperty(value, ref _isDoingSomething); }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(value, ref _isLoading); }
        }

        private void UninstallPlugin(IPluginViewModel publicPluginViewModel)
        {
            var currentPlugin =
                PluginManager.Current.LoadedPlugins.FirstOrDefault(x => x.PluginInfo.Guid == publicPluginViewModel.Guid);

            if (currentPlugin != null)
            {
                PluginManager.Current.RemovePlugin(currentPlugin);
                RefreshAdministration = true;
            }

            var file = new FileInfo(publicPluginViewModel.LocalPath);
            if (file.Exists)
                file.Delete();
        }

        private async Task DownloadPlugin(PublicPluginViewModel pluginViewModel, string fileName)
        {
            await WebServerConnection.DownloadPlugin(pluginViewModel.Guid, fileName, d => CurrentProgress = d);

            try
            {
                PluginManager.Current.AddPlugin(fileName);
            }
            catch (Exception)
            {
                File.Delete(fileName);
                throw;
            }

            if (pluginViewModel.Type == PluginType.Administration)
                RefreshAdministration = true;
        }

        private async void Load()
        {
            var downloadedPlugins = await DownloadPluginInformation(WindowServiceInterface.Current.GetCurrentWindow());
            var installedPlugins = PluginManager.Current.LoadedPlugins;

            //Browse List
            PublicPlugins = new ObservableCollection<PublicPluginViewModel>(downloadedPlugins.Select(
                x =>
                    new PublicPluginViewModel(x)).OrderByDescending(x => x.Downloads).ToList());

            foreach (var installedPluginInfo in installedPlugins)
            {
                var publicPluginInfo = PublicPlugins.FirstOrDefault(x => x.Guid == installedPluginInfo.PluginInfo.Guid);
                if (publicPluginInfo == null)
                    continue;

                publicPluginInfo.IsInstalled = true;
                publicPluginInfo.IsUpdateAvailable = publicPluginInfo.Version > installedPluginInfo.PluginInfo.Version;
                publicPluginInfo.LocalPath = installedPluginInfo.Path;
            }

            BrowseCollectionView = CollectionViewSource.GetDefaultView(PublicPlugins);
            BrowseCollectionView.Filter += BrowseCollectionFilter;

            //Installed List
            InstalledPlugins = new ObservableCollection<IPluginViewModel>();

            foreach (var installedPlugin in installedPlugins)
            {
                var publicPlugin = PublicPlugins.FirstOrDefault(x => x.Guid == installedPlugin.PluginInfo.Guid);
                if (publicPlugin == null)
                    InstalledPlugins.Add(new InstalledUnknownPluginViewModel(installedPlugin));
                else
                {
                    var plugin = (PublicPluginViewModel) publicPlugin.Clone();
                    plugin.Version = installedPlugin.PluginInfo.Version;
                    plugin.Description = installedPlugin.PluginInfo.Description;
                    InstalledPlugins.Add(plugin);
                }
            }

            InstalledCollectionView = CollectionViewSource.GetDefaultView(InstalledPlugins);
            InstalledCollectionView.Filter += InstalledCollectionFilter;

            //Update List
            UpdateCollectionView = new CollectionViewSource {Source = PublicPlugins}.View;
            UpdateCollectionView.Filter += UpdateFilter;
            IsLoading = false;
        }

        private bool UpdateFilter(object o)
        {
            var plugin = (PublicPluginViewModel) o;
            return plugin.IsUpdateAvailable;
        }

        private bool InstalledCollectionFilter(object o)
        {
            var plugin = (IPluginViewModel) o;
            return string.IsNullOrWhiteSpace(InstalledCollectionSearchText) ||
                   plugin.Name.IndexOf(InstalledCollectionSearchText, StringComparison.OrdinalIgnoreCase) > -1 ||
                   plugin.Author.IndexOf(InstalledCollectionSearchText, StringComparison.OrdinalIgnoreCase) > -1;
        }

        private bool BrowseCollectionFilter(object o)
        {
            var plugin = (PublicPluginViewModel) o;
            return string.IsNullOrWhiteSpace(BrowseCollectionSearchText) ||
                   plugin.Name.IndexOf(BrowseCollectionSearchText, StringComparison.OrdinalIgnoreCase) > -1 ||
                   plugin.Author.IndexOf(BrowseCollectionSearchText, StringComparison.OrdinalIgnoreCase) > -1 ||
                   plugin.Tags.IndexOf(BrowseCollectionSearchText, StringComparison.OrdinalIgnoreCase) > -1;
        }
    }
}