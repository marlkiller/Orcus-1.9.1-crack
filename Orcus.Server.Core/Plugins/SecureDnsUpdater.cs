using System;
using System.Net;
using NLog;
using Orcus.Shared.Encryption;

namespace Orcus.Server.Core.Plugins
{
    public class SecureDnsUpdater : StandardUpdater<SecureDnsUpdater.SecureDnsUpdaterSettings>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly object _passwordLock = new object();

        public override string Name { get; } = "Secure DNS Updater";
        public override string Host => Settings?.HostName;

        protected override void UpdateDns()
        {
            Logger.Debug("Update Secure DNS");
            using (var webClient = new WebClient())
            {
                var ip = webClient.DownloadString("https://api.ipify.org/");
                string response;
                lock (_passwordLock)
                    response = webClient.DownloadString(
                        $"https://dyndns.topdns.com/update?hostname={Settings.HostName}&username={Settings.UserName}&password={Settings.Password}&myip={ip}");
            }
        }

        public override string SaveSettings()
        {
            lock (_passwordLock)
            {
                var password = Settings.Password;
                Settings.Password = AES.Encrypt(password, "WTASFDOHIZAGSD)P(ASGD");
                var result = base.SaveSettings();
                Settings.Password = password;
                return result;
            }
        }

        public override void LoadSettings(string settings)
        {
            base.LoadSettings(settings);
            Settings.Password = AES.Decrypt(Settings.Password, "WTASFDOHIZAGSD)P(ASGD");
        }

        public override bool SetupConsole()
        {
            Console.Write("Your DynDNS user name: ");
            var userName = Console.ReadLine();
            if (string.IsNullOrEmpty(userName))
                return false;

            Console.Write("Your DynDNS password: ");
            var password = Console.ReadLine();
            if (string.IsNullOrEmpty(password))
                return false;

            Console.Write("Your DynDNS host name: ");
            var hostName = Console.ReadLine();
            if (string.IsNullOrEmpty(hostName))
                return false;

            Settings = new SecureDnsUpdaterSettings {HostName = hostName, Password = password, UserName = userName};
            return true;
        }

        public class SecureDnsUpdaterSettings
        {
            public string HostName { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
        }
    }
}