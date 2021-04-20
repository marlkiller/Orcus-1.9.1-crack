namespace Orcus.Shared.Commands.WindowManager
{
    public enum WindowManagerCommunication : byte
    {
        GetAllWindows,
        ResponseWindows,
        MaximizeWindow,
        ResponseWindowMaximized,
        ResponseWindowMaximizingFailed,
        MinimizeWindow,
        ResponseWindowMinimized,
        ResponseWindowMinimizingFailed,
        BringToFront,
        ResponseWindowBroughtToFront,
        ResponseWindowBringToFrontFailed,
        MakeTopmost,
        ResponseWindowIsTopmost,
        ResponseMakeWindowTopmostFailed,
        CloseWindow,
        ResponseWindowClosed,
        RestoreWindow,
        ResponseWindowRestored,
        ResponseWindowRestoringFailed,
        MakeWindowLoseTopmost,
        ResponseWindowLostTopmost,
        ResponseWindowLostTopmostFailed
    }
}