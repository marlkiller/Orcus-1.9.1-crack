namespace Orcus.Administration.Plugins.Administration
{
    /// <summary>
    ///     The Administration control keeps important properties, methods and events for managing everything
    /// </summary>
    public interface IAdministrationControl
    {
        /// <summary>
        ///     The connection manager keeps the connection to the server
        /// </summary>
        IAdministrationConnectionManager AdministrationConnectionManager { get; }

        /// <summary>
        ///     Provides all static commands and allows to send them
        /// </summary>
        IStaticCommander StaticCommander { get; }
    }
}