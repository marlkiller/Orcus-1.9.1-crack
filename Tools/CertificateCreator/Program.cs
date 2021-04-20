using System;
using System.IO;

namespace CertificateCreator
{
    internal class Program
    {
        private static void Main()
        {
            Console.WriteLine("Please enter your hostname:");
            Console.Write(">");
            string hostname;

            while (true)
            {
                hostname = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(hostname))
                    break;
            }

            Console.WriteLine("Please enter a password:");
            Console.Write(">");
            string password;

            while (true)
            {
                password = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(password))
                    break;
            }

            var startDate = new DateTime(2016, 1, 1);
            var endDate = new DateTime(2050, 1, 1);

            var c = Certificate.CreateSelfSignCertificatePfx("CN=" + hostname, startDate, endDate, password);
            using (var fs = new FileStream("certificate.pfx", FileMode.CreateNew, FileAccess.ReadWrite))
            {
                fs.Write(c, 0, c.Length);
            }

            Console.WriteLine("Certificate created successful.");
            Console.WriteLine("Hostname: {0}", hostname);
            Console.WriteLine("Password: {0}", password);
            Console.WriteLine("Start date: {0}", startDate);
            Console.WriteLine("End date: {0}", endDate);
            Console.ReadLine();
        }
    }
}