using System;
using System.Collections.Generic;
using Orcus.Shared.DynamicCommands;

namespace Orcus.Shared.Connection
{
    [Serializable]
    public class ActiveCommandsUpdate
    {
        public List<CommandStatusInfo> CommandsDeactivated { get; set; }
        public List<ActiveCommandUpdateInfo> UpdatedCommands { get; set; }
    }

    [Serializable]
    public class ActiveCommandUpdateInfo
    {
        public List<int> Clients { get; set; }
        public int CommandId { get; set; }
    }

    [Serializable]
    public class CommandStatusInfo
    {
        public DynamicCommandStatus Status { get; set; }
        public int CommandId { get; set; }
    }
}