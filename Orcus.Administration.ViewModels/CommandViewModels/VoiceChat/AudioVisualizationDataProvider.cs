using System;

namespace Orcus.Administration.ViewModels.CommandViewModels.VoiceChat
{
    public class AudioVisualizationDataProvider
    {
        public void AddSamples(float left, float right)
        {
            SamplesAdded?.Invoke(this, new Tuple<float, float>(left, right));
        }

        public event EventHandler<Tuple<float, float>> SamplesAdded;
    }
}