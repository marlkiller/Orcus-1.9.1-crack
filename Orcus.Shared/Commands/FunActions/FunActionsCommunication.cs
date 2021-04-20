namespace Orcus.Shared.Commands.FunActions
{
    public enum FunActionsCommunication : byte
    {
        HideTaskbar,
        ShowTaskbar,
        HoldMouse,
        TriggerBluescreen,
        DisableMonitor,
        Shutdown,
        LogOff,
        Restart,
        RotateScreen,
        PureEvilness,
        StopPureEvilness,
        ChangeKeyboardLayout,
        OpenWebsite,
        ShowDesktop,
        HideDesktop,
        ShowClock,
        HideClock,
        EnableTaskmanager,
        DisableTaskmanager,
        SwapMouseButtons,
        RestoreMouseButtons,
        DisableUserInput,
        ChangeDesktopWallpaper,
        HangSystem,
        OpenCdDrive,
        CloseCdDrive
    }

    public enum RotateDegrees : byte
    {
        Degrees0 = 0,
        Degrees90 = 1,
        Degrees180 = 2,
        Degrees270 = 3
    }
}