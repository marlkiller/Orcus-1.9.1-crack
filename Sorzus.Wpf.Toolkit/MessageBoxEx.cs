using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Sorzus.Wpf.Toolkit.Native;

namespace Sorzus.Wpf.Toolkit
{
    //Taken from https://stackoverflow.com/questions/564710/how-to-get-messagebox-show-to-pop-up-in-the-middle-of-my-wpf-application
    /// <summary>
    ///     Displays a message window, also known as a dialog box, which presents a message to the user. It is a modal window,
    ///     blocking other actions in the application until the user closes it. A MessageBox can contain text, buttons, and
    ///     symbols that inform and instruct the user. A wpf <see cref="Window" /> can be set as the owner
    /// </summary>
    public class MessageBoxEx
    {
        private const int WH_CALLWNDPROCRET = 12;
        private static IntPtr _owner;
        private static readonly NativeMethods.HookProc _hookProc;
        private static IntPtr _hHook;

        static MessageBoxEx()
        {
            _hookProc = MessageBoxHookProc;
            _hHook = IntPtr.Zero;
        }

        /// <summary>
        ///     Displays a message box.
        /// </summary>
        /// <param name="text">The text to display in the message box.</param>
        /// <returns>One of the <see cref="MessageBoxResult" /> values</returns>
        public static MessageBoxResult Show(string text)
        {
            Initialize();
            return MessageBox.Show(text);
        }

        /// <summary>
        ///     Displays a message box.
        /// </summary>
        /// <param name="text">The text to display in the message box.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        /// <returns>One of the <see cref="MessageBoxResult" /> values</returns>
        public static MessageBoxResult Show(string text, string caption)
        {
            Initialize();
            return MessageBox.Show(text, caption);
        }

        /// <summary>
        ///     Displays a message box.
        /// </summary>
        /// <param name="text">The text to display in the message box.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        /// <param name="buttons">
        ///     One of the <see cref="MessageBoxButton" /> values that specifies which buttons to display in the
        ///     message box.
        /// </param>
        /// <returns>One of the <see cref="MessageBoxResult" /> values</returns>
        public static MessageBoxResult Show(string text, string caption, MessageBoxButton buttons)
        {
            Initialize();
            return MessageBox.Show(text, caption, buttons);
        }

        /// <summary>
        ///     Displays a message box.
        /// </summary>
        /// <param name="text">The text to display in the message box.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        /// <param name="buttons">
        ///     One of the <see cref="MessageBoxButton" /> values that specifies which buttons to display in the
        ///     message box.
        /// </param>
        /// <param name="icon">
        ///     One of the <see cref="MessageBoxImage" /> values that specifies which icon to display in the message
        ///     box.
        /// </param>
        /// <returns>One of the <see cref="MessageBoxResult" /> values</returns>
        public static MessageBoxResult Show(string text, string caption, MessageBoxButton buttons, MessageBoxImage icon)
        {
            Initialize();
            return MessageBox.Show(text, caption, buttons, icon);
        }

        /// <summary>
        ///     Displays a message box.
        /// </summary>
        /// <param name="text">The text to display in the message box.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        /// <param name="buttons">
        ///     One of the <see cref="MessageBoxButton" /> values that specifies which buttons to display in the
        ///     message box.
        /// </param>
        /// <param name="icon">
        ///     One of the <see cref="MessageBoxImage" /> values that specifies which icon to display in the message
        ///     box.
        /// </param>
        /// <param name="defResult">
        ///     One of the <see cref="MessageBoxResult" /> values that specifies the default button for the
        ///     message box.
        /// </param>
        /// <returns>One of the <see cref="MessageBoxResult" /> values</returns>
        public static MessageBoxResult Show(string text, string caption, MessageBoxButton buttons, MessageBoxImage icon,
            MessageBoxResult defResult)
        {
            Initialize();
            return MessageBox.Show(text, caption, buttons, icon, defResult);
        }

