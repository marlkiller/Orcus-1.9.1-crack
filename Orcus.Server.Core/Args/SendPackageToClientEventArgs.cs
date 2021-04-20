using Orcus.Shared.Data;
using Orcus.Shared.Server;

namespace Orcus.Server.Core.Args
{
    public class SendPackageToClientEventArgs : SendPackageEventArgs
    {
        public SendPackageToClientEventArgs(int clientId, byte command, WriterCall writerCall)
            : base(command, writerCall)
        {
            ClientId = clientId;
        }

        public int ClientId { get; }
    }
}