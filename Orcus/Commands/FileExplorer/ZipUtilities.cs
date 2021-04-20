using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.LZW;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using Orcus.Shared.Commands.FileExplorer;
using Orcus.Utilities;
using CompressionMethod = Orcus.Shared.Commands.FileExplorer.CompressionMethod;

namespace Orcus.Commands.FileExplorer
{
    //DO NOT MOVE TO Utilities NAMESPACE, THE LIBRAR IS ONLY AVAILABLE FOR THIS COMMAND
    public static class ZipUtilities
    {
        public delegate void ReportCompressionStatus(ProcessingEntry processingEntry);
        public delegate void AddProcessingEntry(ProcessingEntry processingEntry);

        private static void CollectFiles(List<FileInfo> files, DirectoryInfo directoryInfo)
        {
            foreach (var fileInfo in directoryInfo.GetFiles())
                files.Add(fileInfo);

            foreach (var directory in directoryInfo.GetDirectories())
                CollectFiles(files, directory);
        }

        public static void ExtractArchive(string archivePath, string destinationDirectory, CancellationTokenEx cancellationToken,
            ReportCompressionStatus reportCompressionStatus, AddProcessingEntry addProcessingEntry)
        {
            bool isTar;
            Stream inputStream = new FileStream(archivePath, FileMode.Open, FileAccess.Read);

            switch (Path.GetExtension(archivePath).ToUpper())
            {
                case ".ZIP":
                    var fastZip = new FastZip();
                    fastZip.ExtractZip(inputStream, destinationDirectory, FastZip.Overwrite.Always, null, null, null,
                        true, true);
                    //TODO: Add progress
                    return;
                case ".TAR":
                    isTar = true;
                    break;
                case ".GZ":
                    inputStream = new GZipInputStream(inputStream) {IsStreamOwner = true};
                    isTar = archivePath.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase);
                    break;
                case ".BZ2":
                    inputStream = new BZip2InputStream(inputStream) {IsStreamOwner = true};
                    isTar = archivePath.EndsWith(".tar.bz2", StringComparison.OrdinalIgnoreCase);
                    break;
                case ".LZW":
                    inputStream = new LzwInputStream(inputStream) {IsStreamOwner = true};
                    isTar = archivePath.EndsWith(".tar.lzw", StringComparison.OrdinalIgnoreCase);
                    break;
                default:
                    inputStream.Dispose();
                    return;
            }

            Directory.CreateDirectory(destinationDirectory);

            using (inputStream)
            {
                if (isTar)
                {
                    using (TarArchive tarArchive = TarArchive.CreateInputTarArchive(inputStream))
                    {
                        TarEntry lastEntry = null;
                        ProcessingEntry lastProcessingEntry = null;

                        tarArchive.ProgressMessageEvent += (archive, entry, message) =>
                        {
                            if (lastEntry != entry)
                            {
                                if (lastEntry != null)
                                {
                                    lastProcessingEntry.Progress = 1;
                                    lastProcessingEntry.Size = entry.Size;
                                    ThreadPool.QueueUserWorkItem(state => reportCompressionStatus(lastProcessingEntry));
                                }

                                lastEntry = entry;
                                lastProcessingEntry = new ProcessingEntry
                                {
                                    Action = ProcessingEntryAction.Extracting,
                                    CreationTime = DateTime.UtcNow,
                                    Path = entry.File,
                                    Progress = 0,
                                    Name = entry.Name
                                };
                                ThreadPool.QueueUserWorkItem(state => addProcessingEntry(lastProcessingEntry));
                            }
                        };
                        tarArchive.ExtractContents(destinationDirectory);
                    }
                }
                else
                {
                    var filename = Path.GetFileNameWithoutExtension(archivePath);
                    var destinationFilePath = Path.Combine(destinationDirectory, filename);

                    var processingEntry = new ProcessingEntry
                    {
                        Action = ProcessingEntryAction.Extracting,
                        CreationTime = DateTime.UtcNow,
                        IsDirectory = false,
                        IsInterminate = false,
                        LastAccess = DateTime.UtcNow,
                        Path = destinationFilePath,
                        Name = filename
                    };

                    byte[] dataBuffer = new byte[4096];
                    using (var destinationFileStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        addProcessingEntry(processingEntry);

                        StreamUtils.Copy(inputStream, destinationFileStream, dataBuffer, (sender, args) =>
                            {
                                processingEntry.Progress = args.PercentComplete / 100;
                                processingEntry.Size = destinationFileStream.Length;
                                args.ContinueRunning = !cancellationToken.IsCanceled;
                                ThreadPool.QueueUserWorkItem(state => reportCompressionStatus.Invoke(processingEntry));
                            },
                            TimeSpan.FromSeconds(1), null, null);

                        if (cancellationToken.IsCanceled)
                        {
                            processingEntry.Progress = -1;
                            ThreadPool.QueueUserWorkItem(state => reportCompressionStatus.Invoke(processingEntry));
                            return;
                        }

                        processingEntry.Progress = 1;
                        processingEntry.Size = destinationFileStream.Length;
                        ThreadPool.QueueUserWorkItem(state => reportCompressionStatus.Invoke(processingEntry));
                    }
                }
            }
        }

