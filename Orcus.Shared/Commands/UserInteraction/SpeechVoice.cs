using System;

namespace Orcus.Shared.Commands.UserInteraction
{
    [Serializable]
    public class SpeechVoice
    {
        public string Name { get; set; }
        public SpeechAge VoiceAge { get; set; }
        public string Culture { get; set; }
        public SpeechGender VoiceGender { get; set; }
    }

    [Serializable]
    public enum SpeechAge : byte
    {
        NotSet,
        Child = 10,
        Teen = 15,
        Adult = 30,
        Senior = 65
    }

    [Serializable]
    public enum SpeechGender
    {
        NotSet,
        Male,
        Female,
        Neutral
    }
}