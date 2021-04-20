namespace Orcus.Shared.Commands.ClipboardManager
{
    public enum ClipboardManagerCommunication
    {
        GetCurrentClipboard,
        ResponseClipboardChanged,
        ResponseClipboardEmpty,
        StartListener,
        ResponseListenerStarted,
        StopListener,
        ResponseListenerStopped,
        ChangeClipboard,
        ResponseClipboardChangedSuccessfully
    }
}