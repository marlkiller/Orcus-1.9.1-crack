using System.Windows;

namespace Orcus.Administration.Core.Utilities
{
    public static class ApplicationUtilities
    {
        public static void Restart(this Application application)
        {
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            application.Shutdown();
        }

        public static void Restart(this Application application, string arguments)
        {
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location, arguments);
            application.Shutdown();
        }
    }
}