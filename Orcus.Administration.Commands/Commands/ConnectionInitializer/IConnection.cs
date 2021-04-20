using System;

namespace Orcus.Administration.Commands.ConnectionInitializer
{
    public interface IConnection : IDisposable
    {
        event EventHandler<DataReceivedEventArgs> DataReceived;
    }
}