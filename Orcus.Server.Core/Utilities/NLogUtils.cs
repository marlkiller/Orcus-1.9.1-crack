using System.IO;
using System.Reflection;

namespace Orcus.Server.Core.Utilities
{
    public static class NLogUtils
    {
        private const string ConfigFileName = "NLog.config";

        public static void CreateConfigFileIfNotExists()
        {
            var file = new FileInfo(ConfigFileName);
            if (file.Exists)
                return;

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"Orcus.Server.Core.{ConfigFileName}";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var fileStream = file.OpenWrite())
            {
                stream.CopyTo(fileStream);
            }
        }
    }
}