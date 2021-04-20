using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Orcus.Native;
using Orcus.Native.Display;

namespace Orcus.Utilities
{
    //Source: https://stackoverflow.com/questions/4958683/how-do-i-get-the-actual-monitor-name-as-seen-in-the-resolution-dialog
    public static class ScreenExtensions
    {
        public const int ERROR_SUCCESS = 0;

        private static string MonitorFriendlyName(LUID adapterId, uint targetId)
        {
            var deviceName = new DISPLAYCONFIG_TARGET_DEVICE_NAME
            {
                header =
                {
                    size = (uint) Marshal.SizeOf(typeof (DISPLAYCONFIG_TARGET_DEVICE_NAME)),
                    adapterId = adapterId,
                    id = targetId,
                    type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME
                }
            };
            var error = NativeMethods.DisplayConfigGetDeviceInfo(ref deviceName);
            if (error != ERROR_SUCCESS)
                throw new Win32Exception(error);
            return deviceName.monitorFriendlyDeviceName;
        }

        public static string[] GetAllMonitorsFriendlyNames()
        {
            if (CoreHelper.RunningOnVista || CoreHelper.RunningOnXP)
                return GetDefaultScreenNames();

            uint pathCount, modeCount;
            var error = NativeMethods.GetDisplayConfigBufferSizes(QUERY_DEVICE_CONFIG_FLAGS.QDC_ONLY_ACTIVE_PATHS, out pathCount,
                out modeCount);
            if (error != ERROR_SUCCESS)
                return GetDefaultScreenNames();

            var displayPaths = new DISPLAYCONFIG_PATH_INFO[pathCount];
            var displayModes = new DISPLAYCONFIG_MODE_INFO[modeCount];
            error = NativeMethods.QueryDisplayConfig(QUERY_DEVICE_CONFIG_FLAGS.QDC_ONLY_ACTIVE_PATHS,
                ref pathCount, displayPaths, ref modeCount, displayModes, IntPtr.Zero);
            if (error != ERROR_SUCCESS)
                return GetDefaultScreenNames();
            //throw new Win32Exception(error);

            var screenNames = new List<string>();
            for (var i = 0; i < modeCount; i++)
                if (displayModes[i].infoType == DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_TARGET)
                    screenNames.Add(MonitorFriendlyName(displayModes[i].adapterId, displayModes[i].id));

            return screenNames.ToArray();
        }

        private static string[] GetDefaultScreenNames()
        {
            return Screen.AllScreens.Select(x => x.DeviceName).ToArray();
        }

        public static string GetDeviceFriendlyName(this Screen screen)
        {
            var allFriendlyNames = GetAllMonitorsFriendlyNames();
            for (var index = 0; index < Screen.AllScreens.Length; index++)
                if (Equals(screen, Screen.AllScreens[index]))
                    return allFriendlyNames.ToArray()[index];
            return null;
        }

        public static Dictionary<Screen, string> GetScreensWithName()
        {
            var result = new Dictionary<Screen, string>();
            var screens = Screen.AllScreens;
            var allNames = GetAllMonitorsFriendlyNames().ToArray();
            for (int i = 0; i < screens.Length; i++)
                result.Add(screens[i], allNames.Length >= i ? screens[i].DeviceName : allNames[i]);

            return result;
        }
    }
}
