using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace Orcus.Utilities
{
    public static class ResourceHelper
    {
        public static void WriteGZippedResourceToFile(string filename, string resourceName)
        {
            var resourceStream = Assembly.GetEntryAssembly().GetManifestResourceStream(resourceName);
            if (resourceStream == null)
                throw new FileNotFoundException();

            using (resourceStream)
            using (var fileStream = new FileStream(filename, FileMode.Create))
            using (var gzipStream = new GZipStream(resourceStream, CompressionMode.Decompress))
            {
                int read;
                var buffer = new byte[4096];
                while ((read = gzipStream.Read(buffer, 0, buffer.Length)) > 0)
                    fileStream.Write(buffer, 0, read);
            }
        }
    }
}