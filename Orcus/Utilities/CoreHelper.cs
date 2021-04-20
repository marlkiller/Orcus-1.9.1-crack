using System;

namespace Orcus.Utilities
{
    public static class CoreHelper
    {
        public static bool RunningOnXP => Environment.OSVersion.Platform == PlatformID.Win32NT &&
                                          Environment.OSVersion.Version.Major < 6;

        public static bool RunningOnXPOrGreater => Environment.OSVersion.Platform == PlatformID.Win32NT &&
                                                   Environment.OSVersion.Version.Major >= 5;

        public static bool RunningOnVista
            =>
                Environment.OSVersion.Platform == PlatformID.Win32NT &&
                Environment.OSVersion.Version.Equals(new Version(6, 0));

        public static bool RunningOnVistaOrGreater => Environment.OSVersion.Platform == PlatformID.Win32NT &&
                                                      Environment.OSVersion.Version.Major >= 6;

        public static bool RunningOnWin7 => Environment.OSVersion.Platform == PlatformID.Win32NT &&
                                            Environment.OSVersion.Version.Equals(new Version(6, 1));

        public static bool RunningOnWin7OrGreater => Environment.OSVersion.Platform == PlatformID.Win32NT &&
                                                     Environment.OSVersion.Version.CompareTo(new Version(6, 1)) >= 0;

        public static bool RunningOnWin8 => Environment.OSVersion.Platform == PlatformID.Win32NT &&
                                            Environment.OSVersion.Version.Equals(new Version(6, 2));

        public static bool RunningOnWin8OrGreater => Environment.OSVersion.Platform == PlatformID.Win32NT &&
                                                     Environment.OSVersion.Version.CompareTo(new Version(6, 2)) >= 0;

        public static bool RunningOnWin8d1 => Environment.OSVersion.Platform == PlatformID.Win32NT &&
                                              Environment.OSVersion.Version.Equals(new Version(6, 3));

        public static bool RunningOnWin8d1OrGreater => Environment.OSVersion.Platform == PlatformID.Win32NT &&
                                                       Environment.OSVersion.Version.CompareTo(new Version(6, 3)) >= 0;

        public static bool RunningOnWin10 => Environment.OSVersion.Platform == PlatformID.Win32NT &&
                                             Environment.OSVersion.Version.Equals(new Version(10, 0));

        public static bool RunningOnWin10OrGreater => Environment.OSVersion.Platform == PlatformID.Win32NT &&
                                                      Environment.OSVersion.Version.CompareTo(new Version(10, 0)) >= 0;
    }
}