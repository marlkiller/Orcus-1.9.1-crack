using Orcus.Shared.Commands.DeviceManager;

namespace Orcus.Administration.Commands.DeviceManager
{
    public static class DeviceInfoExtensions
    {
        public static string GetStatusErrorMessage(this DeviceInfo deviceInfo)
        {
            switch (deviceInfo.StatusCode)
            {
                case 0:
                    return "Device is working properly.";
                case 1:
                    return "This device is not configured correctly.";
                case 2:
                    return "Windows cannot load the driver for this device.";
                case 3:
                    return
                        "The driver for this device might be corrupted, or your system may be running low on memory or other resources.";
                case 4:
                    return "Device is not working properly. One of its drivers or the registry might be corrupted.";
                case 5:
                    return "Driver for the device requires a resource that Windows cannot manage.";
                case 6:
                    return "Boot configuration for the device conflicts with other devices.";
                case 7:
                    return "Cannot filter.";
                case 8:
                    return "The driver loader for the device is missing.";
                case 9:
                    return
                        "Device is not working properly. The controlling firmware is incorrectly reporting the resources for the device.";
                case 10:
                    return "Device cannot start.";
                case 11:
                    return "Device failed.";
                case 12:
                    return "Device cannot find enough free resources to use.";
                case 13:
                    return "Windows cannot verify the device's resources.";
                case 14:
                    return "Device cannot work properly until the computer is restarted.";
                case 15:
                    return "Device is not working properly due to a possible re-enumeration problem.";
                case 16:
                    return "Windows cannot identify all of the resources that the device uses.";
                case 17:
                    return "Device is requesting an unknown resource type.";
                case 18:
                    return "Device drivers must be reinstalled.";
                case 19:
                    return "Failure using the VxD loader.";
                case 20:
                    return "Registry might be corrupted.";
                case 21:
                    return
                        "System failure. If changing the device driver is ineffective, see the hardware documentation. Windows is removing the device.";
                case 22:
                    return "Device is disabled.";
                case 23:
                    return
                        "System failure. If changing the device driver is ineffective, see the hardware documentation.";
                case 24:
                    return "Device is not present, not working properly, or does not have all of its drivers installed.";
                case 25:
                case 26:
                    return "Windows is still setting up the device.";
                case 27:
                    return "Device does not have valid log configuration.";
                case 28:
                    return "Device drivers are not installed.";
                case 29:
                    return "Device is disabled. The device firmware did not provide the required resources.";
                case 30:
                    return "Device is using an IRQ resource that another device is using.";
                case 31:
                    return "Device is not working properly. Windows cannot load the required device drivers.";
                default:
                    return $"Unknow status code ({deviceInfo.StatusCode})";
            }
        }
    }
}