using System;
using System.Runtime.InteropServices;

namespace Orcus.Administration.Core.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct CryptKeyProviderInformation
    {
        [MarshalAs(UnmanagedType.LPWStr)] public string ContainerName;
        [MarshalAs(UnmanagedType.LPWStr)] public string ProviderName;
        public int ProviderType;
        public int Flags;
        public int ProviderParameterCount;
        public IntPtr ProviderParameters; // PCRYPT_KEY_PROV_PARAM
        public int KeySpec;
    }
}