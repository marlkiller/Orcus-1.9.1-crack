namespace Orcus.Plugins
{
    /// <summary>
    ///     Provides the interface to tools implemented in the client
    /// </summary>
    public interface IToolBase
    {
        /// <summary>
        ///     The connection to the service. Null if not connected
        /// </summary>
        IServicePipe ServicePipe { get; }

        /// <summary>
        ///     Execute the file without being the parent process
        /// </summary>
        /// <param name="path">The path to the file</param>
        void ExecuteFileAnonymously(string path);
    }
}