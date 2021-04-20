using System.Collections.Generic;
using MahApps.Metro.IconPacks;
using Orcus.Administration.Commands.DeviceManager;
using Orcus.Shared.Commands.DeviceManager;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels.DeviceManager
{
    public class DeviceViewModel : PropertyChangedBase, IDeviceEntryViewModel
    {
        private bool _displayWarning;

        public DeviceViewModel(DeviceInfo deviceInfo, DeviceCategoryViewModel deviceCategoryViewModel)
        {
            DeviceInfo = deviceInfo;
            StatusErrorMessage = deviceInfo.GetStatusErrorMessage();
            DeviceCategoryViewModel = deviceCategoryViewModel;
            DisplayWarning = deviceInfo.StatusCode != 0;
            ChildDevices = new List<DeviceViewModel>(0);
            Caption = deviceInfo.Name;
        }

        public string StatusErrorMessage { get; }
        public DeviceInfo DeviceInfo { get; }
        public DeviceCategoryViewModel DeviceCategoryViewModel { get; }
        public List<DeviceViewModel> ChildDevices { get; }

        public bool DisplayWarning
        {
            get { return _displayWarning; }
            set { SetProperty(value, ref _displayWarning); }
        }

        public string WarningMessage => StatusErrorMessage;

        public string Caption { get; }
        public PackIconMaterialKind Icon => DeviceCategoryViewModel.Icon;
        public bool IsCategory { get; } = false;
    }
}