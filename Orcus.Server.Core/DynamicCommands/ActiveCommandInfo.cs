using System.Collections.Generic;
using Orcus.Shared.DynamicCommands;

namespace Orcus.Server.Core.DynamicCommands
{
    public class ActiveCommandInfo
    {
        public ActiveCommandInfo(RegisteredDynamicCommand dynamicCommand)
        {
            DynamicCommand = dynamicCommand;
            Clients = new List<Client>();
        }

        public RegisteredDynamicCommand DynamicCommand { get; }
        public List<Client> Clients { get; }
        public object ClientsLock { get; } = new object();
    }
}