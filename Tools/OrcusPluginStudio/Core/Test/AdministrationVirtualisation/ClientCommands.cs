using System;
using System.Collections.Generic;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.ComputerInformation;
using Orcus.Shared.Commands.Keylogger;
using Orcus.Shared.Commands.Password;
using Orcus.Shared.Connection;

namespace OrcusPluginStudio.Core.Test.AdministrationVirtualisation
{
    public class ClientCommands : IClientCommands
    {
        public void ChangeGroup(List<BaseClientInformation> clients, string newName)
        {
        }

        public void RemoveStoredData(List<OfflineClientInformation> clients)
        {
        }

        public ComputerInformation GetComputerInformation(BaseClientInformation client)
        {
            return new ComputerInformation {ProcessTime = 1000, Timestamp = DateTime.Now.AddSeconds(-5)};
        }

        public PasswordData GetPasswords(BaseClientInformation client)
        {
            return new PasswordData
            {
                Cookies =
                    new List<RecoveredCookie>
                    {
                        new RecoveredCookie
                        {
                            ApplicationName = "Chrome",
                            ExpiresUtc = DateTime.Now.AddDays(100),
                            Path = Environment.SystemDirectory,
                            Name = "Test",
                            Value = "ASDassdaasd",
                            Host = "Host",
                            HttpOnly = true,
                            Secure = false
                        }
                    },
                Passwords =
                    new List<RecoveredPassword>
                    {
                        new RecoveredPassword
                        {
                            Application = "Chrome",
                            Password = "test",
                            UserName = "Garcon",
                            PasswordType = PasswordType.Browser
                        }
                    }
            };
        }

        public LocationInfo GetClientLocation(BaseClientInformation client)
        {
            return new LocationInfo();
        }

        public List<KeyLogPresenter> GetKeyLogs(BaseClientInformation client)
        {
            return new List<KeyLogPresenter> {new KeyLogPresenter {Id = 0, Timestamp = DateTime.Now.AddHours(-4.5)}};
        }

        public List<KeyLogEntry> GetKeyLog(KeyLogPresenter keyLogPresenter)
        {
            if (keyLogPresenter.Id == 0)
                return new List<KeyLogEntry>
                {
                    new NormalText {Text = "youtu"},
                    new StandardKey(Keys.Enter, true),
                    new StandardKey(Keys.Enter, false),
                    new WindowChanged("YouTube")
                };

            return null;
        }
    }
}