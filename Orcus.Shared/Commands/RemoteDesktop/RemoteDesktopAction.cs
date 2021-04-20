namespace Orcus.Shared.Commands.RemoteDesktop
{
    public enum RemoteDesktopAction : byte
    {
        Mouse,
        Keyboard
    }

    public enum RemoteDesktopMouseAction : byte
    {
        LeftDown,
        LeftUp,
        RightDown,
        RightUp,
        MiddleDown,
        MiddleUp,
        XButton1Down,
        XButton1Up,
        XButton2Down,
        XButton2Up,
        Move,
        Wheel
    }

    public enum RemoteDesktopKeyboardAction : byte
    {
        KeyDown,
        KeyUp
    }
}