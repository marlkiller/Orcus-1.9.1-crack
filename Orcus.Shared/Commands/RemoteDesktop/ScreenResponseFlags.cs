using System;

namespace Orcus.Shared.Commands.RemoteDesktop
{
    [Flags]
    public enum ScreenResponseFlags
    {
        Frame = 1 << 0,
        Cursor = 1 << 1
    }
}