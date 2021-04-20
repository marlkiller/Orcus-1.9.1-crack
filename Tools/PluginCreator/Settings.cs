using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace PluginCreator
{
    public class Settings
    {
        private string _path;

        public Settings(string path)
        {
            PluginData = new List<PluginData>();
            _path = path;
        }

        private Settings() : this(null)
        {
        }

        public List<PluginData> PluginData { get; set; }

        public static Settings LoadSettings(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var xmls = new XmlSerializer(typeof (Settings));
                var result = (Settings) xmls.Deserialize(fs);
                result._path = path;
                return result;
            }
        }

        public void Save()
        {
            using (var fs = new FileStream(_path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var xmls = new XmlSerializer(typeof (Settings));
                xmls.Serialize(fs, this);
            }
        }
    }
}