using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleApplication13
{
    class Program
    {
        static void Main(string[] args)
        {
            var s = @"
Adapter
Class = Adapter
ClassGuid = {4d36e964-e325-11ce-bfc1-08002be10318}
This class is obsolete.
APM
Class = APMSupport
ClassGuid = {d45b1c18-c8fa-11d1-9f77-0000f805f530}
This class is reserved for system use.
Computer
Class = Computer
ClassGuid = {4d36e966-e325-11ce-bfc1-08002be10318}
This class is reserved for system use.
Decoders
Class = Decoder
ClassGuid = {6bdd1fc2-810f-11d0-bec7-08002be2092f}
This class is reserved for future use.
Host-side IEEE 1394 Kernel Debugger Support
Class = 1394Debug
ClassGuid = {66f250d6-7801-4a64-b139-eea80a450b24}
This class is reserved for system use.
IEEE 1394 IP Network Enumerator
Class = Enum1394
ClassGuid = {c459df55-db08-11d1-b009-00a0c9081ff6}
This class is reserved for system use.
No driver
Class = NoDriver
ClassGuid = {4d36e976-e325-11ce-bfc1-08002be10318}
This class is obsolete.
Non-Plug and Play Drivers
Class = LegacyDriver
ClassGuid = {8ecc055d-047f-11d1-a537-0000f8753ed1}
This class is reserved for system use.
Other Devices
Class = Unknown
ClassGuid = {4d36e97e-e325-11ce-bfc1-08002be10318}
This class is reserved for system use. Enumerated devices for which the system cannot determine the type are installed under this class. Do not use this class if you are unsure in which class your device belongs. Either determine the correct device setup class or create a new class.
Printer Upgrade
Class = PrinterUpgrade
ClassGuid = {4d36e97a-e325-11ce-bfc1-08002be10318}
This class is reserved for system use.
Sound
Class = Sound
ClassGuid = {4d36e97c-e325-11ce-bfc1-08002be10318}
This class is obsolete.
Storage Volume Snapshots
Class = VolumeSnapshot
ClassGuid = {533c5b84-ec70-11d2-9505-00c04F79deaf}
This class is reserved for system use.
USB Bus Devices (hubs and host controllers)
Class = USB
ClassGuid = {36fc9e60-c465-11cf-8056-444553540000}
This class includes USB host controllers and USB hubs, but not USB peripherals. Drivers for this class are system-supplied.";

            var classGuids = new List<Guid>();
            foreach (
                Match match in
                    Regex.Matches(s,
                        @"^(?<name>(.*?))\r\nClass = (?<class>(.*?))\r\nClassGuid = \{(?<guid>(.*?))\}\r\n(?<description>(.*?))$",
                        RegexOptions.Multiline))
            {
                Console.WriteLine("/// <summary>");
                Console.WriteLine("/// Name = " + match.Groups["name"].Value + ", Class = " +
                                  match.Groups["class"].Value);
                Console.WriteLine("/// " + match.Groups["description"].Value);
                Console.WriteLine("/// </summary>");
                Console.WriteLine($"[DeviceCategoryDisplayName(\"{match.Groups["name"].Value}\")]");
                Console.WriteLine($"[DeviceCategoryGuid(\"{{{match.Groups["guid"].Value}}}\")]");
                Console.WriteLine(match.Groups["name"].Value.Replace(" ", null) + ",");
                classGuids.Add(new Guid(match.Groups["guid"].Value));
            }

            var deviceGuids = new List<Guid>();
            using (
                var searcher = new ManagementObjectSearcher(@"\\" + Environment.MachineName + @"\root\CIMV2",
                    "Select * from Win32_PnPEntity"))
            using (var collection = searcher.Get())
            {
                foreach (var managementObject in collection.Cast<ManagementObject>())
                {
                    var guidStr = (string) managementObject["ClassGuid"];
                    if (guidStr == null)
                        continue;

                    var guod = new Guid(guidStr);
                    if (!classGuids.Contains(guod) && !deviceGuids.Contains(guod))
                    {
                        deviceGuids.Add(guod);
                       // Console.WriteLine("Not found: " + guod + " (" + (string)managementObject["Caption"] + ")");
                    }
                }
            }

            Console.ReadKey();
        }
    }
}