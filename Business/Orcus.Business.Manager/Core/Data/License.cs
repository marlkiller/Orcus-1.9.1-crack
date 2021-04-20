using System;

namespace Orcus.Business.Manager.Core.Data
{
    public class License
    {
        public string licenseKey { get; set; }
        public string id { get; set; }
        public string banned { get; set; }
        public string comment { get; set; }
        public string creationDate { get; set; }

        public DateTime DateTime => DateTime.Parse(creationDate);
        public string DateTimeString => DateTime.ToString("G");
        public bool IsBanned => banned == "1";
        public int RegisteredComputers { get; set; }
        public bool NoMoney => string.IsNullOrEmpty(comment) || comment.IndexOf("#NM", StringComparison.OrdinalIgnoreCase) > -1;
    }
}