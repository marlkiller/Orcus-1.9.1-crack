using System.IO;
using System.Net.Security;
using System.Net.Sockets;

namespace Orcus.Administration.Core.Connection
{
    public class TcpConnection : IConnection
    {
        private readonly TcpClient _tcpClient;

        public TcpConnection(TcpClient tcpClient, BinaryReader binaryReader, BinaryWriter binaryWriter,
            SslStream sslStream)
        {
            _tcpClient = tcpClient;
            BinaryWriter = binaryWriter;
            BinaryReader = binaryReader;
            BaseStream = sslStream;
        }

        public void Dispose()
        {
            using (BinaryWriter)
            using (BinaryReader)
            using (BaseStream)
                _tcpClient.Close();
        }

        public BinaryReader BinaryReader { get; }
        public BinaryWriter BinaryWriter { get; }
        public Stream BaseStream { get; }
    }
}