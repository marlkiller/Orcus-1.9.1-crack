using Microsoft.Win32;

namespace Orcus.Commands.WindowsCustomizer.Core
{
    public static class DesktopProperties
    {
        public static bool DisplayWindowsVersion
        {
            get
            {
                return
                    RegistryUtilities.GetIntValueSafe(Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", false),
                        "PaintDesktopVersion") == 1;
            }
            set
            {
                RegistryUtilities.SetValueSafe(Registry.CurrentUser, "Control Panel\\Desktop", "PaintDesktopVersion",
                    value ? 1 : 0, RegistryValueKind.DWord);
            }
        }

        public static bool DisplayTrayItems
        {
            get
            {
                return
                    RegistryUtilities.GetIntValueSafe(
                        Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies", false),
                        "NoTrayItemsDisplay") != 1;
            }
            set
            {
                RegistryUtilities.SetValueSafe(Registry.LocalMachine,
                    "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies", "NoTrayItemsDisplay",
                    value ? 0 : 1, RegistryValueKind.DWord);
            }
        }

        public static bool WindowAnimation
        {
            get
            {
                return
                    RegistryUtilities.GetIntValueSafe(
                        Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop\\WindowMetrics", false),
                        "MinAnimate") != 0;
            }
            set
            {
                RegistryUtilities.SetValueSafe(Registry.CurrentUser,
                    "Control Panel\\Desktop\\WindowMetrics", "MinAnimate",
                    value ? 1 : 0, RegistryValueKind.String);
            }
        }

        public static bool AeroShake
        {
            get
            {
                return
                    RegistryUtilities.GetIntValueSafe(
                        Registry.CurrentUser.OpenSubKey("Software\\Policies\\Microsoft\\Windows\\Explorer", false),
                        "NoWindowMinimizingShortcuts") != 0;
            }
            set
            {
                RegistryUtilities.SetValueSafe(Registry.CurrentUser,
                    "Software\\Policies\\Microsoft\\Windows\\Explorer", "NoWindowMinimizingShortcuts",
                    value ? 1 : 0, RegistryValueKind.DWord);
            }
        }

        public static bool WindowSnap
        {
            get
            {
                return
                    RegistryUtilities.GetIntValueSafe(
                        Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", false),
                        "WindowArrangementActive") != 0;
            }
            set
            {
                RegistryUtilities.SetValueSafe(Registry.CurrentUser,
                    "Control Panel\\Desktop", "WindowArrangementActive",
                    value ? 1 : 0, RegistryValueKind.DWord);
            }
        }

        public static bool NotificationBalloons
        {
            get
            {
                return
                    RegistryUtilities.GetIntValueSafe(
                        Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced", false),
                        "EnableBalloonTips") != 0;
            }
            set
            {
                RegistryUtilities.SetValueSafe(Registry.CurrentUser,
                    "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced", "EnableBalloonTips",
                    value ? 1 : 0, RegistryValueKind.DWord);
            }
        }

        public static bool LibrariesOnDesktop
        {
            get
            {
                return
                    RegistryUtilities.GetIntValueSafe(
                        Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\HideDesktopIcons\\NewStartPanel", false),
                        "{031E4825-7B94-4dc3-B131-E946B44C8DD5}") != 0;
            }
            set
            {
                RegistryUtilities.SetValueSafe(Registry.CurrentUser,
                    "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\HideDesktopIcons\\NewStartPanel", "{031E4825-7B94-4dc3-B131-E946B44C8DD5}",
                    value ? 1 : 0, RegistryValueKind.DWord);
            }
        }

        public static bool RecycleBinOnComputer
        {
            get
            {
                return
                    Registry.CurrentUser.OpenSubKey(
                        "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\MyComputer\\NameSpace\\{645FF040-5081-101B-9F08-00AA002F954E}",
                        false) != null;
            }
            set
            {
                using (var registryKey = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\MyComputer\\NameSpace"))
                    if (value)
                        registryKey.CreateSubKey("{645FF040-5081-101B-9F08-00AA002F954E}").Close();
                    else
                        registryKey.DeleteSubKey("{645FF040-5081-101B-9F08-00AA002F954E}", false);
            }
        }

        public static bool DesktopPreview
        {
            get
            {
                return
                    RegistryUtilities.GetIntValueSafe(
                        Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced", false),
                        "DisablePreviewDesktop") != 1;
            }
            set
            {
                RegistryUtilities.SetValueSafe(Registry.CurrentUser,
                    "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced", "DisablePreviewDesktop",
                    value ? 0 : 1, RegistryValueKind.DWord);
            }
        }

        public static bool ExplorerCheckBoxSelection
        {
            get
            {
                return
                    RegistryUtilities.GetIntValueSafe(
                        Registry.CurrentUser.OpenSubKey(
                            "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced", false),
                        "AutoCheckSelect") == 1;
            }
            set
            {
                RegistryUtilities.SetValueSafe(Registry.CurrentUser,
                    "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced", "AutoCheckSelect", value ? 1 : 0,
                    RegistryValueKind.DWord);
            }
        }
    }
}