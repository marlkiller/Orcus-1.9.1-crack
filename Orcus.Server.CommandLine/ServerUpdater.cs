using System;
using System.Net;

namespace Orcus.Server.CommandLine
{
    public static class ServerUpdater
    {
        private const string CurrentVersion = "1.14";

        public static bool IsUpdateAvailable()
        {
            using (var webClient = new WebClient())
            {
                var newestVersion =
                    ParseVersion(webClient.DownloadString("http://orcus.pw/orcusapp/OrcusServer.php?method=cv"));
                var currentVersion = ParseVersion(CurrentVersion);

                return newestVersion > currentVersion;
            }
        }

        public static void DownloadUpdates(string path)
        {
            using (var webClient = new WebClient())
            {
                webClient.DownloadFile("http://orcus.pw/orcusapp/OrcusServer.php?method=cu", path);
            }
        }

        private static Version ParseVersion(string version)
        {
            var parts = version.Trim().Split('.');
            if (parts.Length != 2)
                throw new Exception("Invalid amount of version parts: " + parts.Length);

            return new Version(int.Parse(parts[0]), int.Parse(parts[1]));
        }
    }
}