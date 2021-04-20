using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.AudioPlugin;

namespace AudioPackBuilder.Build
{
    public class RawAudioFile
    {
        public string EmbeddedResourceName { get; set; }
        public string Name { get; set; }
        public string Timespan { get; set; }
        public AudioGenre AudioGenre { get; set; }
        public string EmbeddedThumbnailResourceName { get; set; }
    }
}