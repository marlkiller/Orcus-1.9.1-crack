using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using System.Xml;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.AudioPlugin;

namespace AudioPackBuilder.Plugin
{
    public class Plugin : IAudioPlugin
    {
        public IEnumerable<IAudioFile> AudioFiles
        {
            get
            {
                return
                    new JavaScriptSerializer().Deserialize<List<RawAudioFile>>(DataProvider.Data)
                        .Select(
                            x =>
                                new AudioFile(x.EmbeddedResourceName, XmlConvert.ToTimeSpan(x.Timespan), x.AudioGenre,
                                    x.Name, x.EmbeddedThumbnailResourceName));
            }
        }
    }
}