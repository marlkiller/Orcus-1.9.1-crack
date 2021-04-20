#if DEBUG
using System.Windows.Forms;

#else
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;

#endif

namespace Orcus.Service
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        private static void Main(string[] args)
        {
#if (!DEBUG)

            if (System.Environment.UserInteractive)
            {
                var parameter = string.Concat(args);
                switch (parameter)
                {
                    case "--install":
                        ManagedInstallerClass.InstallHelper(new[] { "/LogFile=", Assembly.GetExecutingAssembly().Location });
                        break;
                    case "--uninstall":
                        ManagedInstallerClass.InstallHelper(new[] { "/u", "/LogFile=", Assembly.GetExecutingAssembly().Location });
                        break;
                }
            }
            else
            {
                ServiceBase.Run(new WindowsService());
            }
#else
            var myServ = new WindowsService();
            myServ.Start();
            Application.Run(new AppContext());
#endif
        }
    }

#if DEBUG
    internal class AppContext : ApplicationContext
    {
    }
#endif
}