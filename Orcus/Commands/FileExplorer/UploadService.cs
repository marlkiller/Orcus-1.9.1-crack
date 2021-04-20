using System;
using System.Collections.Generic;
using Orcus.Shared.Commands.FileExplorer;

namespace Orcus.Commands.FileExplorer
{
    public class UploadFinishedEventArgs : EventArgs
    {
        public UploadFinishedEventArgs(UploadProcess uploadProcess)
        {
            UploadProcess = uploadProcess;
        }

        public UploadProcess UploadProcess { get; }
    }

    public class UploadService : IDisposable
    {
        private readonly Dictionary<Guid, UploadProcess> _uploadProcesses;
        private readonly object _lockObject = new object();

        public UploadService()
        {
            _uploadProcesses = new Dictionary<Guid, UploadProcess>();
        }

        public Guid CreateNewUploadProcess(string path, byte[] hashValue, long length)
        {
            var guid = Guid.NewGuid();
            lock (_lockObject)
                _uploadProcesses.Add(guid, new UploadProcess(path, hashValue, length));

            return guid;
        }

        public void CancelUpload(Guid guid)
        {
            lock (_lockObject)
            {
                UploadProcess uploadProcess;
                if (_uploadProcesses.TryGetValue(guid, out uploadProcess))
                {
                    uploadProcess.Failed();
                    _uploadProcesses.Remove(guid);
                }
            }
        }

        public void ReceivePackage(Guid guid, byte[] bytes, int startIndex)
        {
            lock (_lockObject)
            {
                UploadProcess uploadProcess;
                if (_uploadProcesses.TryGetValue(guid, out uploadProcess))
                    uploadProcess.ReceiveData(bytes, startIndex);
            }
        }

        public UploadResult FinishUpload(Guid guid)
        {
            lock (_lockObject)
            {
                UploadProcess uploadProcess;
                if (_uploadProcesses.TryGetValue(guid, out uploadProcess))
                {
                    _uploadProcesses.Remove(guid);
                    return uploadProcess.FinishUpload();
                }
            }

            return UploadResult.UploadNotFound;
        }

        public void Dispose()
        {
            lock (_lockObject)
            {
                foreach (var uploadProcess in _uploadProcesses)
                    uploadProcess.Value.Failed();
                _uploadProcesses.Clear();
            }
        }
    }
}