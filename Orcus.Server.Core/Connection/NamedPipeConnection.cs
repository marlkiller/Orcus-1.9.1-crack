using System.IO;
using System.IO.Pipes;

namespace Orcus.Server.Core.Connection
{
    internal class NamedPipeConnection : IConnection
    {
        private readonly NamedPipeServerStream _namedPipeServerStream;

        public NamedPipeConnection(NamedPipeServerStream namedPipeServerStream)
        {
            _namedPipeServerStream = namedPipeServerStream;
            BinaryWriter = new BinaryWriter(namedPipeServerStream);
            BinaryReader = new BinaryReader(namedPipeServerStream);
        }

        public void Dispose()
        {
            _namedPipeServerStream.Close();
        }

        public BinaryWriter BinaryWriter { get; }
        public BinaryReader BinaryReader { get; }
        public Stream BaseStream => _namedPipeServerStream;

        public void SetTimeout(int timeout)
        {
            //timeout not supported
        }
    }
}