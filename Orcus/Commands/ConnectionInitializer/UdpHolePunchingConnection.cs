using System;
using Lidgren.Network;
using Orcus.Plugins;
using Orcus.Shared.Data;
using Orcus.Shared.Server;

namespace Orcus.Commands.ConnectionInitializer
{
    public class UdpHolePunchingConnection : IConnection
    {
        public UdpHolePunchingConnection(NetClient netClient)
        {
            NetClient = netClient;
        }

        public void Dispose()
        {
        }

        public NetClient NetClient { get; }

        public void SendData(byte[] buffer, int offset, int length)
        {
        }

        public void SendStream(WriterCall writerCall)
        {
            throw new NotImplementedException();
        }

        public bool SupportsStream { get; }
    }
}