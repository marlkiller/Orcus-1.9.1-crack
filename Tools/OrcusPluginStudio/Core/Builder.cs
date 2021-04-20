using System.IO;
using System.IO.Compression;
using System.Xml.Serialization;
using Orcus.Plugins;

namespace OrcusPluginStudio.Core
{
    public class Builder
    {
        public static void BuildPlugin(OrcusPluginProject project, string path)
        {
            var pluginInfo = new PluginInfo
            {
                Author = project.PluginInformation.Author,
                AuthorUrl = project.PluginInformation.AuthorUrl,
                Description = project.PluginInformation.Description,
                Name = project.PluginInformation.Name,
                Guid = project.PluginInformation.Guid,
                Version = project.PluginInformation.Version,
                PluginType = project.PluginType
            };

            using (var fileStream = new FileStream(path, FileMode.Create))
            using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
            {
                var thumbnailEntry = archive.CreateEntry("thumbnail" + Path.GetExtension(project.ThumbnailPath),
                    CompressionLevel.Optimal);
                WriteFileToArchiveEntry(thumbnailEntry, project.ThumbnailPath);
                pluginInfo.Thumbnail = thumbnailEntry.Name;


                var libraryFile = new FileInfo(project.Library1Path);
                var libraryEntry = archive.CreateEntry(libraryFile.Name, CompressionLevel.Optimal);
                WriteFileToArchiveEntry(libraryEntry, libraryFile.FullName);
                pluginInfo.Library1 = libraryEntry.Name;

                if (!string.IsNullOrEmpty(project.Library2Path))
                {
                    var libraryFile2 = new FileInfo(project.Library2Path);
                    var fileName = libraryFile2.Name;
                    if (fileName == libraryFile.Name)
                        fileName = Path.GetFileNameWithoutExtension(libraryFile2.FullName) + "_2.dll";

                    var libraryEntry2 = archive.CreateEntry(fileName, CompressionLevel.Optimal);
                    WriteFileToArchiveEntry(libraryEntry2, libraryFile2.FullName);
                    pluginInfo.Library2 = libraryEntry2.Name;
                }

                var infoFile = archive.CreateEntry("PluginInfo.xml", CompressionLevel.Optimal);
                using (var infoStream = infoFile.Open())
                {
                    var xmls = new XmlSerializer(typeof (PluginInfo));
                    xmls.Serialize(infoStream, pluginInfo);
                }
            }
        }

        private static void WriteFileToArchiveEntry(ZipArchiveEntry archiveEntry, string path)
        {
            using (var thumbnailStream = archiveEntry.Open())
            using (
                var localThumbnailStream = new FileStream(path, FileMode.Open, FileAccess.Read)
                )
                localThumbnailStream.CopyTo(thumbnailStream);
        }
    }
}