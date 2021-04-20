namespace Orcus.Commands.Passwords.Applications.InternetExplorer.Native
{
    public enum STATURL_QUERYFLAGS : uint
    {
        /// <summary>
        ///     The specified URL is in the content cache.
        /// </summary>
        STATURL_QUERYFLAG_ISCACHED = 0x00010000,

        /// <summary>
        ///     Space for the URL is not allocated when querying for STATURL.
        /// </summary>
        STATURL_QUERYFLAG_NOURL = 0x00020000,

        /// <summary>
        ///     Space for the Web page's title is not allocated when querying for STATURL.
        /// </summary>
        STATURL_QUERYFLAG_NOTITLE = 0x00040000,

        /// <summary>
        ///     //The item is a top-level item.
        /// </summary>
        STATURL_QUERYFLAG_TOPLEVEL = 0x00080000
    }
}