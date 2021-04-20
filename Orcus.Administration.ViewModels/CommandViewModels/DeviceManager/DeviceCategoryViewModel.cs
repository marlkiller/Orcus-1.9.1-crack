using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MahApps.Metro.IconPacks;
using Orcus.Shared.Commands.DeviceManager;
using Orcus.Shared.Utilities;

namespace Orcus.Administration.ViewModels.CommandViewModels.DeviceManager
{
    public class DeviceCategoryViewModel : IDeviceEntryViewModel
    {
        private DeviceCategoryViewModel(IEnumerable<DeviceInfo> childDevices)
        {
            ChildDevices = childDevices.Select(x => new DeviceViewModel(x, this)).OrderBy(x => x.Caption).ToList();
        }

        public DeviceCategoryViewModel(IEnumerable<DeviceInfo> childDevices, DeviceCategory deviceCategory)
            : this(childDevices)
        {
            DeviceCategory = deviceCategory;
            Caption = DeviceCategoryToString(deviceCategory);
            Icon = GetDeviceCategoryIcon(deviceCategory);
        }

        public DeviceCategoryViewModel(IEnumerable<DeviceInfo> childDevices, string deviceCategory) : this(childDevices)
        {
            DeviceCategory = DeviceCategory.None;
            Caption = deviceCategory;
            Icon = PackIconMaterialKind.CubeOutline;
        }

        public List<DeviceViewModel> ChildDevices { get; }
        public DeviceCategory DeviceCategory { get; }

        public string Caption { get; }
        public bool DisplayWarning { get; }
        public string WarningMessage { get; }
        public PackIconMaterialKind Icon { get; }
        public bool IsCategory { get; } = true;

        private static PackIconMaterialKind GetDeviceCategoryIcon(DeviceCategory deviceCategory)
        {
            switch (deviceCategory)
            {
                case DeviceCategory.AudioEndpoint:
                case DeviceCategory.Multimedia:
                    return PackIconMaterialKind.VolumeLow;
                case DeviceCategory.Computer:
                    return PackIconMaterialKind.Television;
                case DeviceCategory.PrintQueue:
                case DeviceCategory.Printers:
                case DeviceCategory.Printers_BusSpecificClassDrivers:
                    return PackIconMaterialKind.Printer;
                case DeviceCategory.CDROMDrives:
                    return PackIconMaterialKind.Disk;
                case DeviceCategory.HumanInterfaceDevicesHID:
                    return PackIconMaterialKind.KeyboardVariant;
                case DeviceCategory.DiskDrives:
                case DeviceCategory.StorageVolumes:
                case DeviceCategory.StorageVolumeSnapshots:
                    return PackIconMaterialKind.Harddisk;
                case DeviceCategory.Keyboard:
                    return PackIconMaterialKind.Keyboard;
                case DeviceCategory.DisplayAdapters:
                    return PackIconMaterialKind.Monitor;
                case DeviceCategory.Monitor:
                    return PackIconMaterialKind.MonitorMultiple;
                case DeviceCategory.USBDevice:
                case DeviceCategory.USBBusDevices_hubsandhostcontrollers:
                    return PackIconMaterialKind.Usb;
                case DeviceCategory.Mouse:
                    return PackIconMaterialKind.Mouse;
                case DeviceCategory.WSDPrintDevice:
                    return PackIconMaterialKind.Printer;
                case DeviceCategory.SCSIandRAIDControllers:
                    return PackIconMaterialKind.Raspberrypi;
                case DeviceCategory.NetworkService:
                case DeviceCategory.NetworkAdapter:
                case DeviceCategory.NetworkClient:
                case DeviceCategory.NetworkTransport:
                    return PackIconMaterialKind.LanConnect;
                case DeviceCategory.Processors:
                    return PackIconMaterialKind.Raspberrypi;
                case DeviceCategory.Sensors:
                    return PackIconMaterialKind.AccessPoint;
                case DeviceCategory.BatteryDevices:
                    return PackIconMaterialKind.Battery50;
                default:
                    return PackIconMaterialKind.CubeOutline;
            }
        }

        private static string DeviceCategoryToString(DeviceCategory deviceCategory)
        {
            switch (deviceCategory)
            {
                case DeviceCategory.None:
                    return (string) Application.Current.Resources["Other"];
                case DeviceCategory.NetworkAdapter:
                    return (string) Application.Current.Resources["NetworkAdapters"];
                case DeviceCategory.SystemDevices:
                    return (string) Application.Current.Resources["SystemDevices"];
                case DeviceCategory.HumanInterfaceDevicesHID:
                    return (string) Application.Current.Resources["HumanInterfaceDevices"];
                case DeviceCategory.AudioEndpoint:
                    return (string) Application.Current.Resources["AudioInputsAndOutputs"];
                case DeviceCategory.HardDiskControllers:
                    return (string) Application.Current.Resources["IdeAtaAtapiControllers"];
                case DeviceCategory.SoftwareDevices:
                    return (string) Application.Current.Resources["SoftwareDevices"];
                case DeviceCategory.ImagingDevice:
                    return (string) Application.Current.Resources["ImageEditing"];
                case DeviceCategory.Multimedia:
                    return (string) Application.Current.Resources["SoundVideoAndGameControllers"];
                case DeviceCategory.DiskDrives:
                    return (string) Application.Current.Resources["DiskDrives"];
                case DeviceCategory.PrintQueue:
                    return (string) Application.Current.Resources["PrintQueues"];
                case DeviceCategory.USBDevice:
                    return (string) Application.Current.Resources["UsbControllers"];
                case DeviceCategory.Keyboard:
                    return (string) Application.Current.Resources["Keyboards"];
                case DeviceCategory.Processors:
                    return (string) Application.Current.Resources["Processors"];
                case DeviceCategory.CDROMDrives:
                    return (string) Application.Current.Resources["DvdCdRomDrives"];
                case DeviceCategory.WindowsPortableDevices_WPD:
                    return (string) Application.Current.Resources["PortableDevices"];
                case DeviceCategory.WSDPrintDevice:
                    return (string) Application.Current.Resources["WsdPrintDevices"];
                case DeviceCategory.SCSIandRAIDControllers:
                    return (string) Application.Current.Resources["StorageControllers"];
                case DeviceCategory.Monitor:
                    return (string) Application.Current.Resources["Monitors"];
                case DeviceCategory.PortsCOM_LPTports:
                    return (string) Application.Current.Resources["PortsComLpt"];
                case DeviceCategory.DisplayAdapters:
                    return (string) Application.Current.Resources["DisplayAdapters"];
                case DeviceCategory.Mouse:
                    return (string) Application.Current.Resources["MiceAndOtherPointingDevices"];
                case DeviceCategory.Computer:
                    return (string) Application.Current.Resources["Computer"];
                case DeviceCategory.Sensors:
                    return (string) Application.Current.Resources["Sensors"];
                case DeviceCategory.Printers:
                    return (string) Application.Current.Resources["Printers"];
                case DeviceCategory.NetworkService:
                    break;
                default:
                    return deviceCategory.GetAttributeOfType<DeviceCategoryDisplayNameAttribute>().DisplayName;
            }
            return deviceCategory.ToString();
        }
    }
}