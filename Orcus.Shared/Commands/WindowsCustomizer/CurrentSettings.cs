using System;

namespace Orcus.Shared.Commands.WindowsCustomizer
{
    [Serializable]
    public class CurrentSettings
    {
        //General
        public bool GodMode { get; set; }
        public bool ConfirmFileDelete { get; set; }
        public bool AutoRebootWithLoggedOnUsers { get; set; }
        public bool EnableAUAsDefaultShutdownOption { get; set; }
        public bool EnableWinKeys { get; set; }
        public bool EnableInternetOpenWith { get; set; }
        public bool AutoReboot { get; set; }
        public bool DoErrorReport { get; set; }
        public bool FilePrintSharing { get; set; }
        public bool KernelPaging { get; set; }
        public bool ClearPageFile { get; set; }
        public bool BootDefragmentation { get; set; }
        public bool ReserveBandwidthForSystem { get; set; }
        public bool VerboseLogging { get; set; }
        public bool SeparateExplorerProcess { get; set; }
        public bool CrashOnCtrlScroll { get; set; }
        public bool MobilityCenter { get; set; }

        //Desktop
        public bool DisplayWindowsVersion { get; set; }
        public bool DisplayTrayItems { get; set; }
        public bool WindowAnimation { get; set; }
        public bool AeroShake { get; set; }
        public bool WindowSnap { get; set; }
        public bool NotificationBalloons { get; set; }
        public bool LibrariesOnDesktop { get; set; }
        public bool RecycleBinOnComputer { get; set; }
        public bool DesktopPreview { get; set; }
        public bool ExplorerCheckBoxSelection { get; set; }

        //Windows 10
        public bool IsWindows10Enabled { get; set; }
        public bool LockScreen { get; set; }
        public bool DarkTheme { get; set; }
        public bool BalloonNotifications { get; set; }
        public bool ActionCenter { get; set; }
        public bool ClassicVolumeMixer { get; set; }
    }
}