namespace Orcus.Shared.Commands.VoiceChat
{
    public enum VoiceChatCommunication
    {
        StartLocalStreaming,
        SendAudioPackage,
        StartRemoteStreaming,
        GetRemoteAudioDevices,
        ResponseAudioDevices,
        ResponseAudioPackage,
        StopLocalStreaming,
        StopRemoteStreaming
    }
}