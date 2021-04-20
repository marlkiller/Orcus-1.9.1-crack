using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Orcus.Shared.Commands.FileExplorer;

namespace Orcus.Commands.FileExplorer
{
    public class UploadProcess : IDisposable
    {
        private readonly FileStream _fileStream;
        private bool _isDisposed;
        private bool _failed;

        public UploadProcess(string path, byte[] hashValue, long length)
        {
            Path = path;
            HashValue = hashValue;
            Length = length;
            _fileStream = new FileStream(Path, FileMode.Create, FileAccess.ReadWrite);
        }

        public string Path { get; }
        public byte[] HashValue { get;}
        public long Length { get; }

        public void ReceiveData(byte[] data, int index)
        {
            if (_failed)
                return;

            _fileStream.Write(data, index, data.Length - index);
        }

        public UploadResult FinishUpload()
        {
            if (_failed)
                return UploadResult.UploadNotFound;

            if (_fileStream.Length != Length)
            {
                Failed();
                return UploadResult.InvalidFileLength;
            }

            using (var md5Provider = new MD5CryptoServiceProvider())
            {
                _fileStream.Position = 0;
                var hash = md5Provider.ComputeHash(_fileStream);
                if (!hash.SequenceEqual(HashValue))
                {
                    Failed();
                    return UploadResult.HashValuesDoNotMatch;
                }
            }

            Dispose();
            return UploadResult.Succeed;
        }

        public void Failed()
        {
            _failed = true;
            Dispose();
            try
            {
                File.Delete(Path);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _fileStream.Close();
                _isDisposed = true;
            }
        }
    }
}