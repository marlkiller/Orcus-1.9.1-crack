using System.IO;
using System.IO.Pipes;

namespace Orcus.Administration.Core.Connection
{
    public class NamedPipeConnection : IConnection
    {
        private readonly NamedPipeClientStream _namedPipeConnection;

        public NamedPipeConnection(NamedPipeClientStream namedPipeClientStream)
        {
            _namedPipeConnection = namedPipeClientStream;
            BinaryWriter = new BinaryWriter(namedPipeClientStream);
            BinaryReader = new BinaryReader(namedPipeClientStream);
            BaseStream = namedPipeClientStream;
        }

        public void Dispose()
        {
            using (BinaryReader)
            using (BinaryWriter)
                _namedPipeConnection.Dispose();
        }

        public BinaryReader BinaryReader { get; }
        public BinaryWriter BinaryWriter { get; }
        public Stream BaseStream { get; }
    }
}