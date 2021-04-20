using System;

namespace Orcus.Shared.Commands.RemoteDesktop
{
    [Flags]
    public enum CaptureType
    {
        DesktopDuplication = 1 << 0,
        FrontBuffer = 1 << 1,
        GDI = 1 << 2
    }
}