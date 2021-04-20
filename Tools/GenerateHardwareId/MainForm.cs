using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace GenerateHardwareId
{
    public partial class MainForm : Form
    {
        private const string HowToCalculateString = "MD5 (\"{0}\" + \"{1}\")";

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string parameter1 = null;
            string parameter2 = null;

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

            HowToCalculateLabel.Text = string.Format(HowToCalculateString, parameter1, parameter2);

            using (var md5 = new MD5CryptoServiceProvider())
            {
                HardwareIdTextBox.Text = BitConverter.ToString(
                    md5.ComputeHash(Encoding.UTF8.GetBytes(parameter1 + parameter2)))
                    .Replace("-", null);
            }
        }
    }
}