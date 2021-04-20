namespace Orcus.Plugins
{
    /// <summary>
    ///     Defines a class which gets loaded and unloaded by the client
    /// </summary>
    public interface ILoadable
    {
        /// <summary>
        ///     Called when the application has started successfully
        /// </summary>
        void Start();

        /// <summary>
        ///     Called when the application is closing
        /// </summary>
        void Shutdown();

        /// <summary>
        ///     Called when the application is installing
        /// </summary>
        /// <param name="executablePath">The path to the client file</param>
        void Install(string executablePath);

        /// <summary>
        ///     Called when the client is uninstalling
        /// </summary>
        /// <param name="executablePath">The path to the client file</param>
        void Uninstall(string executablePath);

        /// <summary>
        ///     Called directly after the plugin was loaded into Orcus to set the <see cref="ClientOperator" />
        /// </summary>
        /// <param name="clientOperator"></param>
        void Initialize(IClientOperator clientOperator);
    }
}