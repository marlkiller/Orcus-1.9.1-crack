using System;

namespace Orcus.Shared.Commands.ActiveConnections
{
    [Serializable]
    public class ActiveConnection
    {
        public ProtocolName ProtocolName { get; set; }
        public string LocalAddress { get; set; }
        public string RemoteAddress { get; set; }
        public int LocalPort { get; set; }
        public int RemotePort { get; set; }
        public ConnectionState State { get; set; }
        public string ApplicationName { get; set; }
    }
}