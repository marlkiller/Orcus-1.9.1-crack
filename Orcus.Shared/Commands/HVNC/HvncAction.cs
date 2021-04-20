namespace Orcus.Shared.Commands.HVNC
{
    public enum HvncAction : byte
    {
        LeftDown,
        RightDown,
        LeftUp,
        RightUp,
        MouseMove,
        ScrollUp,
        ScrollDown,
        KeyPressed,
        KeyReleased
    }
}