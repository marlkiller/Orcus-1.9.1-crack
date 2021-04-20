using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Orcus.Administration.Plugins.BuildPlugin;
using Orcus.Plugins.Builder;
using Orcus.Plugins.PropertyGrid;
using Orcus.Shared.Core;
using Orcus.Shared.Settings;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.Views.BuilderViews
{
    /// <summary>
    ///     Interaction logic for ConnectionBuilderPropertyView.xaml
    /// </summary>
    public partial class ConnectionBuilderPropertyView : BuilderPropertyViewUserControl<ConnectionBuilderProperty>, IRequestBuilderInfo
    {
        private RelayCommand _addIpAddressCommand;
        private IBuilderInfo _builderInfo;
        private string _ipAddress;
        private int _port;
        private RelayCommand _removeIpAddressCommand;
        private IpAddressInfo _selectedIpAddress;
        private ObservableCollection<IpAddressInfo> _ipAddresses;

        public ConnectionBuilderPropertyView()
        {
            InitializeComponent();
        }

        public ObservableCollection<IpAddressInfo> IpAddresses
        {
            get { return _ipAddresses; }
            set
            {
                if (_ipAddresses != value)
                {
                    _ipAddresses = value;
                    OnPropertyChanged();
                }
            }
        }

        protected override void OnCurrentBuilderPropertyChanged(ConnectionBuilderProperty newValue)
        {
            SelectedIpAddress =
                BuilderInfo.AvailableIpAddresses.FirstOrDefault(
                    x => x.Ip != "127.0.0.1" && !newValue.IpAddresses.Contains(x)) ??
                BuilderInfo.AvailableIpAddresses.FirstOrDefault(x => !newValue.IpAddresses.Contains(x)) ??
                BuilderInfo.AvailableIpAddresses.First();

            IpAddresses = new ObservableCollection<IpAddressInfo>(newValue.IpAddresses);
        }

        public override InputValidationResult ValidateInput(List<IBuilderProperty> currentBuilderProperties, ConnectionBuilderProperty currentBuilderProperty)
        {
            if (currentBuilderProperty.IpAddresses.Count == 0)
                return InputValidationResult.Error("@ErrorIpAddress");

            return InputValidationResult.Successful;
        }

        public IpAddressInfo SelectedIpAddress
        {
            get { return _selectedIpAddress; }
            set
            {
                if (_selectedIpAddress != value)
                {
                    _selectedIpAddress = value;
                    OnPropertyChanged();

                    if (value != null)
                    {
                        Port = value.Port;
                    }
                }
            }
        }

        public int Port
        {
            get { return _port; }
            set
            {
                if (value != _port)
                {
                    _port = value;
                    OnPropertyChanged();
                }
            }
        }

        public string IpAddress
        {
            get { return _ipAddress; }
            set
            {
                if (value != _ipAddress)
                {
                    _ipAddress = value;
                    OnPropertyChanged();
                }
            }
        }

        public RelayCommand AddIpAddressCommand
        {
            get
            {
                return _addIpAddressCommand ?? (_addIpAddressCommand = new RelayCommand(parameter =>
                {
                    if (string.IsNullOrWhiteSpace(IpAddress))
                        return;

                    if (
                        CurrentBuilderProperty.IpAddresses.Any(
                            x => x.Ip == IpAddress && x.Port == Port))
                        return; //handling required

                    var ipAddressInfo = new IpAddressInfo {Ip = IpAddress, Port = Port};
                    IpAddresses.Add(ipAddressInfo);
                    CurrentBuilderProperty.IpAddresses.Add(ipAddressInfo);
                }));
            }
        }

        public RelayCommand RemoveIpAddressCommand
        {
            get
            {
                return _removeIpAddressCommand ?? (_removeIpAddressCommand = new RelayCommand(parameter =>
                {
                    var ipAddress = parameter as IpAddressInfo;
                    if (ipAddress == null)
                        return;

                    IpAddresses.Remove(ipAddress);
                    CurrentBuilderProperty.IpAddresses.Remove(ipAddress);
                }));
            }
        }

        public override string[] Tags { get; } = {"ip", "addresses", "adressen", "verbindung", "connection", "server", "dns"};

        public override BuilderPropertyPosition PropertyPosition { get; } =
            BuilderPropertyPosition.FromCategory(BuilderCategory.Connection);

        public IBuilderInfo BuilderInfo
        {
            get { return _builderInfo; }
            set
            {
                _builderInfo = value;
                OnPropertyChanged();
            }
        }
    }
}