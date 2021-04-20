using System;

namespace Orcus.Commands.ReverseProxy.Args
{
    public class ReverseProxyEventArgs : EventArgs
    {
        public ReverseProxyEventArgs(int connectionId)
        {
            ConnectionId = connectionId;
        }

        public int ConnectionId { get; }
    }
}