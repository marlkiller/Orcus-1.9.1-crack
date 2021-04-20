using System;

namespace Orcus.Shared.Connection
{
    [Flags]
    public enum ClientRedirectOptions
    {
        None,
        IncludeAdministrationId = 1 << 0
    }
}