using System;
using System.IO;
using Microsoft.Win32;

namespace Orcus.Commands.WindowsCustomizer.Core
{
    public static class General
    {
        private static readonly string GodModePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "God-Mode.{ED7BA470-8E54-465E-825C-99712043E01C}");

        public static bool GodMode
        {
            get { return Directory.Exists(GodModePath); }
            set
            {
                if (value)
                    Directory.CreateDirectory(GodModePath);
                else
                    Directory.Delete(GodModePath);
            }
        }

        public static bool ConfirmFileDelete
        {
            get
            {
                return
                    RegistryUtilities.GetIntValueSafe(
                        Registry.CurrentUser.OpenSubKey(
                            "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer", false),
                        "ConfirmFileDelete") == 1;
            }
            set
            {
                RegistryUtilities.SetValueSafe(Registry.CurrentUser,
                    "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer",
                    "ConfirmFileDelete", value ? 1 : 0, RegistryValueKind.DWord);
            }
        }

        /// <summary>
        ///     To prevent Automatic Updates from restarting a computer while users are logged on, the administrator can create a
        ///     NoAutoRebootWithLoggedOnUsers registry value. The value is a DWORD, and it must be 0 (false) or 1 (true). If this
        ///     value is changed while the computer is in a restart pending state, the changed state will not take effect until the
        ///     next time an update requires a computer restart.
        /// </summary>
        public static bool AutoRebootWithLoggedOnUsers
        {
            get
            {
                return
                    RegistryUtilities.GetIntValueSafe(
                        Registry.LocalMachine.OpenSubKey(
                            "SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate\\AU", false),
                        "NoAutoRebootWithLoggedOnUsers") != 1;
            }
            set
            {
                RegistryUtilities.SetValueSafe(Registry.LocalMachine,
                    "SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate\\AU",
                    "NoAutoRebootWithLoggedOnUsers", value ? 0 : 1, RegistryValueKind.DWord);
            }
        }

        /// <summary>
        /// Some critical security updates will force you to install them by removing the Shutdown button and adding Install Updates and then shut down computer
        /// </summary>
        public static bool EnableAUAsDefaultShutdownOption
        {
            get
            {
                return
                    RegistryUtilities.GetIntValueSafe(
                        Registry.LocalMachine.OpenSubKey(
                            "SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate\\AU", false),
                        "NoAUAsDefaultShutdownOption") != 1;
            }
            set
            {
                RegistryUtilities.SetValueSafe(Registry.LocalMachine,
                    "SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate\\AU",
                    "NoAUAsDefaultShutdownOption", value ? 0 : 1, RegistryValueKind.DWord);
            }
        }

        /// <summary>
        /// This restriction allows you to disable the use of the Windows hotkey combinations that provide shortcuts to the Start Menu and task swapping.
        /// </summary>
        public static bool EnableWinKeys
        {
            get
            {
                return
                    RegistryUtilities.GetIntValueSafe(
                        Registry.CurrentUser.OpenSubKey(
                            "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer", false),
                        "NoWinKeys") != 1;
            }
            set
            {
                RegistryUtilities.SetValueSafe(Registry.CurrentUser,
                    "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer",
                    "NoWinKeys", value ? 0 : 1, RegistryValueKind.DWord);
            }
        }

        /// <summary>
        /// Enable the annoying "Use the Web service to find the correct program" dialog
        /// </summary>
        public static bool EnableInternetOpenWith
        {
            get
            {
                return
                    RegistryUtilities.GetIntValueSafe(
                        Registry.LocalMachine.OpenSubKey(
                            "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer", false),
                        "NoInternetOpenWith") != 1;
            }
            set
            {
                RegistryUtilities.SetValueSafe(Registry.LocalMachine,
                    "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer",
                    "NoInternetOpenWith", value ? 0 : 1, RegistryValueKind.DWord);
            }
        }

        /// <summary>
        /// Automatic restart after a system crash.
        /// </summary>
        public static bool AutoReboot
        {
            get
            {
                return
                    RegistryUtilities.GetIntValueSafe(
                        Registry.LocalMachine.OpenSubKey(
                            "SYSTEM\\CurrentControlSet\\Control\\CrashControl", false),
                        "AutoReboot") == 1;
            }
            set
            {
                RegistryUtilities.SetValueSafe(Registry.LocalMachine,
                    "SYSTEM\\CurrentControlSet\\Control\\CrashControl",
                    "AutoReboot", value ? 1 : 0, RegistryValueKind.DWord);
            }
        }

        public static bool DoErrorReport
        {
            get
            {
                return
                    RegistryUtilities.GetIntValueSafe(
                        Registry.LocalMachine.OpenSubKey(
                            "SOFTWARE\\Microsoft\\PCHealth\\ErrorReporting", false),
                        "DoReport") == 1;
            }
            set
            {
                using (var regKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\PCHealth\\ErrorReporting", RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    // ReSharper disable once PossibleNullReferenceException
                    regKey.SetValue("DoReport", value ? 1 : 0, RegistryValueKind.DWord);
                    regKey.SetValue("ShowUI", value ? 1 : 0, RegistryValueKind.DWord);
                    regKey.SetValue("IncludeKernelFaults", value ? 1 : 0, RegistryValueKind.DWord);
                    regKey.SetValue("IncludeMicrosoftApps", value ? 1 : 0, RegistryValueKind.DWord);
                    regKey.SetValue("IncludeWindowsApps", value ? 1 : 0, RegistryValueKind.DWord);
                }
            }
        }

