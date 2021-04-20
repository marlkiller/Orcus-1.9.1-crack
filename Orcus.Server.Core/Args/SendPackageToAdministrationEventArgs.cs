using Orcus.Shared.Data;
using Orcus.Shared.Server;

namespace Orcus.Server.Core.Args
{
    public class SendPackageToAdministrationEventArgs : SendPackageEventArgs
    {
        public SendPackageToAdministrationEventArgs(ushort administrationId, byte command, WriterCall writerCall)
            : base(command, writerCall)
        {
            AdministrationId = administrationId;
        }

        public ushort AdministrationId { get; }
    }
}