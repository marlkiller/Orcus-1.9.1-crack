using System;
using Microsoft.Win32;

namespace Orcus.Commands.Passwords.Utilities
{
    public static class RegistryKeyHelper
    {
        /// <summary>
        ///     Adds a value to the registry key.
        /// </summary>
        /// <param name="hive">Represents the possible values for a top-level node on a foreign machine.</param>
        /// <param name="path">The path to the registry key.</param>
        /// <param name="name">The name of the value.</param>
        /// <param name="value">The value.</param>
        /// <param name="addQuotes">If set to True, adds quotes to the value.</param>
        /// <returns>True on success, else False.</returns>
        public static bool AddRegistryKeyValue(RegistryHive hive, string path, string name, string value,
            bool addQuotes = false)
        {
            try
            {
                using (
                    RegistryKey key =
                        RegistryExtensions.OpenBaseKey(hive, RegistryExtensions.RegistryView.Registry64)
                            .OpenWritableSubKeySafe(path))
                {
                    if (key == null) return false;

                    if (addQuotes && !value.StartsWith("\"") && !value.EndsWith("\""))
                        value = "\"" + value + "\"";

                    key.SetValue(name, value);
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        ///     Opens a read-only registry key.
        /// </summary>
        /// <param name="hive">Represents the possible values for a top-level node on a foreign machine.</param>
        /// <param name="path">The path to the registry key.</param>
        /// <returns></returns>
        public static RegistryKey OpenReadonlySubKey(RegistryHive hive, string path)
        {
            try
            {
                return RegistryExtensions.OpenBaseKey(hive, RegistryExtensions.RegistryView.Registry64)
                    .OpenSubKey(path, false);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        ///     Deletes the specified value from the registry key.
        /// </summary>
        /// <param name="hive">Represents the possible values for a top-level node on a foreign machine.</param>
        /// <param name="path">The path to the registry key.</param>
        /// <param name="name">The name of the value to delete.</param>
        /// <returns>True on success, else False.</returns>
        public static bool DeleteRegistryKeyValue(RegistryHive hive, string path, string name)
        {
            try
            {
                using (
                    RegistryKey key =
                        RegistryExtensions.OpenBaseKey(hive, RegistryExtensions.RegistryView.Registry64)
                            .OpenWritableSubKeySafe(path))
                {
                    if (key == null) return false;
                    key.DeleteValue(name, true);
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}