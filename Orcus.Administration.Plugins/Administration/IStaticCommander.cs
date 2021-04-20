using System.Collections.Generic;
using System.Threading.Tasks;
using Orcus.Plugins.StaticCommands;
using Orcus.Shared.DynamicCommands;

namespace Orcus.Administration.Plugins.Administration
{
    /// <summary>
    ///     Send static commands to a range of clients
    /// </summary>
    public interface IStaticCommander
    {
        /// <summary>
        ///     Get all static commands which are available
        /// </summary>
        /// <returns></returns>
        List<StaticCommand> GetStaticCommands();

        /// <summary>
        ///     Send a static command to the server
        /// </summary>
        /// <param name="staticCommand">The static command which should get executed</param>
        /// <param name="transmissionEvent">Defines when the command should get executed</param>
        /// <param name="executionEvent">The event when the command should be executed (after the client received it)</param>
        /// <param name="stopEvent">
        ///     The event when the command should be stopped. This property can be left null when the
        ///     <see cref="StaticCommand" /> is not an <see cref="ActiveStaticCommand" />
        /// </param>
        /// <param name="conditions">The conditions which must apply to the receivers</param>
        /// <param name="target">The targets which should execute the static command</param>
        Task<bool> ExecuteCommand(StaticCommand staticCommand, TransmissionEvent transmissionEvent,
            ExecutionEvent executionEvent, StopEvent stopEvent, List<Condition> conditions, CommandTarget target);
    }
}