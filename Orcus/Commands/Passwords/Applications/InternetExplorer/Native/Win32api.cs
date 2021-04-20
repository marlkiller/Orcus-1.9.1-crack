using System;
using System.Runtime.InteropServices;
using System.Text;

// ReSharper disable InconsistentNaming

namespace Orcus.Commands.Passwords.Applications.InternetExplorer.Native
{
    public class Win32api
    {
        /// <summary>
        ///     Used by CannonializeURL method.
        /// </summary>
        [Flags]
        public enum shlwapi_URL : uint
        {
            /// <summary>
            ///     Treat "/./" and "/../" in a URL string as literal characters, not as shorthand for navigation.
            /// </summary>
            URL_DONT_SIMPLIFY = 0x08000000,

            /// <summary>
            ///     Convert any occurrence of "%" to its escape sequence.
            /// </summary>
            URL_ESCAPE_PERCENT = 0x00001000,

            /// <summary>
            ///     Replace only spaces with escape sequences. This flag takes precedence over URL_ESCAPE_UNSAFE, but does not apply to
            ///     opaque URLs.
            /// </summary>
            URL_ESCAPE_SPACES_ONLY = 0x04000000,

            /// <summary>
            ///     Replace unsafe characters with their escape sequences. Unsafe characters are those characters that may be altered
            ///     during transport across the Internet, and include the (<, >, ", #, {, }, |, \, ^, ~, [, ], and ') characters. This
            ///     flag applies to all URLs, including opaque URLs.
            /// </summary>
            URL_ESCAPE_UNSAFE = 0x20000000,

            /// <summary>
            ///     Combine URLs with client-defined pluggable protocols, according to the World Wide Web Consortium (W3C)
            ///     specification. This flag does not apply to standard protocols such as ftp, http, gopher, and so on. If this flag is
            ///     set, UrlCombine does not simplify URLs, so there is no need to also set URL_DONT_SIMPLIFY.
            /// </summary>
            URL_PLUGGABLE_PROTOCOL = 0x40000000,

            /// <summary>
            ///     Un-escape any escape sequences that the URLs contain, with two exceptions. The escape sequences for "?" and "#" are
            ///     not un-escaped. If one of the URL_ESCAPE_XXX flags is also set, the two URLs are first un-escaped, then combined,
            ///     then escaped.
            /// </summary>
            URL_UNESCAPE = 0x10000000
        }

        public const uint SHGFI_ATTR_SPECIFIED = 0x20000;
        public const uint SHGFI_ATTRIBUTES = 0x800;
        public const uint SHGFI_PIDL = 0x8;
        public const uint SHGFI_DISPLAYNAME = 0x200;
        public const uint SHGFI_USEFILEATTRIBUTES = 0x10;
        public const uint FILE_ATTRIBUTRE_NORMAL = 0x4000;
        public const uint SHGFI_EXETYPE = 0x2000;
        public const uint SHGFI_SYSICONINDEX = 0x4000;
        public const uint ILC_COLORDDB = 0x1;
        public const uint ILC_MASK = 0x0;
        public const uint ILD_TRANSPARENT = 0x1;
        public const uint SHGFI_ICON = 0x100;
        public const uint SHGFI_LARGEICON = 0x0;
        public const uint SHGFI_SHELLICONSIZE = 0x4;
        public const uint SHGFI_SMALLICON = 0x1;
        public const uint SHGFI_TYPENAME = 0x400;
        public const uint SHGFI_ICONLOCATION = 0x1000;

        [DllImport("shlwapi.dll")]
        public static extern int UrlCanonicalize(
            string pszUrl,
            StringBuilder pszCanonicalized,
            ref int pcchCanonicalized,
            shlwapi_URL dwFlags
            );


        /// <summary>
        ///     Takes a URL string and converts it into canonical form
        /// </summary>
        /// <param name="pszUrl">URL string</param>
        /// <param name="dwFlags">shlwapi_URL Enumeration. Flags that specify how the URL is converted to canonical form.</param>
        /// <returns>The converted URL</returns>
        public static string CannonializeURL(string pszUrl, shlwapi_URL dwFlags)
        {
            var buff = new StringBuilder(260);
            int s = buff.Capacity;
            int c = UrlCanonicalize(pszUrl, buff, ref s, dwFlags);
            if (c == 0)
                return buff.ToString();
            buff.Capacity = s;
            c = UrlCanonicalize(pszUrl, buff, ref s, dwFlags);
            return buff.ToString();
        }

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool FileTimeToSystemTime
            (ref System.Runtime.InteropServices.ComTypes.FILETIME FileTime, ref SYSTEMTIME SystemTime);


        /// <summary>
        ///     Converts a file time to DateTime format.
        /// </summary>
        /// <param name="filetime">FILETIME structure</param>
        /// <returns>DateTime structure</returns>
        public static DateTime FileTimeToDateTime(System.Runtime.InteropServices.ComTypes.FILETIME filetime)
        {
            var st = new SYSTEMTIME();
            FileTimeToSystemTime(ref filetime, ref st);
            try
            {
                return new DateTime(st.Year, st.Month, st.Day, st.Hour, st.Minute, st.Second, st.Milliseconds);
            }
            catch (Exception)
            {
                return DateTime.UtcNow;
            }
        }

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool SystemTimeToFileTime([In] ref SYSTEMTIME lpSystemTime,
            out System.Runtime.InteropServices.ComTypes.FILETIME lpFileTime);


        /// <summary>
        ///     Converts a DateTime to file time format.
        /// </summary>
        /// <param name="datetime">DateTime structure</param>
        /// <returns>FILETIME structure</returns>
        public static System.Runtime.InteropServices.ComTypes.FILETIME DateTimeToFileTime(DateTime datetime)
        {
            var st = new SYSTEMTIME
            {
                Year = (short) datetime.Year,
                Month = (short) datetime.Month,
                Day = (short) datetime.Day,
                Hour = (short) datetime.Hour,
                Minute = (short) datetime.Minute,
                Second = (short) datetime.Second,
                Milliseconds = (short) datetime.Millisecond
            };
            System.Runtime.InteropServices.ComTypes.FILETIME filetime;
            SystemTimeToFileTime(ref st, out filetime);
            return filetime;
        }

        //compares two file times.
        [DllImport("Kernel32.dll")]
        public static extern int CompareFileTime([In] ref System.Runtime.InteropServices.ComTypes.FILETIME lpFileTime1,
            [In] ref System.Runtime.InteropServices.ComTypes.FILETIME lpFileTime2);


        //Retrieves information about an object in the file system.
        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi,
            uint cbSizeFileInfo, uint uFlags);

        public struct SYSTEMTIME
        {
            public Int16 Day;
            public Int16 DayOfWeek;
            public Int16 Hour;
            public Int16 Milliseconds;
            public Int16 Minute;
            public Int16 Month;
            public Int16 Second;
            public Int16 Year;
        }
    }
}