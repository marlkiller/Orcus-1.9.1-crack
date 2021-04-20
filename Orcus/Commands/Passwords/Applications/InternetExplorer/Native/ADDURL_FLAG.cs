namespace Orcus.Commands.Passwords.Applications.InternetExplorer.Native
{
    /// <summary>
    ///     Used bu the AddHistoryEntry method.
    /// </summary>
    public enum ADDURL_FLAG : uint
    {
        /// <summary>
        ///     Write to both the visited links and the dated containers.
        /// </summary>
        ADDURL_ADDTOHISTORYANDCACHE = 0,

        /// <summary>
        ///     Write to only the visited links container.
        /// </summary>
        ADDURL_ADDTOCACHE = 1
    }
}