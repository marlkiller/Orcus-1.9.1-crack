using System;

namespace Orcus.Shared.Commands.DropAndExecute
{
    [Serializable]
    public class ExecuteOptions
    {
        public ExecutionMode ExecutionMode { get; set; }
        public bool RunAsAdministrator { get; set; }
        public string Arguments { get; set; }
        public Guid FileGuid { get; set; }
    }
}