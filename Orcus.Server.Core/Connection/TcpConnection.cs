using System.IO;
using System.Net.Security;
using System.Net.Sockets;

namespace Orcus.Server.Core.Connection
{
    internal class TcpConnection : IConnection
    {
        private readonly SslStream _sslStream;
        private readonly TcpClient _tcpClient;

        public TcpConnection(TcpClient tcpClient, SslStream sslStream, BinaryReader binaryReader,
            BinaryWriter binaryWriter)
        {
            BinaryReader = binaryReader;
            BinaryWriter = binaryWriter;
            _tcpClient = tcpClient;
            _sslStream = sslStream;
        }

        public void Dispose()
        {
            using (BinaryReader)
            using (BinaryWriter)
            using (_sslStream)
                _tcpClient.Close();
        }

        public BinaryWriter BinaryWriter { get; }
        public BinaryReader BinaryReader { get; }
        public Stream BaseStream => _sslStream;

        public void SetTimeout(int timeout)
        {
            _tcpClient.SendTimeout = timeout;
            _tcpClient.ReceiveTimeout = timeout;
        }
    }
}