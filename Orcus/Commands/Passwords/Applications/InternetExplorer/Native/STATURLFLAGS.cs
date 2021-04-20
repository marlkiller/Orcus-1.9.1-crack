namespace Orcus.Commands.Passwords.Applications.InternetExplorer.Native
{
    /// <summary>
    ///     Flag on the dwFlags parameter of the STATURL structure, used by the SetFilter method.
    /// </summary>
    public enum STATURLFLAGS : uint
    {
        /// <summary>
        ///     Flag on the dwFlags parameter of the STATURL structure indicating that the item is in the cache.
        /// </summary>
        STATURLFLAG_ISCACHED = 0x00000001,

        /// <summary>
        ///     Flag on the dwFlags parameter of the STATURL structure indicating that the item is a top-level item.
        /// </summary>
        STATURLFLAG_ISTOPLEVEL = 0x00000002
    }
}