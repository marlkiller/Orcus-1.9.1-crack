using System.IO;
using System.Xml.Serialization;

namespace OrcusPluginStudio.Core
{
    public static class OrcusPluginProjectUtilities
    {
        public static void WriteToFile(this OrcusPluginProject pluginProject, string path)
        {
            var xmls = new XmlSerializer(typeof (OrcusPluginProject));
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                xmls.Serialize(fs, pluginProject);
        }

        public static OrcusPluginProject LoadPluginProject(string path)
        {
            var xmls = new XmlSerializer(typeof (OrcusPluginProject));
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                return (OrcusPluginProject) xmls.Deserialize(fs);
        }
    }
}