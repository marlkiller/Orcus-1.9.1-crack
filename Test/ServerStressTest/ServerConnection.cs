using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;

namespace ServerStressTest
{
    class ServerConnection : IDisposable
    {
        private readonly BinaryReader _binaryReader;
        private readonly BinaryWriter _binaryWriter;
        private readonly TcpClient _client;
        private readonly SslStream _stream;

        public ServerConnection(TcpClient client, SslStream stream, BinaryReader binaryReader, BinaryWriter binaryWriter)
        {
            _client = client;
            _stream = stream;
            _binaryReader = binaryReader;
            _binaryWriter = binaryWriter;
        }

        public void Dispose()
        {
            _binaryWriter.Dispose();
            _binaryReader.Dispose();
            _stream.Dispose();
            _client.Close();
        }
    }
}