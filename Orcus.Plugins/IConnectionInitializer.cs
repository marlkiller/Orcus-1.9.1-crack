using System;
using Orcus.Shared.Data;
using Orcus.Shared.Server;

namespace Orcus.Plugins
{
    public interface IConnectionInitializer
    {
        IConnection TakeConnection(Guid guid);
    }

    public interface IConnection : IDisposable
    {
        void SendData(byte[] buffer, int offset, int length);
        void SendStream(WriterCall writerCall);
        bool SupportsStream { get; }
    }
}