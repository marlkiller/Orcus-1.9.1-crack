using System;
using Orcus.Shared.DynamicCommands;

namespace Orcus.Server.Core.DynamicCommands
{
    public class ExecuteDynamicCommandEventArgs : EventArgs
    {
        public ExecuteDynamicCommandEventArgs(RegisteredDynamicCommand dynamicCommand)
        {
            DynamicCommand = dynamicCommand;
        }

        public RegisteredDynamicCommand DynamicCommand { get; }
    }
}