using System;

namespace Orcus.Shared.Commands.ComputerInformation
{
    [Serializable]
    public class BiosInformation
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string ProductId { get; set; }
        public string Manufacturer { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Language { get; set; }
        public string SupportedLanguages { get; set; }
    }
}