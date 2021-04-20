using System;
using Orcus.Shared.Commands.Registry;

namespace Orcus.Administration.Commands.Registry
{
    public static class RegistryExtensions
    {
        public static string ToReadableString(this RegistryHive hive)
        {
            switch (hive)
            {
                case RegistryHive.ClassesRoot:
                    return "HKEY_CLASSES_ROOT";
                case RegistryHive.CurrentUser:
                    return "HKEY_CURRENT_USER";
                case RegistryHive.LocalMachine:
                    return "HKEY_LOCAL_MACHINE";
                case RegistryHive.Users:
                    return "HKEY_USERS";
                case RegistryHive.CurrentConfig:
                    return "HKEY_CURRENT_CONFIG";
                default:
                    throw new ArgumentOutOfRangeException(nameof(hive), hive, null);
            }
        }
    }
}