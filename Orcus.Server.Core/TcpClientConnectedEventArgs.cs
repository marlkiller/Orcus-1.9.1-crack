using System;
using System.Net.Sockets;

namespace Orcus.Server.Core
{
    public class TcpClientConnectedEventArgs : EventArgs
    {
        public TcpClientConnectedEventArgs(TcpClient tcpClient)
        {
            TcpClient = tcpClient;
        }

        public TcpClient TcpClient { get; }
    }
}