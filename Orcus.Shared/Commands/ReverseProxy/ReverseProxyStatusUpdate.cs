using System;
using System.Net;

namespace Orcus.Shared.Commands.ReverseProxy
{
    [Serializable]
    public class ReverseProxyStatusUpdate
    {
        public int ConnectionId { get; set; }
        public bool IsConnected { get; set; }
        public IPAddress LocalAddress { get; set; }
        public int LocalPort { get; set; }
        public string HostName { get; set; }
    }
}