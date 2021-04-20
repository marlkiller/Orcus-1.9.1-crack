using System.Collections.Generic;
using System.Threading.Tasks;
using Orcus.Administration.Plugins.Administration;
using Orcus.Plugins.StaticCommands;
using Orcus.Shared.DynamicCommands;

namespace OrcusPluginStudio.Core.Test.AdministrationVirtualisation
{
    public class StaticCommander : IStaticCommander
    {
        public List<StaticCommand> GetStaticCommands()
        {
            return new List<StaticCommand>();
        }

        public Task<bool> ExecuteCommand(StaticCommand staticCommand, TransmissionEvent transmissionEvent, ExecutionEvent executionEvent,
            StopEvent stopEvent, List<Condition> conditions, CommandTarget target)
        {
            return Task.FromResult(true);
        }
    }
}