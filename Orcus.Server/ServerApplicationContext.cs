using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using NLog;
using NLog.Config;
using NLog.Targets;
using Orcus.Server.Core;
using Orcus.Server.Core.Config;
using Orcus.Server.Core.Database;
using Orcus.Server.Core.UI;
using Orcus.Server.Forms;
using Orcus.Server.Native;
using Orcus.Server.Ui;
using Orcus.Shared.Core;

namespace Orcus.Server
{
    internal class ServerApplicationContext : ApplicationContext
    {
        private readonly Form _mainForm;
        private readonly NotifyIcon _notifyIcon;
        private readonly TcpServer _server;
        private readonly MenuItem _startMenuItem;
        private readonly MenuItem _stopMenuItem;

        public ServerApplicationContext()
        {
            var memoryLogger = new MemoryTarget {Layout = Program.LogLayout, Name = "textBox"};
            var memoryLoggerRule = new LoggingRule("*", Program.MinLogLevel, memoryLogger);
            LogManager.Configuration.LoggingRules.Add(memoryLoggerRule);

            LogManager.Configuration = LogManager.Configuration; //important, refreshes the config

            //Really important if executes by autostart
            var fi = new FileInfo(Application.ExecutablePath);
            Directory.SetCurrentDirectory(fi.DirectoryName);

            var settingsFile = new FileInfo("settings.json");
            Settings settings;
            if (settingsFile.Exists)
                settings = Settings.Load(settingsFile.FullName);
            else
            {
                var window = new CreateSettingsForm();
                if (window.ShowDialog() != DialogResult.OK)
                    Environment.Exit(0);

                settings = new Settings
                {
                    Password = window.Password,
                    SslCertificatePath = window.SslCertificatePath,
                    SslCertificatePassword = window.SslCertificatePassword
                };
                settings.IpAddresses.Add(new IpAddressInfo {Ip = window.Ip, Port = window.Port});
                settings.Save();
            }

            var database = new DatabaseManager("database.sqlite");
            database.Load();

            X509Certificate2 certificate;
            try
            {
                certificate = new X509Certificate2(settings.SslCertificatePath, settings.SslCertificatePassword);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to load the SSL certificate. Please change it or create a new one. Error details: {ex.Message}",
                    "Orcus Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(ex.HResult);
                return; //important for the dumb compiler
            }

            _server = new TcpServer(database, settings.IpAddresses, certificate) {Password = settings.Password};
            if (settings.IsGeoIpLocationEnabled)
            {
                _server.Ip2LocationEmailAddress = settings.Ip2LocationEmailAddress;
                _server.Ip2LocationPassword = settings.Ip2LocationPassword;
            }

            if (settings.UpdatePlugin != null && settings.IsDnsUpdaterEnabled)
            {
                settings.UpdatePlugin.ServerStarted();
                _server.DnsHostName = settings.UpdatePlugin.Host;
            }

            var blockListFile = new FileInfo("block-list.txt");
            if (blockListFile.Exists)
                _server.InitializeIpBlockList(blockListFile.FullName);
            else
                TcpServer.WriteDefaultIpBlockList(blockListFile.FullName);

            MainForm mainForm;
            _mainForm = mainForm = new MainForm(_server, memoryLogger, memoryLoggerRule, settings);

            mainForm.ShowHideMessage +=
                (sender, args) =>
                    _notifyIcon.ShowBalloonTip(5000, "Orcus Server", "The server was minimized to tray",
                        ToolTipIcon.Info);

            UiManager.RegisterUiImplementation(new WinFormsUiImplementation(mainForm));

            _server.Start();

            if (!Environment.GetCommandLineArgs().Contains("/hidden"))
                ShowWindow();

            _notifyIcon = new NotifyIcon
            {
                Text = "Orcus Server",
                Visible = true
            };

            var iconHandle = Properties.Resources.RemoteServer_16x.GetHicon();
            try
            {
                _notifyIcon.Icon = Icon.FromHandle(iconHandle);
            }
            finally
            {
                NativeMethods.DestroyIcon(iconHandle);
            }

            _notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
            _notifyIcon.ContextMenu = new ContextMenu();
            _notifyIcon.ContextMenu.MenuItems.Add(new MenuItem("Open", (sender, args) => ShowWindow()));
            _notifyIcon.ContextMenu.MenuItems.Add("-");

            _startMenuItem = new MenuItem("Start", (sender, args) => _server.Start()) {Enabled = !_server.IsRunning};
            _notifyIcon.ContextMenu.MenuItems.Add(_startMenuItem);

            _stopMenuItem = new MenuItem("Stop", (sender, args) => _server.Stop()) {Enabled = _server.IsRunning};
            _notifyIcon.ContextMenu.MenuItems.Add(_stopMenuItem);

            _server.IsRunningChanged += _server_IsRunningChanged;

            _notifyIcon.ContextMenu.MenuItems.Add("-");
            _notifyIcon.ContextMenu.MenuItems.Add("Close", (sender, args) =>
            {
                //all clients are automatically stopped when Dispose of server is called
                Application.Exit();
            });

            Application.ApplicationExit += Application_ApplicationExit;
        }

        private void _server_IsRunningChanged(object sender, EventArgs e)
        {
            _startMenuItem.Enabled = !_server.IsRunning;
            _stopMenuItem.Enabled = _server.IsRunning;
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowWindow();
        }

        private void ShowWindow()
        {
            _mainForm.Show();
            _mainForm.Activate();
        }

        private void Application_ApplicationExit(object sender, EventArgs e)
        {
            ((MainForm) _mainForm).PrepareShutdown();
            _mainForm.Close();
            _mainForm.Dispose();
            _server.Dispose();
            _notifyIcon.Dispose();
        }
    }
}