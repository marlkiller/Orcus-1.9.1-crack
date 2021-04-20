using System;
using System.IO;
using System.Net.Sockets;
using Orcus.Plugins;
using Orcus.Shared.Connection;

namespace OrcusPluginStudio.Core.Test.ClientVirtualisation
{
    public class ServerConnection : IServerConnection
    {
        public void Dispose()
        {
        }

        public BinaryReader BinaryReader { get; }
        public BinaryWriter BinaryWriter { get; }
        public object SendLock { get; } = new object();
        public TcpClient TcpClient { get; } = new TcpClient();
        public IDatabaseConnection DatabaseConnection { get; }
        public event EventHandler Disconnected;

        public void SendServerPackage(ServerPackageType serverPackageType, byte[] data, bool redirectPackage,
            ushort administrationId)
        {
        }
    }
}