        /// <summary>
        ///     Displays a message box.
        /// </summary>
        /// <param name="text">The text to display in the message box.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        /// <param name="buttons">
        ///     One of the <see cref="MessageBoxButton" /> values that specifies which buttons to display in the
        ///     message box.
        /// </param>
        /// <param name="icon">
        ///     One of the <see cref="MessageBoxImage" /> values that specifies which icon to display in the message
        ///     box.
        /// </param>
        /// <param name="defResult">
        ///     One of the <see cref="MessageBoxResult" /> values that specifies the default button for the
        ///     message box.
        /// </param>
        /// <param name="options">
        ///     One of the <see cref="MessageBoxOptions" /> values that specifies which display and association
        ///     options will be used for the message box. You may pass in 0 if you wish to use the defaults.
        /// </param>
        /// <returns>One of the <see cref="MessageBoxResult" /> values</returns>
        public static MessageBoxResult Show(string text, string caption, MessageBoxButton buttons, MessageBoxImage icon,
            MessageBoxResult defResult, MessageBoxOptions options)
        {
            Initialize();
            return MessageBox.Show(text, caption, buttons, icon, defResult, options);
        }

        /// <summary>
        ///     Displays a message box.
        /// </summary>
        /// <param name="owner">The window which will own the message box</param>
        /// <param name="text">The text to display in the message box.</param>
        /// <returns>One of the <see cref="MessageBoxResult" /> values</returns>
        public static MessageBoxResult Show(Window owner, string text)
        {
            _owner = new WindowInteropHelper(owner).Handle;
            Initialize();
            return MessageBox.Show(owner, text);
        }

        /// <summary>
        ///     Displays a message box.
        /// </summary>
        /// <param name="owner">The window which will own the message box</param>
        /// <param name="text">The text to display in the message box.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        /// <returns>One of the <see cref="MessageBoxResult" /> values</returns>
        public static MessageBoxResult Show(Window owner, string text, string caption)
        {
            _owner = new WindowInteropHelper(owner).Handle;
            Initialize();
            return MessageBox.Show(owner, text, caption);
        }

        /// <summary>
        ///     Displays a message box.
        /// </summary>
        /// <param name="owner">The window which will own the message box</param>
        /// <param name="text">The text to display in the message box.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        /// <param name="buttons">
        ///     One of the <see cref="MessageBoxButton" /> values that specifies which buttons to display in the
        ///     message box.
        /// </param>
        /// <returns>One of the <see cref="MessageBoxResult" /> values</returns>
        public static MessageBoxResult Show(Window owner, string text, string caption, MessageBoxButton buttons)
        {
            _owner = new WindowInteropHelper(owner).Handle;
            Initialize();
            return MessageBox.Show(owner, text, caption, buttons);
        }

        /// <summary>
        ///     Displays a message box.
        /// </summary>
        /// <param name="owner">The window which will own the message box</param>
        /// <param name="text">The text to display in the message box.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        /// <param name="buttons">
        ///     One of the <see cref="MessageBoxButton" /> values that specifies which buttons to display in the
        ///     message box.
        /// </param>
        /// <param name="icon">
        ///     One of the <see cref="MessageBoxImage" /> values that specifies which icon to display in the message
        ///     box.
        /// </param>
        /// <returns>One of the <see cref="MessageBoxResult" /> values</returns>
        public static MessageBoxResult Show(Window owner, string text, string caption, MessageBoxButton buttons,
            MessageBoxImage icon)
        {
            _owner = new WindowInteropHelper(owner).Handle;
            Initialize();
            return MessageBox.Show(owner, text, caption, buttons, icon);
        }

        /// <summary>
        ///     Displays a message box.
        /// </summary>
        /// <param name="owner">The window which will own the message box</param>
        /// <param name="text">The text to display in the message box.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        /// <param name="buttons">
        ///     One of the <see cref="MessageBoxButton" /> values that specifies which buttons to display in the
        ///     message box.
        /// </param>
        /// <param name="icon">
        ///     One of the <see cref="MessageBoxImage" /> values that specifies which icon to display in the message
        ///     box.
        /// </param>
        /// <param name="defResult">
        ///     One of the <see cref="MessageBoxResult" /> values that specifies the default button for the
        ///     message box.
        /// </param>
        /// <returns>One of the <see cref="MessageBoxResult" /> values</returns>
        public static MessageBoxResult Show(Window owner, string text, string caption, MessageBoxButton buttons,
            MessageBoxImage icon, MessageBoxResult defResult)
        {
            _owner = new WindowInteropHelper(owner).Handle;
            Initialize();
            return MessageBox.Show(owner, text, caption, buttons, icon, defResult);
        }

