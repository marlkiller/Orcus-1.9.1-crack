using System.Collections.Generic;
using Orcus.Administration.Core.Build.Configuration;
using Orcus.Shared.Core;
using Orcus.Shared.Settings;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.ClientBuilder
{
    public class BuildConfigurationViewModel : PropertyChangedBase
    {
        private string _clientTag;
        private bool _install;
        private List<IpAddressInfo> _ipAddresses;
        private string _name;

        public BuildConfigurationViewModel(BuildConfigurationInfo buildConfigurationInfo)
        {
            Update(buildConfigurationInfo);
        }

        public BuildConfigurationInfo BuildConfigurationInfo { get; private set; }

        public string Name
        {
            get { return _name; }
            set { SetProperty(value, ref _name); }
        }

        public string ClientTag
        {
            get { return _clientTag; }
            set { SetProperty(value, ref _clientTag); }
        }

        public bool Install
        {
            get { return _install; }
            set { SetProperty(value, ref _install); }
        }

        public List<IpAddressInfo> IpAddresses
        {
            get { return _ipAddresses; }
            set { SetProperty(value, ref _ipAddresses); }
        }

        public void Update(BuildConfigurationInfo buildConfigurationInfo)
        {
            Name = buildConfigurationInfo.BuildConfiguration.Name;
            BuildConfigurationInfo = buildConfigurationInfo;

            Install =
                BuildConfigurationInfo.BuildConfiguration.Settings.GetBuilderProperty<InstallBuilderProperty>().Install;

            ClientTag =
                BuildConfigurationInfo.BuildConfiguration.Settings.GetBuilderProperty<ClientTagBuilderProperty>().ClientTag;

            IpAddresses =
                BuildConfigurationInfo.BuildConfiguration.Settings.GetBuilderProperty<ConnectionBuilderProperty>().IpAddresses;
        }
    }
}