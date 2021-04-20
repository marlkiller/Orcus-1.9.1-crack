using System;
using Lidgren.Network;

namespace Orcus.Administration.Commands.ConnectionInitializer.Connections
{
    public class NetServerConnection : IConnection
    {
        private readonly NetServer _netServer;

        public NetServerConnection(NetServer netServer)
        {
            _netServer = netServer;
            netServer.RegisterReceivedCallback(Callback);
        }

        private void Callback(object state)
        {
            var message = _netServer.ReadMessage();
            if (message.MessageType == NetIncomingMessageType.Data)
                DataReceived?.Invoke(this, new DataReceivedEventArgs(message.Data, message.PositionInBytes, message.LengthBytes));

            _netServer.Recycle(message);
        }

        public void Dispose()
        {
            _netServer.Shutdown("Dispose");
        }

        public event EventHandler<DataReceivedEventArgs> DataReceived;
    }
}