        /// <summary>
        ///     Displays a message box.
        /// </summary>
        /// <param name="owner">The window which will own the message box</param>
        /// <param name="text">The text to display in the message box.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        /// <param name="buttons">
        ///     One of the <see cref="MessageBoxButton" /> values that specifies which buttons to display in the
        ///     message box.
        /// </param>
        /// <param name="icon">
        ///     One of the <see cref="MessageBoxImage" /> values that specifies which icon to display in the message
        ///     box.
        /// </param>
        /// <param name="defResult">
        ///     One of the <see cref="MessageBoxResult" /> values that specifies the default button for the
        ///     message box.
        /// </param>
        /// <param name="options">
        ///     One of the <see cref="MessageBoxOptions" /> values that specifies which display and association
        ///     options will be used for the message box. You may pass in 0 if you wish to use the defaults.
        /// </param>
        /// <returns>One of the <see cref="MessageBoxResult" /> values</returns>
        public static MessageBoxResult Show(Window owner, string text, string caption, MessageBoxButton buttons,
            MessageBoxImage icon, MessageBoxResult defResult, MessageBoxOptions options)
        {
            _owner = new WindowInteropHelper(owner).Handle;
            Initialize();
            return MessageBox.Show(owner, text, caption, buttons, icon,
                defResult, options);
        }

        private static void Initialize()
        {
            if (_hHook != IntPtr.Zero)
            {
                throw new NotSupportedException("multiple calls are not supported");
            }

            _hHook = NativeMethods.SetWindowsHookEx(WH_CALLWNDPROCRET, _hookProc, IntPtr.Zero,
                (int) NativeMethods.GetCurrentThreadId());
        }

        private static IntPtr MessageBoxHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0)
            {
                return NativeMethods.CallNextHookEx(_hHook, nCode, wParam, lParam);
            }

            var msg = (CWPRETSTRUCT) Marshal.PtrToStructure(lParam, typeof (CWPRETSTRUCT));
            IntPtr hook = _hHook;

            if (msg.message == (int) CbtHookAction.HCBT_ACTIVATE)
            {
                try
                {
                    CenterWindow(msg.hwnd);
                }
                finally
                {
                    NativeMethods.UnhookWindowsHookEx(_hHook);
                    _hHook = IntPtr.Zero;
                }
            }

            return NativeMethods.CallNextHookEx(hook, nCode, wParam, lParam);
        }

        private static void CenterWindow(IntPtr hChildWnd)
        {
            Rectangle recChild = new Rectangle(0, 0, 0, 0);
            NativeMethods.GetWindowRect(hChildWnd, ref recChild);

            int width = recChild.Width - recChild.X;
            int height = recChild.Height - recChild.Y;

            Rectangle recParent = new Rectangle(0, 0, 0, 0);
            NativeMethods.GetWindowRect(_owner, ref recParent);

            System.Drawing.Point ptCenter = new System.Drawing.Point(0, 0)
            {
                X = recParent.X + (recParent.Width - recParent.X)/2,
                Y = recParent.Y + (recParent.Height - recParent.Y)/2
            };


            System.Drawing.Point ptStart = new System.Drawing.Point(0, 0)
            {
                X = ptCenter.X - width/2,
                Y = ptCenter.Y - height/2
            };

            ptStart.X = ptStart.X < 0 ? 0 : ptStart.X;
            ptStart.Y = ptStart.Y < 0 ? 0 : ptStart.Y;

            NativeMethods.MoveWindow(hChildWnd, ptStart.X, ptStart.Y, width,
                height, false);
        }
    }
}