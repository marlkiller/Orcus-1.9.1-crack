using System;
using System.Collections.Generic;

namespace Orcus.Shared.Commands.UserInteraction
{
    [Serializable]
    public class UserInteractionWelcomePackage
    {
        public List<SpeechVoice> Voices { get; set; }
    }
}