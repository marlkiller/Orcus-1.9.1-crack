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
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using Orcus.Administration.Core;
using Orcus.Administration.Core.Build;
using Orcus.Administration.Core.Build.Configuration;
using Orcus.Administration.Core.Plugins;
using Orcus.Administration.Core.Plugins.Wrappers;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.BuildPlugin;
using Orcus.Administration.Resources;
using Orcus.Administration.ViewModels.ClientBuilder;
using Orcus.Administration.ViewModels.ViewInterface;
using Orcus.Plugins.Builder;
using Orcus.Plugins.PropertyGrid;
using Orcus.Shared.Client;
using Orcus.Shared.Core;
using Orcus.Shared.Settings;
using Orcus.Shared.Utilities;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels
{
    public class ClientBuilderViewModel : PropertyChangedBase, IBuilderInfo
    {
        private readonly ObservableCollection<PluginViewModel> _plugins;
        private RelayCommand _buildCommand;
        private BuildLogger _buildLogger;
        private RelayCommand _closeCommand;
        private bool _isBuilding;
        private RelayCommand<BuildConfigurationViewModel> _loadBuildConfigurationCommand;
        private RelayCommand _openConfigurationManagerCommand;
        private RelayCommand _openEulaCommand;
        private RelayCommand _openUrlCommand;
        private string _pluginsSearchText;
        private RelayCommand _saveCurrentConfigurationCommand;

        public ClientBuilderViewModel(ClientConfig clientConfig,
            List<IpAddressInfo> serverIpAddresses) : this()
        {
            AvailableIpAddresses = serverIpAddresses;
            LoadSettings(clientConfig.Settings);

            foreach (var pluginViewModel in _plugins)
                pluginViewModel.IsSelected = false;

            foreach (var pluginResource in clientConfig.PluginResources)
            {
                var plugin = _plugins.FirstOrDefault(x => x.Plugin.PluginInfo.Guid == pluginResource.Guid);
                if (plugin != null)
                    plugin.IsSelected = true;
            }
        }

        public ClientBuilderViewModel(List<IpAddressInfo> serverIpAddresses) : this()
        {
            AvailableIpAddresses = serverIpAddresses;

            foreach (var builderPropertyViewModel in BuilderSettingsManager.BuilderSettings)
            {
                var connectionBuilderSetting = builderPropertyViewModel.BuilderProperty as ConnectionBuilderProperty;
                if (connectionBuilderSetting != null)
                {
                    var primaryIpAddress =
                        AvailableIpAddresses.FirstOrDefault(
                            x => x.Ip != "127.0.0.1") ??
                        AvailableIpAddresses.First();

                    if (primaryIpAddress != null)
                        connectionBuilderSetting.IpAddresses.Add(primaryIpAddress);
                    break;
                }
            }

            foreach (var pluginViewModel in _plugins)
            {
                pluginViewModel.IsSelected =
                    Settings.Current.EnabledPlugins.Contains(pluginViewModel.Plugin.PluginInfo.Guid);
            }
        }

        private ClientBuilderViewModel()
        {
            _plugins =
                new ObservableCollection<PluginViewModel>(
                    PluginManager.Current.LoadedPlugins.Select(x =>
                    {
                        var buildPlugin = x as BuildPlugin;
                        if (buildPlugin != null)
                            return (PluginViewModel) new BuilderPluginViewModel(buildPlugin);

                        var clientPlugin = x as ClientPlugin;
                        if (clientPlugin != null)
                            return new ClientPluginViewModel(clientPlugin);

                        var factoryCommandPlugin = x as FactoryCommandPlugin;
                        if (factoryCommandPlugin != null)
                            return new FactoryPluginViewModel(factoryCommandPlugin);

                        return null;
                    }).Where(x => x != null));

            foreach (var pluginViewModel in _plugins)
            {
                pluginViewModel.OpenSettings += PluginViewModelOnOpenSettings;
                pluginViewModel.SelectedChanged += PluginViewModelOnSelectedChanged;
            }

            Plugins = CollectionViewSource.GetDefaultView(_plugins);
            Plugins.GroupDescriptions.Add(new PropertyGroupDescription("IsBuildPlugin"));
            Plugins.SortDescriptions.Add(new SortDescription("IsBuildPlugin", ListSortDirection.Ascending));
            Plugins.Filter = FilterPlugins;

            BuilderSettingsManager = new BuilderSettingsManager();
            BuilderSettingsManager.BuilderSettingsOverwriteRequest +=
                BuilderSettingsManagerOnBuilderSettingsOverwriteRequest;

            BuildConfigurationManager = new BuildConfigurationManager();
        }

        public bool IsBuilding
        {
            get { return _isBuilding; }
            set { SetProperty(value, ref _isBuilding); }
        }

        public BuildLogger BuildLogger
        {
            get { return _buildLogger; }
            set { SetProperty(value, ref _buildLogger); }
        }

        public string PluginsSearchText
        {
            get { return _pluginsSearchText; }
            set
            {
                if (SetProperty(value, ref _pluginsSearchText))
                    Plugins.Refresh();
            }
        }

        public BuilderSettingsManager BuilderSettingsManager { get; }
        public ICollectionView Plugins { get; }
        public BuildConfigurationManager BuildConfigurationManager { get; }

        public RelayCommand OpenUrlCommand
        {
            get
            {
                return _openUrlCommand ?? (_openUrlCommand = new RelayCommand(parameter =>
                {
                    var url = parameter as string;
                    if (!string.IsNullOrWhiteSpace(url))
                        Process.Start(url);
                }));
            }
        }

        public RelayCommand BuildCommand
        {
            get { return _buildCommand ?? (_buildCommand = new RelayCommand(parameter => { BuildClient(); })); }
        }

        public RelayCommand OpenConfigurationManagerCommand
        {
            get
            {
                return _openConfigurationManagerCommand ??
                       (_openConfigurationManagerCommand = new RelayCommand(parameter =>
                       {
                           var configurationManagerViewModel =
                               new ConfigurationManagerViewModel(BuildConfigurationManager);
                           if (WindowServiceInterface.Current.OpenWindowDialog(configurationManagerViewModel) == true)
                               LoadBuildConfigurationCommand.Execute(configurationManagerViewModel.BuildConfiguration);
                       }));
            }
        }

        public RelayCommand<BuildConfigurationViewModel> LoadBuildConfigurationCommand
        {
            get
            {
                return _loadBuildConfigurationCommand ??
                       (_loadBuildConfigurationCommand = new RelayCommand<BuildConfigurationViewModel>(parameter =>
                       {
                           var configuration = parameter.BuildConfigurationInfo.BuildConfiguration;
                           LoadSettings(configuration.Settings.Where(x => !(x is PluginSetting)));

                           foreach (var pluginViewModel in _plugins)
                           {
                               if (configuration.Plugins.Any(x => x.Guid == pluginViewModel.Plugin.PluginInfo.Guid))
                               {
                                   pluginViewModel.IsSelected = true;

                                   if (pluginViewModel.PluginSettings == PluginSettings.PropertyGrid)
                                   {
                                       var pluginSettings =
                                           configuration.Plugins.OfType<PluginSetting>()
                                               .FirstOrDefault(x => x.PluginId == pluginViewModel.Plugin.PluginInfo.Guid);

                                       if (pluginSettings != null)
                                           PropertyGridExtensions.InitializeProperties(pluginViewModel.PluginObject,
                                               pluginSettings.Properties);
                                   }
                                   else if (pluginViewModel.PluginSettings == PluginSettings.BuilderSettings)
                                   {
                                       var pluginSettings =
                                           configuration.Settings.OfType<PluginSetting>()
                                               .Where(x => x.PluginId == pluginViewModel.Plugin.PluginInfo.Guid)
                                               .ToList();

                                       if (pluginSettings.Count > 0)
                                           BuilderSettingsManager.UpdatePluginSettings(pluginViewModel.Plugin,
                                               pluginSettings);
                                   }
                               }
                               else pluginViewModel.IsSelected = false;
                           }

                           BuildConfigurationManager.CurrentBuildConfiguration = parameter;
                       }));
            }
        }

        public RelayCommand SaveCurrentConfigurationCommand
        {
            get
            {
                return _saveCurrentConfigurationCommand ??
                       (_saveCurrentConfigurationCommand = new RelayCommand(parameter =>
                       {
                           var inputViewModel =
                               new InputTextViewModel(BuildConfigurationManager.CurrentBuildConfiguration?.Name,
                                   (string) Application.Current.Resources["Name"],
                                   (string) Application.Current.Resources["Save"]);

                           if (
                               WindowServiceInterface.Current.OpenWindowDialog(inputViewModel,
                                   (string) Application.Current.Resources["SaveConfiguration"]) != true)
                               return;

                           var buildConfiguration = new BuildConfiguration
                           {
                               Name = inputViewModel.Text,
                               Plugins = new List<BuildConfigurationPlugin>(),
                               Settings =
                                   BuilderSettingsManager.BuilderSettings.Where(x => !x.IsFromPlugin).Select(
                                       x => x.BuilderProperty.ToClientSetting())
                                       .ToList()
                           };

                           foreach (var pluginViewModel in _plugins.Where(x => x.IsSelected))
                           {
                               buildConfiguration.Plugins.Add(new BuildConfigurationPlugin
                               {
                                   Name = pluginViewModel.Plugin.PluginInfo.Name,
                                   Guid = pluginViewModel.Plugin.PluginInfo.Guid,
                                   Type =
                                       pluginViewModel.IsBuildPlugin
                                           ? PluginSettingType.BuildPlugin
                                           : PluginSettingType.ClientPlugin,
                                   Version = pluginViewModel.Plugin.PluginInfo.Version
                               });

                               var providesProperties = pluginViewModel.PluginObject as IProvideEditableProperties;
                               if (providesProperties != null)
                               {
                                   buildConfiguration.Settings.Add(new PluginSetting
                                   {
                                       PluginId = pluginViewModel.Plugin.PluginInfo.Guid,
                                       Properties =
                                           new List<PropertyNameValue>(
                                               providesProperties.Properties.Select(x => x.ToPropertyNameValue())),
                                       SettingsType = pluginViewModel.PluginObject.ToString()
                                   });
                               }

                               var providesBuilderSettings = pluginViewModel.PluginObject as IProvideBuilderSettings;
                               if (providesBuilderSettings != null)
                               {
                                   buildConfiguration.Settings.AddRange(
                                       providesBuilderSettings.BuilderSettings.Select(
                                           x => x.BuilderProperty.ToPluginSetting(pluginViewModel.Plugin.PluginInfo)));
                               }
                           }

                           var existingBuildConfiguration =
                               BuildConfigurationManager.BuildConfigurations.FirstOrDefault(
                                   x => string.Equals(x.Name, inputViewModel.Text, StringComparison.OrdinalIgnoreCase));

                           if (existingBuildConfiguration != null)
                           {
                               BuildConfigurationManager.UpdateBuildConfiguration(existingBuildConfiguration,
                                   buildConfiguration);
                           }
                           else
                           {
                               BuildConfigurationManager.AddBuildConfiguration(buildConfiguration);
                           }
                       }));
            }
        }

        public RelayCommand CloseCommand
        {
            get
            {
                return _closeCommand ?? (_closeCommand = new RelayCommand(parameter =>
                {
                    Settings.Current.EnabledPlugins =
                        _plugins.Where(x => x.IsSelected).Select(x => x.Plugin.PluginInfo.Guid).ToList();
                    Settings.Current.Save();
                }));
            }
        }

        public RelayCommand OpenEulaCommand
        {
            get
            {
                return _openEulaCommand ?? (_openEulaCommand = new RelayCommand(parameter =>
                {
                    WindowServiceInterface.Current.OpenWindowDialog(
                        new InputMultilineTextViewModel(Licenses.OrcusEULA), "End User License Agreement");
                }));
            }
        }

        public List<IpAddressInfo> AvailableIpAddresses { get; }

        private void LoadSettings(IEnumerable<ClientSetting> clientSettings)
        {
            var settings = new List<IBuilderProperty>();
            foreach (var clientSetting in clientSettings)
            {
                var type = Type.GetType(clientSetting.SettingsType);
                if (type == null)
                    continue;

                var settingInstance = Activator.CreateInstance(type) as IBuilderProperty;
                if (settingInstance == null)
                    continue;

                BuilderPropertyHelper.ApplyProperties(settingInstance, clientSetting.Properties);
                settings.Add(settingInstance);
            }

            BuilderSettingsManager.InitializeSettings(settings);
        }

        public event EventHandler<IBuilderProperty> ShowBuilderProperty;
        public event EventHandler ShowBuildTab;

        private void PluginViewModelOnSelectedChanged(object sender, EventArgs eventArgs)
        {
            var pluginViewModel = (PluginViewModel) sender;
            if (pluginViewModel.IsSelected)
            {
                var overwriteBuilderProperties = pluginViewModel.PluginObject as IOverwriteBuilderProperties;
                if (overwriteBuilderProperties?.OverwrittenSettings?.Count > 0)
                    BuilderSettingsManager.AddOverwritingPlugin(pluginViewModel.Plugin, overwriteBuilderProperties);

                var provideBuilderSettings = pluginViewModel.PluginObject as IProvideBuilderSettings;
                if (provideBuilderSettings?.BuilderSettings?.Count > 0)
                    BuilderSettingsManager.AddProvidingPlugin(pluginViewModel.Plugin, provideBuilderSettings);
            }
            else if (pluginViewModel.PluginObject is IOverwriteBuilderProperties ||
                     pluginViewModel.PluginObject is IProvideBuilderSettings)
                BuilderSettingsManager.RemovePlugin(pluginViewModel.Plugin);
        }

        private async void BuildClient()
        {
            BuildLogger = new BuildLogger();

            var builderProperties =
                BuilderSettingsManager.BuilderSettings.Where(x => !x.IsFromPlugin)
                    .Select(x => x.BuilderProperty)
                    .ToList();

            foreach (var builderPropertyViewModel in BuilderSettingsManager.BuilderSettings)
            {
                var inputValidationResult =
                    builderPropertyViewModel.BuilderPropertyView.ValidateInput(builderProperties,
                        builderPropertyViewModel.BuilderProperty);
                builderPropertyViewModel.Failed = false;

                switch (inputValidationResult.ValidationState)
                {
                    case ValidationState.Error:
                        builderPropertyViewModel.Failed = true;

                        if (inputValidationResult.Message[0] == '@')
                            builderPropertyViewModel.FailMessage =
                                (string)
                                    Application.Current.Resources[inputValidationResult.Message.Substring(1)];
                        else
                            builderPropertyViewModel.FailMessage = inputValidationResult.Message;

                        ShowBuilderProperty?.Invoke(this, builderPropertyViewModel.BuilderProperty);
                        return;
                    case ValidationState.WarningYesNo:
                        if (WindowServiceInterface.Current.ShowMessageBox(inputValidationResult.Message,
                            (string) Application.Current.Resources["Warning"], MessageBoxButton.YesNo,
                            MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                            continue;

                        ShowBuilderProperty?.Invoke(this, builderPropertyViewModel.BuilderProperty);
                        return;
                    case ValidationState.Success:
                        continue;
                }
            }

            var builderPlugins =
                _plugins.Where(x => x.IsBuildPlugin && x.IsSelected).Cast<BuilderPluginViewModel>().ToList();
            var clientPlugins =
                _plugins.Where(x => !x.IsBuildPlugin && x.IsSelected).Cast<ClientPluginViewModel>().ToList();

            var settingsList = builderProperties.AsReadOnly();
            var builderArguments = new BuilderArguments(settingsList,
                $"{Application.Current.Resources["Executable"]}|*.exe|{Application.Current.Resources["Screensaver"]}|*.scr|Component Object Model|*.com");

            foreach (var builderPlugin in builderPlugins)
            {
                builderArguments.CurrentBuildPlugin = builderPlugin.Plugin;
                builderPlugin.BuildPlugin.Prepare(builderArguments);
            }

            builderArguments.CurrentBuildPlugin = null;

            var builderEvents = builderArguments.BuildPluginEvents;
            if (builderEvents.Count > 0)
                BuildLogger.Status(string.Format(
                    (string) Application.Current.Resources["BuildStatusLoadedBuildPlugins"],
                    builderEvents.Select(x => x.BuildPlugin).Distinct().Count(), builderEvents.Count));

            BuilderInformation builderInformation;
            if (builderArguments.SaveDialog == SaveDialogType.SaveFileDialog)
            {
                var sfd = new SaveFileDialog
                {
                    AddExtension = true,
                    CheckPathExists = true,
                    Filter = builderArguments.SaveDialogFilter
                };

                if (WindowServiceInterface.Current.ShowFileDialog(sfd) != true)
                    return;

                builderInformation = new BuilderInformation(sfd.FileName, BuildLogger);
            }
            else
            {
                var fbd = new VistaFolderBrowserDialog {ShowNewFolderButton = true};
                if (WindowServiceInterface.Current.ShowDialog(fbd.ShowDialog) != true)
                    return;

                builderInformation =
                    new BuilderInformation(
                        FileExtensions.MakeUnique(Path.Combine(fbd.SelectedPath, "OrcusClient.exe")), BuildLogger);
            }

            ShowBuildTab?.Invoke(this, EventArgs.Empty);
            IsBuilding = true;

            var builder = new Builder();

            var sw = Stopwatch.StartNew();
            try
            {
                await
                    Task.Run(
                        () =>
                            builder.Build(builderInformation, builderProperties, builderEvents,
                                clientPlugins.Select(x => x.ClientPlugin).ToList(), BuildLogger));
            }
            catch (PluginException ex)
            {
                BuildLogger.Error(ex.LogMessage);
                BuildLogger.Error(string.Format((string) Application.Current.Resources["BuildStatusFailed"],
                    sw.Elapsed.ToString("mm\\:ss\\.fff")));
                return;
            }
            catch (Exception ex)
            {
                BuildLogger.Error(ex.ToString());
                BuildLogger.Error(string.Format((string) Application.Current.Resources["BuildStatusFailed"],
                    sw.Elapsed.ToString("mm\\:ss\\.fff")));
                return;
            }
            finally
            {
                IsBuilding = false;
            }

            BuildLogger.Success(string.Format((string) Application.Current.Resources["BuildStatusSucceeded"],
                sw.Elapsed.ToString("mm\\:ss\\.fff")));

            Process.Start("explorer.exe", $"/select, \"{builderInformation.AssemblyPath}\"");
        }

        private void BuilderSettingsManagerOnBuilderSettingsOverwriteRequest(object sender,
            BuilderSettingsOverwriteRequestEventArgs builderSettingsOverwriteRequestEventArgs)
        {
            builderSettingsOverwriteRequestEventArgs.Overwrite =
                WindowServiceInterface.Current.ShowMessageBox("", "", MessageBoxButton.YesNo, MessageBoxImage.Stop,
                    MessageBoxResult.No) == MessageBoxResult.Yes;
        }

        private bool FilterPlugins(object o)
        {
            if (string.IsNullOrWhiteSpace(PluginsSearchText))
                return true;

            var pluginViewModel = (PluginViewModel) o;
            return
                pluginViewModel.Plugin.PluginInfo.Name.IndexOf(PluginsSearchText, StringComparison.OrdinalIgnoreCase) >
                -1 ||
                pluginViewModel.Plugin.PluginInfo.Author.IndexOf(PluginsSearchText, StringComparison.OrdinalIgnoreCase) >
                -1;
        }

        private void PluginViewModelOnOpenSettings(object sender,
            OpenPluginSettingsEventArgs openPluginSettingsEventArgs)
        {
            var propertiesEditable = openPluginSettingsEventArgs.PluginObject as IProvideEditableProperties;
            if (propertiesEditable != null)
            {
                WindowServiceInterface.Current.OpenWindowDialog(new PropertyGridSettingsViewModel(propertiesEditable),
                    openPluginSettingsEventArgs.Plugin.PluginInfo.Name);
                return;
            }

            var provideBuilderSettings = openPluginSettingsEventArgs.PluginObject as IProvideBuilderSettings;
            if (provideBuilderSettings != null)
            {
                var firstBuilderProperty = provideBuilderSettings.BuilderSettings.FirstOrDefault()?.BuilderProperty;
                if (firstBuilderProperty != null)
                    ShowBuilderProperty?.Invoke(this, firstBuilderProperty);
                return;
            }

            var provideWindowSettings = openPluginSettingsEventArgs.PluginObject as IProvideWindowSettings;
            provideWindowSettings?.ShowSettingsWindow(WindowServiceInterface.Current.GetCurrentWindow());
        }
    }
}