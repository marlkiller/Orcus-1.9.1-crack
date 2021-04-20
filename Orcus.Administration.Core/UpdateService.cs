using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using nUpdate.UpdateEventArgs;
using nUpdate.Updating;
#if !DEBUG
using System.Threading.Tasks;
#endif

namespace Orcus.Administration.Core
{
    public class UpdateService
    {
        private readonly UpdateManager _updateManager;

        public UpdateService()
        {
            _updateManager = new UpdateManager(new Uri("https://orcus.pw/orcusapp/update/administration/updates.json"),
                "<RSAKeyValue><Modulus>iH37f7oFsXTajxSKZBBfUWB214YAwgjD2Y/GnIeBL9ejgi7FhvY1UYmR1ZpMPfAXhyoPwsI9jI58ks8X4cU4bBgBMpRIhz0wBDQ34RE+QQlPPSkth13VLGZg0noDd7SMcuq/ja43cfQbe3txNuMT+pEoKnGthK35FwH1tz9yMnqa/rZ1IF6eMcYwbhgxCHAFxjfvTDUA2KobDDlfF3lukSbCK8CDOS392+oOibHYlrMlrSDgWybfkvzEqMoqnPKUs2pjpIoCKLsJxVzON9O+Um/AdKXxvZr5etxJ5IKa6LJHD6eta2e4516zra2h0W3EELojLtL2hHTJ5JJj2FVd+yGkj7bgsqc7Mw4mDDDu1oKcQ+jxv0H7kcXixw5tVQ1okVkK+xoMSwllM9/0NGWBUanCuM3UP+okt6OD+oYMgG81+zl+tUwP/srT2ZXHqb4KiIuvSXFFe22FpOkgVZTabPs89J1h2GvMnekr+Djhl4hhJ9pTHcbDQr3Mb8dovRbnCpC1Hq+toibRAHZErYw+ZaEPAFe7rKDVM0gHGYiRv1/8GNQcRNGh/jn9bVkuVvKdEzXh0YLuVyDFM2vdlNFafyJwQjqsk+u99ijNqt8UVsQgjVKg3J8U36stikdm+pdPz3yPM+4FEP6/naq6383Wc9ojttI6BOhmcbJtzsw3VrievlBYepgDSuwdskWDeMq5Rbzm+Co5jEwsCNHgj0jnYfwJDv0P2qKAQRsZMLLsRR6L84EqCWx/QV/XA+DeLujE+tlwtrkStrOfEQ8fiW3jPssqRTh8Uddk2BIU00kdoDd7ApZgOaVKLthm1kI/G4HVMS0gYOPK5CaMkFDJvigOlLcN/71Cw+EscuQvAwhJGDTU6TmZCogZ/DCkC4wYxVIuSoYNn2JvLWxnNLCwhSxRtLJigJIstZ67Efbs2M4cmks4yso3Av6zqIJ5LidEqk0giM/C0XPWnDHeBib1yAjqt8n1+6WrRASkTiNOZJID0jZWQYu4BQilEncrtJV79lJU6U84DTNBuIJnI798Pc3Jpx/eJOeJiqabjvIP2qM8aUhZ3MSFttIVA9UR9CcNMx5e31T446b7BjR0h9WjC2FMmxvSPOwaPUsa/1WnXe5KYVFIfDpnIn1fSKhB9+MApOInrV4rvHSP88rtltgvky2JvK64BiBW87f2CDhAJaiZTIyDpnQN/VcsKDy2nbWWE7nDm6YRsj7oiiLqFFMDtFWvA2KfJbaQVvw8R/TMeJfSBDdXTs9Zs+CwmSOT2EGpqaDNTKRlwEWVEfBrHOqNeDWj4IjmutRTq3Pfknp5+/bKW5r24+eiPTz9j8pwg/ZDjvGKclwUwSUHHT4q7lCC6YmMfQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>",
                new CultureInfo("en"), ConfigurationUpdateAction) {CloseHostApplication = true};

            _updateManager.PackagesDownloadFinished += _updateManager_PackagesDownloadFinished;
            _updateManager.PackagesDownloadProgressChanged += _updateManager_PackagesDownloadProgressChanged;
        }

        public string NewVersion { get; private set; }
        public double UpdateSize { get; private set; }

        public string Changelog
        {
            get
            {
#if DEBUG
                if (_updateManager.PackageConfigurations == null)
                    return "No update available";
#endif
                var sb = new StringBuilder();
                foreach (var package in _updateManager.PackageConfigurations)
                {
                    sb.AppendLine("[i]" +
                                  string.Format((string) Application.Current.Resources["UpdateChangelogText"],
                                      package.LiteralVersion));
                    sb.AppendLine();
                    string changelog;
                    if (
                        package.Changelog.Any(
                            x =>
                                x.Key.TwoLetterISOLanguageName ==
                                Settings.Current.Language.CultureInfo.TwoLetterISOLanguageName))
                        changelog =
                            package.Changelog.First(
                                x =>
                                    x.Key.TwoLetterISOLanguageName ==
                                    Settings.Current.Language.CultureInfo.TwoLetterISOLanguageName).Value;
                    else
                        changelog = package.Changelog.First(x => x.Key.TwoLetterISOLanguageName == "en").Value;
                    sb.AppendLine(changelog);
                    sb.AppendLine();
                }

                return sb.ToString();
            }
        }

        private static IEnumerable<UpdateConfiguration> ConfigurationUpdateAction(
            IEnumerable<UpdateConfiguration> updateConfigurations)
        {
            foreach (var updateConfiguration in updateConfigurations)
            {
                updateConfiguration.UpdatePackageUri =
                    new Uri(
                        $"https://www.orcus.pw/orcusapp/OrcusServer.php?method=u&DownloadPath={Regex.Match(updateConfiguration.UpdatePackageUri.AbsoluteUri, @"(?<=\/update\/administration\/).+?$").Value}",
                        UriKind.Absolute);
            }

            return updateConfigurations;
        }

        public event EventHandler UpdateFound;
        public event EventHandler<UpdateDownloadProgressChangedEventArgs> DownloadProgressChanged;

        private void _updateManager_PackagesDownloadProgressChanged(object sender,
            UpdateDownloadProgressChangedEventArgs e)
        {
            DownloadProgressChanged?.Invoke(this, e);
        }

        private void _updateManager_PackagesDownloadFinished(object sender, EventArgs e)
        {
            if (_updateManager.ValidatePackages())
                _updateManager.InstallPackage();
        }

#if DEBUG
        public void CheckForUpdates()
        {
            //dont check for updates every time we debug
            NewVersion = string.Empty;
            UpdateSize = 0;
        }
#else
        public async void CheckForUpdates()
        {
            try
            {
                if (await Task.Run(() => _updateManager.SearchForUpdates()))
                {
                    UpdateSize = _updateManager.TotalSize;
                    NewVersion = Version.Parse(_updateManager.PackageConfigurations.Last().LiteralVersion).ToString(3);
                    UpdateFound?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
#endif

        public void Update()
        {
            _updateManager.DownloadPackagesAsync();
        }

        public void CancelUpdate()
        {
            _updateManager.CancelDownload();
        }
    }
}