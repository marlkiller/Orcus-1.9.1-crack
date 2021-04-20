using System;
using Orcus.Shared.DynamicCommands;

namespace Orcus.Administration.Core.Args
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