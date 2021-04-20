using System.Collections.Generic;

namespace Orcus.Server.CommandLine
{
    internal class CommandLineSettings
    {
        public bool Verbose { get; set; }
        public string Settings { get; set; }
        public List<string> IpAddresses { get; set; }
        public string DatabasePath { get; set; }
        public bool NoSettings { get; set; }
        public string SslCertificatePath { get; set; }
        public string SslCertificatePassword { get; set; }
        public string ServerPassword { get; set; }
    }
}