        public static bool FilePrintSharing
        {
            get
            {
                return
                    RegistryUtilities.GetIntValueSafe(
                        Registry.LocalMachine.OpenSubKey(
                            "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies", false),
                        "NoFileSharing") != 1 &&
                    RegistryUtilities.GetIntValueSafe(
                        Registry.LocalMachine.OpenSubKey(
                            "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies", false),
                        "NoPrintSharing") != 1;
            }
            set
            {
                using (var regKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies", RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    // ReSharper disable once PossibleNullReferenceException
                    regKey.SetValue("NoFileSharing", value ? 0 : 1, RegistryValueKind.DWord);
                    regKey.SetValue("NoPrintSharing", value ? 0 : 1, RegistryValueKind.DWord);
                }
            }
        }

        public static bool KernelPaging
        {
            get
            {
                return
                    RegistryUtilities.GetIntValueSafe(
                        Registry.LocalMachine.OpenSubKey(
                            "SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Memory Management", false),
                        "DisablePagingExecutive") != 1;
            }
            set
            {
                RegistryUtilities.SetValueSafe(Registry.LocalMachine,
                    "SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Memory Management",
                    "DisablePagingExecutive", value ? 0 : 1, RegistryValueKind.DWord);
            }
        }

        public static bool ClearPageFile
        {
            get
            {
                return
                    RegistryUtilities.GetIntValueSafe(
                        Registry.LocalMachine.OpenSubKey(
                            "SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Memory Management", false),
                        "ClearPageFileAtShutdown") == 1;
            }
            set
            {
                RegistryUtilities.SetValueSafe(Registry.LocalMachine,
                    "SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Memory Management",
                    "ClearPageFileAtShutdown", value ? 1 : 0, RegistryValueKind.DWord);
            }
        }

        public static bool BootDefragmentation
        {
            get
            {
                return
                    RegistryUtilities.GetStringValueSafe(
                        Registry.LocalMachine.OpenSubKey(
                            "SOFTWARE\\Microsoft\\Dfrg\\BootOptimizeFunction", false),
                        "Enable") == "Y";
            }
            set
            {
                RegistryUtilities.SetValueSafe(Registry.LocalMachine,
                    "SOFTWARE\\Microsoft\\Dfrg\\BootOptimizeFunction",
                    "Enable", value ? "Y" : "N", RegistryValueKind.String);
            }
        }

        public static bool ReserveBandwidthForSystem
        {
            get
            {
                return
                    RegistryUtilities.GetIntValueSafe(
                        Registry.LocalMachine.OpenSubKey(
                            "SOFTWARE\\Policies\\Microsoft\\Windows\\Psched", false),
                        "NonBestEffortLimit") == 1;
            }
            set
            {
                RegistryUtilities.SetValueSafe(Registry.LocalMachine,
                    "SOFTWARE\\Policies\\Microsoft\\Windows\\Psched",
                    "NonBestEffortLimit", value ? 1 : 0, RegistryValueKind.DWord);
            }
        }

        /// <summary>
        /// Receive verbose startup, shutdown, logon, and logoff status messages
        /// </summary>
        public static bool VerboseLogging
        {
            get
            {
                return
                    RegistryUtilities.GetIntValueSafe(
                        Registry.LocalMachine.OpenSubKey(
                            "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System", false),
                        "VerboseStatus") == 1;
            }
            set
            {
                RegistryUtilities.SetValueSafe(Registry.LocalMachine,
                    "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System",
                    "VerboseStatus", value ? 1 : 0, RegistryValueKind.DWord);
            }
        }

        public static bool SeparateExplorerProcess
        {
            get
            {
                return
                    RegistryUtilities.GetIntValueSafe(
                        Registry.CurrentUser.OpenSubKey(
                            "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced", false),
                        "SeperateProcess") == 1;
            }
            set
            {
                RegistryUtilities.SetValueSafe(Registry.CurrentUser,
                    "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced",
                    "SeperateProcess", value ? 1 : 0, RegistryValueKind.DWord);
            }
        }

        public static bool CrashOnCtrlScroll
        {
            get
            {
                return
                    RegistryUtilities.GetIntValueSafe(
                        Registry.LocalMachine.OpenSubKey(
                            "SYSTEM\\CurrentControlSet\\Services\\kbdhid\\Parameters", false),
                        "CrashOnCtrlScroll") == 1 &&
                    RegistryUtilities.GetIntValueSafe(
                        Registry.LocalMachine.OpenSubKey(
                            "SYSTEM\\CurrentControlSet\\Services\\i8042prt\\Parameters", false),
                        "CrashOnCtrlScroll") == 1;
            }
            set
            {
                RegistryUtilities.SetValueSafe(Registry.LocalMachine,
                    "SYSTEM\\CurrentControlSet\\Services\\kbdhid\\Parameters",
                    "CrashOnCtrlScroll", value ? 1 : 0, RegistryValueKind.DWord);
                RegistryUtilities.SetValueSafe(Registry.LocalMachine,
                    "SYSTEM\\CurrentControlSet\\Services\\i8042prt\\Parameters",
                    "CrashOnCtrlScroll", value ? 1 : 0, RegistryValueKind.DWord);
            }
        }

        public static bool MobilityCenter
        {
            get
            {
                return
                    RegistryUtilities.GetIntValueSafe(
                        Registry.CurrentUser.OpenSubKey(
                            "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\MobilityCenter", false),
                        "NoMobilityCenter") != 1;
            }
            set
            {
                RegistryUtilities.SetValueSafe(Registry.CurrentUser,
                    "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\MobilityCenter",
                    "NoMobilityCenter", value ? 0 : 1, RegistryValueKind.DWord);
            }
        }
    }
}