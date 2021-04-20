using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace Orcus.Administration.Licensing
{
    public class HardwareIdGenerator
    {
        private static string _hardwareId;
        public static string HardwareId => _hardwareId ?? (_hardwareId = GenerateHardwareId());

        private static string GenerateHardwareId()
        {
            string parameter1 = null;
            string parameter2 = null;
            string parameter3 = null;

            try
            {
                using (var searcher = new ManagementObjectSearcher("Select * FROM WIN32_Processor"))
                {
                    var processorManagementObject = searcher.Get().OfType<ManagementObject>().FirstOrDefault();
                    if (processorManagementObject != null)
                    {
                        parameter1 = (string) processorManagementObject["ProcessorId"];
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                using (var searcher = new ManagementObjectSearcher("Select * FROM Win32_BaseBoard"))
                {
                    var processorManagementObject = searcher.Get().OfType<ManagementObject>().FirstOrDefault();
                    if (processorManagementObject != null)
                    {
                        parameter3 = (string) processorManagementObject["SerialNumber"];
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                var drive = DriveInfo.GetDrives().First().Name.Replace("\\", null);
                using (var dsk = new ManagementObject(@"win32_logicaldisk.deviceid=""" + drive + "\""))
                {
                    dsk.Get();

                    parameter2 = (string) dsk["VolumeSerialNumber"];
                }
            }
            catch (Exception)
            {
                // ignored
            }

            using (var md5 = new SHA256CryptoServiceProvider())
            {
                return BitConverter.ToString(
                    md5.ComputeHash(Encoding.UTF8.GetBytes(parameter1 + parameter2 + parameter3)))
                    .Replace("-", null);
            }
        }
    }
}