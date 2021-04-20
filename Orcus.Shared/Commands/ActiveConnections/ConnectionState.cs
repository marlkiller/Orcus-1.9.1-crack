using System;

namespace Orcus.Shared.Commands.ActiveConnections
{
    [Serializable]
    public enum ConnectionState : byte
    {
        NoError,
        Established,
        Listening,
        TimeWait,
        CloseWait,
        Other
    }
}