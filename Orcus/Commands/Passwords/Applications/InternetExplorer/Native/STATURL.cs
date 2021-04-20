using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace Orcus.Commands.Passwords.Applications.InternetExplorer.Native
{
    /// <summary>
    ///     The structure that contains statistics about a URL.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct STATURL
    {
        /// <summary>
        ///     Struct size
        /// </summary>
        public int cbSize;

        /// <summary>
        ///     URL
        /// </summary>
        [MarshalAs(UnmanagedType.LPWStr)] public string pwcsUrl;

        /// <summary>
        ///     Page title
        /// </summary>
        [MarshalAs(UnmanagedType.LPWStr)] public string pwcsTitle;

        /// <summary>
        ///     Last visited date (UTC)
        /// </summary>
        public System.Runtime.InteropServices.ComTypes.FILETIME ftLastVisited;

        /// <summary>
        ///     Last updated date (UTC)
        /// </summary>
        public System.Runtime.InteropServices.ComTypes.FILETIME ftLastUpdated;

        /// <summary>
        ///     The expiry date of the Web page's content (UTC)
        /// </summary>
        public System.Runtime.InteropServices.ComTypes.FILETIME ftExpires;

        /// <summary>
        ///     Flags. STATURLFLAGS Enumaration.
        /// </summary>
        public STATURLFLAGS dwFlags;

        /// <summary>
        ///     sets a column header in the DataGrid control. This property is not needed if you do not use it.
        /// </summary>
        public string URL => pwcsUrl;

        public string UrlString
        {
            get
            {
                int index = pwcsUrl.IndexOf('?');
                return index < 0 ? pwcsUrl : pwcsUrl.Substring(0, index);
            }
        }

        /// <summary>
        ///     sets a column header in the DataGrid control. This property is not needed if you do not use it.
        /// </summary>
        public string Title
        {
            get
            {
                if (pwcsUrl.StartsWith("file:"))
                    return Win32api.CannonializeURL(pwcsUrl, Win32api.shlwapi_URL.URL_UNESCAPE).Substring(8).Replace(
                        '/', '\\');
                return pwcsTitle;
            }
        }

        /// <summary>
        ///     sets a column header in the DataGrid control. This property is not needed if you do not use it.
        /// </summary>
        public DateTime LastVisited => Win32api.FileTimeToDateTime(ftLastVisited).ToLocalTime();

        /// <summary>
        ///     sets a column header in the DataGrid control. This property is not needed if you do not use it.
        /// </summary>
        public DateTime LastUpdated => Win32api.FileTimeToDateTime(ftLastUpdated).ToLocalTime();

        /// <summary>
        ///     sets a column header in the DataGrid control. This property is not needed if you do not use it.
        /// </summary>
        public DateTime Expires
        {
            get
            {
                try
                {
                    return Win32api.FileTimeToDateTime(ftExpires);
                }
                catch (Exception)
                {
                    return DateTime.UtcNow;
                }
            }
        }

        public override string ToString()
        {
            return pwcsUrl;
        }
    }
}