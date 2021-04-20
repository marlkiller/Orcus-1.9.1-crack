using Microsoft.Win32;

namespace Orcus.Commands.WindowsCustomizer
{
    public static class RegistryUtilities
    {
        public static int? GetIntValueSafe(RegistryKey registryKey, string valueName)
        {
            using (registryKey)
            {
                var value = registryKey?.GetValue(valueName, null);
                if (value == null)
                    return null;

                int intValue;
                if (int.TryParse(value.ToString(), out intValue))
                    return intValue;

                return null;
            }
        }

        public static string GetStringValueSafe(RegistryKey registryKey, string valueName)
        {
            using (registryKey)
                return registryKey?.GetValue(valueName, null)?.ToString();
        }

        public static void SetValueSafe(RegistryKey hive, string path, string valueName, object value,
            RegistryValueKind registryValueKind)
        {
            //RegistryKey.CreateSubKey - Creates a new subkey or opens an existing subkey.
            using (var regKey = hive.CreateSubKey(path, RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                // ReSharper disable once PossibleNullReferenceException
                regKey.SetValue(valueName, value, registryValueKind);
            }
        }
    }
}