        public static void CreateArchive(ArchiveOptions archiveOptions, ProcessingEntry processingEntry,
            CancellationTokenEx cancellationToken, ReportCompressionStatus reportCompressionStatus)
        {
            var folderName = new Lazy<string>(() =>
            {
                var firstEntry = archiveOptions.Entries[0];
                return (firstEntry.IsDirectory
                    ? new DirectoryInfo(firstEntry.Path).Parent.FullName
                    : new FileInfo(firstEntry.Path).DirectoryName).TrimEnd('\\');
            });

            Stream outputStream = new FileStream(archiveOptions.ArchivePath, FileMode.Create, FileAccess.ReadWrite);

            switch (archiveOptions.CompressionMethod)
            {
                case CompressionMethod.None:
                    //dont wrap the stream
                    break;
                case CompressionMethod.Zip:
                    using (var zipStream = new ZipOutputStream(outputStream) {IsStreamOwner = true})
                    {
                        zipStream.SetLevel(archiveOptions.CompressionLevel);
                        zipStream.Password = archiveOptions.Password;

                        var folderOffset = folderName.Value.Length;

                        var fileList = new List<FileInfo>();
                        foreach (var entry in archiveOptions.Entries)
                        {
                            if (entry.IsDirectory)
                                CollectFiles(fileList, new DirectoryInfo(entry.Path));
                            else
                            {
                                fileList.Add(new FileInfo(entry.Path));
                            }
                        }

                        double totalLength = fileList.Sum(x => x.Length);
                        long currentLength = 0;
                        var updateStopwatch = Stopwatch.StartNew();

                        void UpdateProgress(float progress)
                        {
                            //important for a lot of small files
                            if (updateStopwatch.ElapsedMilliseconds > 1000)
                            {
                                updateStopwatch.Reset();
                                processingEntry.Progress = progress;
                                processingEntry.Size = zipStream.Length;
                                ThreadPool.QueueUserWorkItem(state => reportCompressionStatus.Invoke(processingEntry));
                                updateStopwatch.Start();
                            }
                        }

                        foreach (var fileInfo in fileList)
                        {
                            var entryName = ZipEntry.CleanName(fileInfo.FullName.Substring(folderOffset));
                            var zipEntry = new ZipEntry(entryName)
                            {
                                DateTime = fileInfo.LastWriteTime,
                                AESKeySize = string.IsNullOrEmpty(archiveOptions.Password) ? 0 : 256,
                                Size = fileInfo.Length
                            };

                            byte[] buffer = new byte[4096];
                            FileStream zipEntryStream;
                            try
                            {
                                zipEntryStream = fileInfo.OpenRead();
                            }
                            catch (Exception)
                            {
                                continue; //access denied
                            }

                            zipStream.PutNextEntry(zipEntry);

                            using (zipEntryStream)
                            {
                                StreamUtils.Copy(zipEntryStream, zipStream, buffer, (sender, args) =>
                                {
                                    UpdateProgress((float) ((currentLength + args.Processed) / totalLength));
                                    args.ContinueRunning = !cancellationToken.IsCanceled;
                                }, TimeSpan.FromSeconds(1), null, null);
                            }

                            if (cancellationToken.IsCanceled)
                            {
                                //force update
                                processingEntry.Progress = -1;
                                ThreadPool.QueueUserWorkItem(state => reportCompressionStatus.Invoke(processingEntry));
                                return;
                            }

                            currentLength += fileInfo.Length;
                            zipStream.CloseEntry();

                            UpdateProgress((float) (currentLength / totalLength));
                        }

                        //force update
                        processingEntry.Size = zipStream.Length;
                        processingEntry.Progress = 1;
                        ThreadPool.QueueUserWorkItem(state => reportCompressionStatus.Invoke(processingEntry));
                    }
                    return;
                case CompressionMethod.Gzip:
                    var gzipStream = new GZipOutputStream(outputStream) {IsStreamOwner = true};
                    gzipStream.SetLevel(archiveOptions.CompressionLevel);
                    gzipStream.Password = archiveOptions.Password;
                    outputStream = gzipStream;
                    break;
                case CompressionMethod.Bzip2:
                    outputStream = new BZip2OutputStream(outputStream) {IsStreamOwner = true};
                    break;
                default:
                    throw new ArgumentException("Unknown compression method: " + archiveOptions.CompressionMethod);
            }

            using (outputStream)
            {
                if (archiveOptions.UseTarPacker)
                {
                    using (var tarOutputStream = new TarOutputStream(outputStream))
                    {
                        var rootPath =
                            Path.GetDirectoryName(archiveOptions.Entries[0].Path).Replace('\\', '/').TrimEnd('/');

                        var fileList = new List<FileInfo>();
                        foreach (var entry in archiveOptions.Entries)
                        {
                            if (entry.IsDirectory)
                                CollectFiles(fileList, new DirectoryInfo(entry.Path));
                            else
                            {
                                fileList.Add(new FileInfo(entry.Path));
                            }
                        }

                        var buffer = new byte[4096];
                        double totalLength = fileList.Sum(x => x.Length);
                        long currentLength = 0;
                        var updateStopwatch = Stopwatch.StartNew();

                        foreach (var fileInfo in fileList)
                        {
                            Stream fileStream;
                            try
                            {
                                fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read);
                            }
                            catch (Exception)
                            {
                                continue;
                            }

                            using (fileStream)
                            {
                                var tarEntry = TarEntry.CreateEntryFromFile(fileInfo.FullName);
                                tarEntry.Name = fileInfo.FullName.Substring(rootPath.Length + 1);
                                tarOutputStream.PutNextEntry(tarEntry);

                                StreamUtils.Copy(fileStream, tarOutputStream, buffer, (sender, args) =>
                                    {
                                        args.ContinueRunning = !cancellationToken.IsCanceled;
                                        if (updateStopwatch.ElapsedMilliseconds > 1000)
                                        {
                                            updateStopwatch.Reset();
                                            processingEntry.Progress =
                                                (float) ((currentLength + args.Processed) / totalLength);
                                            processingEntry.Size = tarOutputStream.Length;
                                            ThreadPool.QueueUserWorkItem(
                                                state => reportCompressionStatus.Invoke(processingEntry));
                                            updateStopwatch.Start();
                                        }
                                    },
                                    TimeSpan.FromSeconds(1), null, null);
                                tarOutputStream.CloseEntry();
                            }

                            currentLength += fileInfo.Length;

                            if (cancellationToken.IsCanceled)
                            {
                                processingEntry.Progress = -1;
                                ThreadPool.QueueUserWorkItem(state => reportCompressionStatus.Invoke(processingEntry));
                                return;
                            }
                        }
                    }
                }
                else
                {
                    var entry = archiveOptions.Entries[0];
                    if (entry.IsDirectory)
                        throw new ArgumentException("Cannot pack directory without tar/zip");

                    byte[] dataBuffer = new byte[4096];
                    using (var sourceStream = new FileStream(entry.Path, FileMode.Open, FileAccess.Read))
                        StreamUtils.Copy(sourceStream, outputStream, dataBuffer, (sender, args) =>
                        {
                            //no stopwatch needed because it is only one entry
                            processingEntry.Progress = args.PercentComplete / 100;
                            processingEntry.Size = outputStream.Length;
                            args.ContinueRunning = !cancellationToken.IsCanceled;
                            ThreadPool.QueueUserWorkItem(state => reportCompressionStatus.Invoke(processingEntry));
                        }, TimeSpan.FromSeconds(1), null, null);

                    if (cancellationToken.IsCanceled)
                    {
                        //force update
                        processingEntry.Progress = -1;
                        ThreadPool.QueueUserWorkItem(state => reportCompressionStatus.Invoke(processingEntry));
                        return;
                    }
                }

                processingEntry.Size = outputStream.Length;
                processingEntry.Progress = 1;
                ThreadPool.QueueUserWorkItem(state => reportCompressionStatus.Invoke(processingEntry));
            }
        }
    }
}