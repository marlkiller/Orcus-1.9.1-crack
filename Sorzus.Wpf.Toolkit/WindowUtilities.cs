using System;
using System.Windows;
using System.Windows.Interop;
using Sorzus.Wpf.Toolkit.Native;

namespace Sorzus.Wpf.Toolkit
{
    /// <summary>
    ///     Some useful utilities for <see cref="Window" />s
    /// </summary>
    public static class WindowUtilities
    {
        /// <summary>
        ///     Remove the icon of a <see cref="Window" />
        /// </summary>
        /// <param name="window">The window which should not have an icon</param>
        public static void RemoveIcon(this Window window)
        {
            // Get this window's handle
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            // Change the extended window style to not show a window icon
            int extendedStyle = NativeMethods.GetWindowLong(hwnd, GWL_EXSTYLE);
            NativeMethods.SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_DLGMODALFRAME);
            // Update the window's non-client area to reflect the changes
            NativeMethods.SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
        }

        /// <summary>
        ///     Center one window on another one
        /// </summary>
        /// <param name="window">The window which should be centered</param>
        /// <param name="parentWindow">The window which should be the owner</param>
        public static void CenterOnWindow(this Window window, Window parentWindow)
        {
            if (!window.IsLoaded)
            {
                RoutedEventHandler handler = null;
                handler = (sender, args) =>
                {
                    window.Loaded -= handler;
                    window.CenterOnWindow(parentWindow);
                };
                window.Loaded += handler;
                return;
            }
            window.Left = parentWindow.Left + (parentWindow.ActualWidth - window.Width)/2;
            window.Top = parentWindow.Top + (parentWindow.ActualHeight - window.Height)/2;
        }

        // ReSharper disable InconsistentNaming
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_DLGMODALFRAME = 0x0001;
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOZORDER = 0x0004;
        private const int SWP_FRAMECHANGED = 0x0020;
        // ReSharper restore InconsistentNaming
    }
}