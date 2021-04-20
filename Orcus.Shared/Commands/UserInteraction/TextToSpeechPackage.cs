using System;

namespace Orcus.Shared.Commands.UserInteraction
{
    [Serializable]
    public class TextToSpeechPackage
    {
        public string VoiceName { get; set; }
        //-10 - 10
        public sbyte Speed { get; set; }
        //0-100
        public int Volume { get; set; }

        public string Text { get; set; }
    }
}