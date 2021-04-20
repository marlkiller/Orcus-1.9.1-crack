using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace OrcusPluginStudio.Core.Settings
{
    public class OrcusPluginStudioSettings
    {
        private const string Path = "settings.xml";
        private static OrcusPluginStudioSettings _current;

        public OrcusPluginStudioSettings()
        {
            RecentEntries = new List<RecentEntry>();
        }

        public static OrcusPluginStudioSettings Current => _current ?? (_current = new OrcusPluginStudioSettings());

        public List<RecentEntry> RecentEntries { get; set; }

        public static void LoadSettings()
        {
            if (File.Exists(Path))
                using (var fs = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var xmls = new XmlSerializer(typeof (OrcusPluginStudioSettings));
                    var result = (OrcusPluginStudioSettings) xmls.Deserialize(fs);
                    _current = result;
                }
        }

        public void Save()
        {
            using (var fs = new FileStream(Path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var xmls = new XmlSerializer(typeof (OrcusPluginStudioSettings));
                xmls.Serialize(fs, this);
            }
        }
    }
}