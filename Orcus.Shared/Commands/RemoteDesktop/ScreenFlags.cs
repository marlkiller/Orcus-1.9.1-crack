using System;

namespace Orcus.Shared.Commands.RemoteDesktop
{
    [Flags]
    public enum ScreenFlags
    {
        None = 1 << 0,
        DirtyRects = 1 << 1,
        /// <summary>
        /// Second index. 8 bytes x & y (int) coordinate of cursor, 4 bytes cursor data size (int), data
        /// </summary>
        Cursor = 1 << 2
    }
}