using System;
using Orcus.Shared.DynamicCommands;

namespace Orcus.Server.Core.DynamicCommands
{
    public class DynamicCommandStatusUpdatedEventArgs : EventArgs
    {
        public DynamicCommandStatusUpdatedEventArgs(int dynamicCommand, DynamicCommandStatus status)
        {
            DynamicCommand = dynamicCommand;
            Status = status;
        }

        public int DynamicCommand { get; }
        public DynamicCommandStatus Status { get; }
    }
}