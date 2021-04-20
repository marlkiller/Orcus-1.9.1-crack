using System.Collections.Generic;

namespace Orcus.Administration.Plugins.CommandViewPlugin
{
    /// <summary>
    ///     Manages responses from the client
    /// </summary>
    public interface ICommander
    {
        /// <summary>
        ///     A full list of all registered commands
        /// </summary>
        List<Command> Commands { get; }

        /// <summary>
        ///     Returns a command of type <see cref="T" />
        /// </summary>
        /// <typeparam name="T">The type of the command</typeparam>
        T GetCommand<T>() where T : Command;
    }
}