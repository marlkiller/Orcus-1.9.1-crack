using System.Collections.Generic;
using System.Linq;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;

namespace OrcusPluginStudio.Core.Test.AdministrationVirtualisation
{
    public class Commander : ICommander
    {
        public Commander(Command command, IConnectionInfo connectionInfo)
        {
            Commands = new List<Command> {command};
            command.Initialize(connectionInfo);
        }

        public List<Command> Commands { get; }

        public T GetCommand<T>() where T : Command
        {
            return Commands.OfType<T>().FirstOrDefault();
        }
    }
}