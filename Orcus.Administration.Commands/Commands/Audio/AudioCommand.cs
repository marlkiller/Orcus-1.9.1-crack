using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.AudioPlugin;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Plugins;
using Orcus.Shared.Commands.Audio;
using Orcus.Shared.Connection;
using Orcus.Shared.NetSerializer;
using Command = Orcus.Administration.Plugins.CommandViewPlugin.Command;

namespace Orcus.Administration.Commands.Audio
{
    [ProvideLibrary(PortableLibrary.CSCore)]
    [DescribeCommandByEnum(typeof (AudioCommunication))]
    public class AudioCommand : Command
    {
        public event EventHandler<List<SoundOutDevice>> AudioDevicesReceived;

        public override void ResponseReceived(byte[] parameter)
        {
            switch ((AudioCommunication) parameter[0])
            {
                case AudioCommunication.ResponseDevices:
                    AudioDevicesReceived?.Invoke(this,
                        new JavaScriptSerializer().Deserialize<List<SoundOutDevice>>(
                            Encoding.UTF8.GetString(parameter, 1, parameter.Length - 1)));
                    LogService.Receive((string) Application.Current.Resources["AudioDevices"]);
                    break;
                case AudioCommunication.ResponseAudioIsPlaying:
                    LogService.Receive((string) Application.Current.Resources["AudioIsNowPlaying"]);
                    break;
                case AudioCommunication.ResponseNotSupported:
                    LogService.Error((string) Application.Current.Resources["PlatformNotSupported"]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void GetDevices()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) AudioCommunication.GetDevices});
            LogService.Send((string) Application.Current.Resources["GetSoundDevices"]);
        }

        public void PlayAudio(IAudioFile audioFile, SoundOutDevice soundOutDevice, float volume)
        {
            var result = new PlayAudioInformation
            {
                SoundOutId = soundOutDevice.Id,
                Volume = volume,
                AudioData = audioFile.Data
            };

            var package = new List<byte> {(byte) AudioCommunication.PlayAudio};
            var serializer = new Serializer(typeof (PlayAudioInformation));
            package.AddRange(serializer.Serialize(result));
            ConnectionInfo.SendCommand(this, package.ToArray());
            LogService.Send(string.Format((string) Application.Current.Resources["PlayAudio"], audioFile.Name,
                soundOutDevice.Name, volume));
        }

        protected override uint GetId()
        {
            return 1;
        }
    }
}