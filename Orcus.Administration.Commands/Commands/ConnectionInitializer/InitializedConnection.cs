using System;

namespace Orcus.Administration.Commands.ConnectionInitializer
{
    public struct InitializedConnection
    {
        public InitializedConnection(IConnection connection, Guid remoteConnectionGuid)
        {
            Connection = connection;
            RemoteConnectionGuid = remoteConnectionGuid;
            Succeed = true;
        }

        public Guid RemoteConnectionGuid { get; }
        public IConnection Connection { get; }
        public bool Succeed { get; set; }

        public static InitializedConnection Failed => new InitializedConnection {Succeed = false};
    }
}