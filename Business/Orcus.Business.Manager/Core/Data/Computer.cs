using System;

namespace Orcus.Business.Manager.Core.Data
{
    public class Computer
    {
        public string licenseId { get; set; }
        public string hardwareId { get; set; }
        public string id { get; set; }
        public string timestamp { get; set; }

        public DateTime DateTime => DateTime.Parse(timestamp);
        public string DateTimeString => DateTime.ToString("G");
    }
}