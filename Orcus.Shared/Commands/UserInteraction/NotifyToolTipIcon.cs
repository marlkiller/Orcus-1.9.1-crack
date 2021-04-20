using System;

namespace Orcus.Shared.Commands.UserInteraction
{
    [Serializable]
    public enum NotifyToolTipIcon : byte
    {
        None = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    }
}