using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using Orcus.Server.Core.Plugins;
using Orcus.Shared.Encryption;
using Orcus.Shared.Server;

namespace Orcus.Server.Core.Config
{
    public class Settings : ServerConfig
    {
        private const string AesPassword = "just trolling the repo";
        private static List<IUpdatePlugin> _updatePlugins;

        private IUpdatePlugin _updatePlugin;

        [ScriptIgnore]
        public IUpdatePlugin UpdatePlugin
        {
            get { return _updatePlugin; }
            set
            {
                _updatePlugin?.Stop();
                _updatePlugin = value;
            }
        }

        public static List<IUpdatePlugin> GetUpdatePlugins()
        {
            return _updatePlugins ??
                   (_updatePlugins = new List<IUpdatePlugin> {new SecureDnsUpdater(), new NoIpUpdater(), new NamecheapUpdater()});
        }

        public void Save()
        {
            var temp = SslCertificatePassword;

            if (!string.IsNullOrEmpty(SslCertificatePassword))
                SslCertificatePassword = AES.Encrypt(SslCertificatePassword, AesPassword);

            DnsUpdaterSettings = UpdatePlugin?.SaveSettings();
            DnsUpdaterType = UpdatePlugin?.GetType().FullName;

            File.WriteAllText("settings.json", new JavaScriptSerializer().Serialize(this));
            SslCertificatePassword = temp;
        }

        public static Settings Load(string path)
        {
            var result = new JavaScriptSerializer().Deserialize<Settings>(File.ReadAllText(path));
            if (!string.IsNullOrEmpty(result.SslCertificatePassword))
                result.SslCertificatePassword = AES.Decrypt(result.SslCertificatePassword, AesPassword);
            if (!string.IsNullOrEmpty(result.DnsUpdaterType))
            {
                try
                {
                    var updaterType = Type.GetType(result.DnsUpdaterType);
                    result.UpdatePlugin = GetUpdatePlugins().First(x => x.GetType() == updaterType);
                    result.UpdatePlugin.LoadSettings(result.DnsUpdaterSettings);
                }
                catch (Exception)
                {
                    result.DnsUpdaterType = null;
                    result.DnsUpdaterSettings = null;
                    result.IsDnsUpdaterEnabled = false;
                }
            }
            return result;
        }
    }
}