using System;
using System.Threading.Tasks;
using Orcus.Administration.Plugins.Administration;
using Orcus.Shared.Client;
using Orcus.Shared.Connection;

namespace Orcus.Administration.Plugins.CommandViewPlugin
{
    /// <summary>
    ///     Provide a lot of methods which can control the client and provide information
    /// </summary>
    public interface IClientController : IDisposable
    {
        /// <summary>
        ///     Provides all registered commands
        /// </summary>
        ICommander Commander { get; }

        /// <summary>
        ///     All basic information about the client
        /// </summary>
        OnlineClientInformation Client { get; }

        /// <summary>
        ///     Some commands of the client
        /// </summary>
        IClientCommands ClientCommands { get; }

        /// <summary>
        ///     Manages the server connection
        /// </summary>
        IConnectionManager ConnectionManager { get; }

        /// <summary>
        ///     Provides all static commands
        /// </summary>
        IStaticCommander StaticCommander { get; }

        /// <summary>
        ///     Get the config of the client
        /// </summary>
        /// <returns>The deserialized config</returns>
        Task<ClientConfig> GetClientConfig();

        /// <summary>
        ///     Triggered when the client is disconnected
        /// </summary>
        event EventHandler Disconnected;
    }
}