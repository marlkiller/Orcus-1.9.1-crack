using System;
using System.IO;

namespace Orcus.Server.Core.Database.FileSystem
{
    public static class DataSystem
    {
        public static Guid StoreData(byte[] data)
        {
            string dataFileName;
            var fileNameGuid = GetFreeGuid(out dataFileName);

            File.WriteAllBytes(dataFileName, data);

            return fileNameGuid;
        }

        public static Guid StoreFile(string filename)
        {
            string dataFileName;
            var fileNameGuid = GetFreeGuid(out dataFileName);

            File.Move(filename, dataFileName);

            return fileNameGuid;
        }

        public static FileInfo GetFile(Guid guid)
        {
            var dataDirectory = new DirectoryInfo("data");
            var file = new FileInfo(Path.Combine(dataDirectory.FullName, guid.ToString("D")));
            
            return file.Exists ? file : null;
        }

        public static byte[] GetData(Guid guid)
        {
            var dataDirectory = new DirectoryInfo("data");
            var file = new FileInfo(Path.Combine(dataDirectory.FullName, guid.ToString("D")));

            return file.Exists ? File.ReadAllBytes(file.FullName) : null;
        }

        public static Guid GetFreeGuid(out string filename)
        {
            var dataDirectory = new DirectoryInfo("data");
            if (!dataDirectory.Exists)
                dataDirectory.Create();

            Guid fileNameGuid;
            var dataFileName = Path.Combine(dataDirectory.FullName, (fileNameGuid = Guid.NewGuid()).ToString("D"));
            while (File.Exists(dataFileName))
            {
                dataFileName = Path.Combine(dataDirectory.FullName, (fileNameGuid = Guid.NewGuid()).ToString("D"));
            }

            filename = dataFileName;
            return fileNameGuid;
        }
    }
}