using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Globalization;
using System.Linq;
using System.ServiceProcess;

namespace Orcus.Service
{
    [RunInstaller(true)]
    public class WindowsServiceInstaller : Installer
    {
        private readonly Dictionary<string, string> _translatedDescriptions = new Dictionary<string, string>
        {
            {
                "en",
                "Manages input devices for Windows-based programs. If this service is stopped, input devices will not function properly. If this service is disabled, any services that explicitly depend on it will fail to start."
            },
            {
                "de",
                "Verwaltet Eingabegeräte für Windows-basierte Programme. Wenn dieser Dienst beendet wird, funktionieren Eingabegeräte nicht ordnungsgemäß. Wenn dieser Dienst deaktiviert wird, können die Dienste, die von diesem Dienst explizit abhängig sind, nicht mehr gestartet werden."
            }
        };

        private readonly Dictionary<string, string> _translatedNames = new Dictionary<string, string>
        {
            {"en", "Windows Input"},
            {"de", "Windows-Eingabe"}
        };

        /// <summary>
        ///     Public Constructor for WindowsServiceInstaller.
        ///     - Put all of your Initialization code here.
        /// </summary>
        public WindowsServiceInstaller()
        {
            ServiceProcessInstaller serviceProcessInstaller =
                new ServiceProcessInstaller();
            ServiceInstaller serviceInstaller = new ServiceInstaller();

            //# Service Account Information
            serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            serviceProcessInstaller.Username = null;
            serviceProcessInstaller.Password = null;

            //# Service Information
            serviceInstaller.DisplayName = GetCultureDependentString(_translatedNames);
            serviceInstaller.StartType = ServiceStartMode.Automatic;

            //# This must be identical to the WindowsService.ServiceBase name
            //# set in the constructor of WindowsService.cs
            serviceInstaller.ServiceName = "WindowsInput";
            serviceInstaller.Description = GetCultureDependentString(_translatedDescriptions);

            Installers.Add(serviceProcessInstaller);
            Installers.Add(serviceInstaller);
        }

        private static string GetCultureDependentString(IDictionary<string, string> dictionary)
        {
            var cultureKey = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;
            if (dictionary.ContainsKey(cultureKey))
                return dictionary[cultureKey];
            return dictionary.First().Value;
        }
    }
}