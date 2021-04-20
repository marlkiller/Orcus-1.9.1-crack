using System.Runtime.InteropServices;

namespace Orcus.Native
{
    // Header describing each of the secrets such ass username/password.
    // Two secret entries having same SecretId are paired
    [StructLayout(LayoutKind.Explicit)]
    internal struct SecretEntry
    {
        [FieldOffset(0)] public uint dwOffset; //Offset of this secret entry from the start of secret entry strings

        [FieldOffset(4)] public byte SecretId; //UNIQUE id associated with the secret
        [FieldOffset(5)] public byte SecretId1;
        [FieldOffset(6)] public byte SecretId2;
        [FieldOffset(7)] public byte SecretId3;
        [FieldOffset(8)] public byte SecretId4;
        [FieldOffset(9)] public byte SecretId5;
        [FieldOffset(10)] public byte SecretId6;
        [FieldOffset(11)] public byte SecretId7;

        [FieldOffset(12)] public uint dwLength; //length of this secret
    }
}