using System;

namespace Orcus.Administration.Commands.ConnectionInitializer.Connections
{
    public class ServerConnection : IConnection
    {
        public void Dispose()
        {
        }

        public event EventHandler<DataReceivedEventArgs> DataReceived;
    }
}