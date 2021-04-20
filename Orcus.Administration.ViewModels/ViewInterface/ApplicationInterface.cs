namespace Orcus.Administration.ViewModels.ViewInterface
{
    public static class ApplicationInterface
    {
        public static bool ForceShutdown { get; set; }
        public static int ClientVersion { get; } = 19;
    }
}