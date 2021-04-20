using System.Collections.Generic;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.AudioPlugin;

namespace OrcusPluginStudio.Core.Test.ManualTests
{
    public class AudioPluginTest : IManualTest
    {
        private readonly IAudioPlugin _audioPlugin;

        public AudioPluginTest(IAudioPlugin audioPlugin, List<IAudioFile> audioFiles)
        {
            AudioFiles = audioFiles;
            _audioPlugin = audioPlugin;
        }

        public List<IAudioFile> AudioFiles { get; }

        public void Dispose()
        {
            foreach (var audioFile in AudioFiles)
                audioFile.Dispose();
        }
    }
}