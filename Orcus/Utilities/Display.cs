using System;
using System.Runtime.InteropServices;
using Orcus.Native;
using Orcus.Native.Display;

namespace Orcus.Utilities
{
    public class Display
    {
        private const int DMDO_DEFAULT = 0;
        private const int DMDO_90 = 1;
        private const int DMDO_180 = 2;
        private const int DMDO_270 = 3;

        private const int ENUM_CURRENT_SETTINGS = -1;

        public enum Orientations
        {
            DEGREES_CW_0 = 0,
            DEGREES_CW_90 = 3,
            DEGREES_CW_180 = 2,
            DEGREES_CW_270 = 1
        }

        public static DISP_CHANGE Rotate(uint displayNumber, Orientations orientation)
        {
            if (displayNumber == 0)
                throw new ArgumentOutOfRangeException("displayNumber", displayNumber, "First display is 1.");

            DISP_CHANGE result = DISP_CHANGE.Failed;
            DISPLAY_DEVICE d = new DISPLAY_DEVICE();
            DEVMODE dm = new DEVMODE();
            d.cb = Marshal.SizeOf(d);

            if (!NativeMethods.EnumDisplayDevices(null, displayNumber - 1, ref d, 0))
                throw new ArgumentOutOfRangeException("displayNumber", displayNumber,
                    "Number is greater than connected displays.");

            if (0 != NativeMethods.EnumDisplaySettings(
                    d.DeviceName, ENUM_CURRENT_SETTINGS, ref dm))
            {
                if ((dm.dmDisplayOrientation + (int) orientation) % 2 == 1) // Need to swap height and width?
                {
                    int temp = dm.dmPelsHeight;
                    dm.dmPelsHeight = dm.dmPelsWidth;
                    dm.dmPelsWidth = temp;
                }

                switch (orientation)
                {
                    case Orientations.DEGREES_CW_90:
                        dm.dmDisplayOrientation = DMDO_270;
                        break;
                    case Orientations.DEGREES_CW_180:
                        dm.dmDisplayOrientation = DMDO_180;
                        break;
                    case Orientations.DEGREES_CW_270:
                        dm.dmDisplayOrientation = DMDO_90;
                        break;
                    case Orientations.DEGREES_CW_0:
                        dm.dmDisplayOrientation = DMDO_DEFAULT;
                        break;
                    default:
                        break;
                }

                result = NativeMethods.ChangeDisplaySettingsEx(
                    d.DeviceName, ref dm, IntPtr.Zero,
                    DisplaySettingsFlags.CDS_UPDATEREGISTRY, IntPtr.Zero);
            }

            return result;
        }

        public static DISP_CHANGE RotateAllScreens(Orientations orientation)
        {
            //if one fails, we return the result of that failed rotation
            DISP_CHANGE result = DISP_CHANGE.Successful;

            try
            {
                uint i = 0;
                while (++i <= 64)
                {
                    var tempResult = Rotate(i, orientation);
                    if (tempResult != DISP_CHANGE.Successful)
                        result = tempResult;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                // Everything is fine, just reached the last display
            }

            return result;
        }

        public static void ResetAllRotations()
        {
            try
            {
                uint i = 0;
                while (++i <= 64)
                {
                    Rotate(i, Orientations.DEGREES_CW_0);
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                // Everything is fine, just reached the last display
            }
        }
    }
}