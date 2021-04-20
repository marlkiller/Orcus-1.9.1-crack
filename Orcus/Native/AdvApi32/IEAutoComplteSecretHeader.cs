using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace Orcus.Native
{
    //Main Decrypted Autocomplete Header data
    [StructLayout(LayoutKind.Sequential)]
    internal struct IEAutoComplteSecretHeader
    {
        public uint dwSize; //This header size
        public uint dwSecretInfoSize; //= sizeof(IESecretInfoHeader) + numSecrets * sizeof(SecretEntry);
        public uint dwSecretSize; //Size of the actual secret strings such as username & password
        public IESecretInfoHeader IESecretHeader; //info about secrets such as count, size etc
        //SecretEntry secEntries[numSecrets];      //Header for each Secret String
        //WCHAR secrets[numSecrets];               //Actual Secret String in Unicode
    }
}