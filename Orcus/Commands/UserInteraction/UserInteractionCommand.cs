using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Orcus.Plugins;
using Orcus.Shared.Commands.UserInteraction;
using Orcus.Shared.NetSerializer;
using Orcus.Utilities;

namespace Orcus.Commands.UserInteraction
{
    internal class UserInteractionCommand : Command
    {
        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            switch ((UserInteractionCommunication) parameter[0])
            {
                case UserInteractionCommunication.TextToSpeech:
                    var textToSpeechInfo =
                        new Serializer(typeof (TextToSpeechPackage)).Deserialize<TextToSpeechPackage>(parameter, 1);
                    using (var speaker = new SpeechSynthesizer())
                    {
                        speaker.Rate = textToSpeechInfo.Speed;
                        speaker.Volume = textToSpeechInfo.Volume;
                        speaker.SelectVoice(textToSpeechInfo.VoiceName);
                        speaker.SetOutputToDefaultAudioDevice();
                        connectionInfo.CommandResponse(this,
                            new[] {(byte) UserInteractionCommunication.SpeakingText});
                        speaker.Speak(textToSpeechInfo.Text);
                        connectionInfo.CommandResponse(this,
                            new[] {(byte) UserInteractionCommunication.SpeakingFinished});
                    }
                    break;
                case UserInteractionCommunication.GetWelcomePackage:
                    var package = new UserInteractionWelcomePackage();
                    using (var speaker = new SpeechSynthesizer())
                        package.Voices =
                            speaker.GetInstalledVoices()
                                .Select(
                                    x =>
                                        new SpeechVoice
                                        {
                                            Culture = x.VoiceInfo.Culture.TwoLetterISOLanguageName,
                                            Name = x.VoiceInfo.Name,
                                            VoiceAge = (SpeechAge) (int) x.VoiceInfo.Age,
                                            VoiceGender = (SpeechGender) (int) x.VoiceInfo.Gender
                                        })
                                .ToList();

                    var data = new List<byte> {(byte) UserInteractionCommunication.WelcomePackage};
                    data.AddRange(new Serializer(typeof (UserInteractionWelcomePackage)).Serialize(package));
                    connectionInfo.CommandResponse(this, data.ToArray());
                    break;
                case UserInteractionCommunication.OpenInEditor:
                    var textLength = BitConverter.ToInt32(parameter, 1);
                    NotepadHelper.ShowMessage(Encoding.UTF8.GetString(parameter, 5, textLength),
                        Encoding.UTF8.GetString(parameter, textLength + 5, parameter.Length - (5 + textLength)));
                    connectionInfo.CommandResponse(this,
                        new[] {(byte) UserInteractionCommunication.OpenedInEditorSuccessfully});
                    break;
                case UserInteractionCommunication.NotifyIconMessage:
                    var timeout = BitConverter.ToInt32(parameter, 2);
                    var titleLength = BitConverter.ToInt32(parameter, 6);
                    var title = Encoding.UTF8.GetString(parameter, 10, titleLength);
                    var text = Encoding.UTF8.GetString(parameter, 10 + titleLength,
                        parameter.Length - (10 + titleLength));

                    using (var notifyIcon = new NotifyIcon {Icon = SystemIcons.Application})
                    {
                        notifyIcon.Visible = true;
                        notifyIcon.ShowBalloonTip(timeout, title, text, (ToolTipIcon) parameter[1]);
                        connectionInfo.CommandResponse(this,
                            new[] {(byte) UserInteractionCommunication.NotifyIconMessageOpened});
                        Thread.Sleep(timeout);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override uint GetId()
        {
            return 22;
        }
    }
}