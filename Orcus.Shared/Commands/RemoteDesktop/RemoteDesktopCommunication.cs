namespace Orcus.Shared.Commands.RemoteDesktop
{
    public enum RemoteDesktopCommunication : byte
    {
        GetInfo,
        Start,
        Stop,
        ResponseInfo,
        Initialize,
        InitializeConnection,
        InitializeDirectConnection,
        ResponseFrame,
        ChangeQuality,
        ResponseInitializationFailed,
        ResponseInitializationSucceeded,
        ChangeMonitor,
        DesktopAction,
        ChangeDrawCursor,
        ResponseCaptureCancelled
    }
}