using CSCore.CoreAudioAPI;
using Orcus.Shared.Commands.VoiceChat;

namespace Orcus.Administration.ViewModels.CommandViewModels.VoiceChat
{
    public class LocalCaptureDeviceInfo : CaptureDeviceInfo
    {
        public MMDevice Device { get; set; }
    }
}