using System;
using System.Net;
using System.Text;
using NLog;
using Orcus.Shared.Encryption;

namespace Orcus.Server.Core.Plugins
{
    public class NoIpUpdater : StandardUpdater<NoIpUpdater.NoIpUpdaterSettings>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly object _passwordLock = new object();

        public override string Name { get; } = "No-IP Updater";
        public override string Host => Settings?.HostName;

        protected override void UpdateDns()
        {
            Logger.Debug("Update No-Ip DNS");

            var request =
                (HttpWebRequest)
                WebRequest.Create(
                    $"http://dynupdate.no-ip.com/nic/update?hostname={Settings.HostName}");

            request.Proxy = null;
            request.UserAgent = $"Orcus No-Ip Updater/2.0 {Settings.HostName}";
            request.Timeout = 10000;
            lock (_passwordLock)
                request.Headers.Add(HttpRequestHeader.Authorization,
                    $"Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes($"{Settings.EMail}:{Settings.Password}"))}");
            request.Method = "GET";

            ((IDisposable) (HttpWebResponse) request.GetResponse()).Dispose();
        }

        public override bool SetupConsole()
        {
            Console.Write("Your No-IP E-Mail: ");
            var email = Console.ReadLine();
            if (string.IsNullOrEmpty(email))
                return false;

            Console.Write("Your No-IP password: ");
            var password = Console.ReadLine();
            if (string.IsNullOrEmpty(password))
                return false;

            Console.Write("Your No-IP host name: ");
            var hostName = Console.ReadLine();
            if (string.IsNullOrEmpty(hostName))
                return false;

            Settings = new NoIpUpdaterSettings {HostName = hostName, Password = password, EMail = email};
            return true;
        }

        public override string SaveSettings()
        {
            lock (_passwordLock)
            {
                var password = Settings.Password;
                Settings.Password = AES.Encrypt(password, "ASDTWRSDWRdwsdwedaWEAE");
                var result = base.SaveSettings();
                Settings.Password = password;
                return result;
            }
        }

        public override void LoadSettings(string settings)
        {
            base.LoadSettings(settings);
            Settings.Password = AES.Decrypt(Settings.Password, "ASDTWRSDWRdwsdwedaWEAE");
        }

        public class NoIpUpdaterSettings
        {
            public string HostName { get; set; }
            public string EMail { get; set; }
            public string Password { get; set; }
        }
    }
}