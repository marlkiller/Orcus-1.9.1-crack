using Orcus.Shared.DynamicCommands;

namespace Orcus.Administration.Plugins.Administration
{
    /// <summary>
    ///     A connection to the server
    /// </summary>
    public interface IServerConnection
    {
        /// <summary>
        ///     Send a dynamic command to the server
        /// </summary>
        /// <param name="dynamicCommand">The dynamic command which should get executed</param>
        void SendCommand(DynamicCommand dynamicCommand);
    }
}