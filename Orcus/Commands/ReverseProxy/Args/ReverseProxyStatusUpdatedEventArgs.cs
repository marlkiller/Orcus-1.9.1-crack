using System;
using System.Net;
using Orcus.Shared.Commands.ReverseProxy;

namespace Orcus.Commands.ReverseProxy.Args
{
    public class ReverseProxyStatusUpdatedEventArgs : ReverseProxyEventArgs
    {
        public bool IsConnected { get; }
        public IPAddress LocalAddress { get; }
        public int LocalPort { get; }
        public string TargetServer { get; }
        public string HostName { get; set; }

        public ReverseProxyStatusUpdate ToStatusUpdate()
        {
            return new ReverseProxyStatusUpdate
            {
                ConnectionId = ConnectionId,
                HostName = HostName,
                IsConnected = IsConnected,
                LocalPort = LocalPort,
                LocalAddress = LocalAddress
            };
        }

        public ReverseProxyStatusUpdatedEventArgs(int connectionId, bool isConnected, IPAddress localAddress,
            int localPort, string targetServer) : base(connectionId)
        {
            IsConnected = isConnected;
            LocalAddress = localAddress;
            LocalPort = localPort;
            TargetServer = targetServer;

            if (isConnected)
            {
                try
                {
                    //resolve the HostName of the Server
                    var entry = Dns.GetHostEntry(targetServer);
                    if (!string.IsNullOrEmpty(entry.HostName))
                    {
                        HostName = entry.HostName;
                    }
                }
                catch
                {
                    HostName = null;
                }
            }
        }
    }
}