using System;
using System.Net;
using NLog;
using Orcus.Shared.Encryption;

namespace Orcus.Server.Core.Plugins
{
    public class NamecheapUpdater : StandardUpdater<NamecheapUpdater.NamecheapUpdaterSettings>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly object _passwordLock = new object();

        public override string Name { get; } = "Namecheap Updater";

        public override string Host
            => string.IsNullOrEmpty(Settings.Host) ? Settings.DomainName : Settings.Host + "." + Settings.DomainName
        ;

        protected override void UpdateDns()
        {
            Logger.Debug("Update Namecheap DNS");

            var host = string.IsNullOrEmpty(Settings.Host) ? "@" : Settings.Host;

            using (var webClient = new WebClient())
            {
                lock (_passwordLock)
                    webClient.DownloadString(
                        $"https://dynamicdns.park-your-domain.com/update?host={host}&domain={Settings.Host}&password={Settings.Password}");
            }
        }

        public override bool SetupConsole()
        {
            Console.Write("Your Domain Name (example: yourdomain.tld): ");
            var domain = Console.ReadLine();
            if (string.IsNullOrEmpty(domain))
                return false;

            Console.Write(
                "Host (if you want the bare domain, leave empty. type subdomain name if you want the subdomain (ex: \"test\" for test.yourdomain.tld)): ");
            var host = Console.ReadLine();
            if (string.IsNullOrEmpty(host))
                return false;

            Console.Write("Your Dynamic DNS Password (Manage > Advanced DNS > Dynamic DNS): ");
            var password = Console.ReadLine();
            if (string.IsNullOrEmpty(password))
                return false;

            Settings = new NamecheapUpdaterSettings {DomainName = domain, Host = host, Password = password};
            return true;
        }

        public override string SaveSettings()
        {
            lock (_passwordLock)
            {
                var password = Settings.Password;
                Settings.Password = AES.Encrypt(password, "Gm<&9.7exSbu>GVP");
                var result = base.SaveSettings();
                Settings.Password = password;
                return result;
            }
        }

        //https://www.namecheap.com/support/knowledgebase/article.aspx/29/11/how-do-i-use-a-browser-to-dynamically-update-the-hosts-ip
        public override void LoadSettings(string settings)
        {
            base.LoadSettings(settings);
            Settings.Password = AES.Decrypt(Settings.Password, "Gm<&9.7exSbu>GVP");
        }

        public class NamecheapUpdaterSettings
        {
            public string DomainName { get; set; }
            public string Host { get; set; }
            public string Password { get; set; }
        }
    }
}