using Microsoft.Win32;

namespace Orcus.Utilities
{
    public class ProxyHelper
    {
        public static bool GetSystemProxy(out string ipAddress, out int port)
        {
            ipAddress = null;
            port = 0;

            using (
                var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings")
                )
            {
                var proxyEnabled = (int) key.GetValue("ProxyEnable", 0) == 1;
                if (!proxyEnabled)
                    return false;

                var server = (string) key.GetValue("ProxyServer", "");
                if (string.IsNullOrEmpty(server) || server.StartsWith("http"))
                    return false;

                var parts = server.Split(new[] {':'}, 2);
                ipAddress = parts[0];
                port = int.Parse(parts[1]);
                return true;
            }
        }
    }
}