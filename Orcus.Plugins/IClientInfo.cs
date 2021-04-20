using Orcus.Shared.Resharper;

namespace Orcus.Plugins
{
    /// <summary>
    ///     Actions and information of the client
    /// </summary>
    public interface IClientInfo
    {
        /// <summary>
        ///     Provides all information about the connection to the server
        /// </summary>
        [CanBeNull]
        IServerConnection ServerConnection { get; }

        /// <summary>
        ///     Information and actions of the client
        /// </summary>
        IClientOperator ClientOperator { get; }
    }
}