using System;
using System.IO;
using System.IO.Packaging;

namespace Orcus.Shared.Utilities.Compression
{
    public static class Packages
    {
        [Obsolete("Please use ZIP")]
        public static void ExtractFilesFromPackage(string packagePath, string output)
        {
            var pack = Package.Open(packagePath, FileMode.Open);

            var collection = pack.GetParts();

            foreach (var part in collection)
            {
                var resourcestream = part.GetStream(FileMode.Open, FileAccess.Read);
                ExtractFile(resourcestream, output, part.Uri.OriginalString);
            }
            pack.Close();
        }

        private static void ExtractFile(Stream stream, string packagePath, string filePath)
        {
            //The below line is for Unicode support
            var filePathCleared = Uri.UnescapeDataString(filePath);
            var finfo = new FileInfo(packagePath);
            var directory = finfo.Directory.FullName;
            var fileName = directory + "//" + filePathCleared;

            finfo = new FileInfo(fileName);

            var dInfo = new DirectoryInfo(finfo.Directory.FullName);

            if (!dInfo.Exists)
            {
                dInfo.Create();
            }

            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                CopyStream(stream, fs);
            }
        }

        [Obsolete("Please use ZIP")]
        public static void CreatePackage(string folderPath, string outputFile)
        {
            var dInfoParent = new DirectoryInfo(folderPath);

            using (var package = Package.Open(outputFile, FileMode.Create))
            {
                CreatePackageForEachFolder(package, dInfoParent, dInfoParent.Name);
            }
        }

        private static void CreatePackageForEachFolder(Package package, DirectoryInfo parentDirectoryInfo, string partName)
        {
            foreach (var file in parentDirectoryInfo.GetFiles())
            {
                var fileUri = PackUriHelper.CreatePartUri(new Uri(partName + "//" + file.Name, UriKind.Relative));
                // Add the Document part to the Package
                var packagePartDocument =
                    package.CreatePart(fileUri,
                        System.Net.Mime.MediaTypeNames.Application.Octet, CompressionOption.Maximum);

                // Copy the data to the Document Part
                using (var fileStream = new FileStream(
                    parentDirectoryInfo.FullName + "//" + file.Name, FileMode.Open, FileAccess.Read))
                {
                    CopyStream(fileStream, packagePartDocument.GetStream());
                }
            }

            foreach (var dInfo in parentDirectoryInfo.GetDirectories())
            {
                CreatePackageForEachFolder(package, dInfo, partName + "//" + dInfo.Name);
            }
        }

        private static void CopyStream(Stream source, Stream target)
        {
            const int bufSize = 0x1000;
            var buf = new byte[bufSize];
            int bytesRead;
            while ((bytesRead = source.Read(buf, 0, bufSize)) > 0)
                target.Write(buf, 0, bytesRead);
        }
    }
}