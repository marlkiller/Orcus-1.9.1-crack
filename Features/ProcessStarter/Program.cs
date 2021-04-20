using System;
using System.Diagnostics;
using System.Text;

namespace ProcessStarter
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] parms)
        {
            if (parms.Length == 0)
                return;

            var path = Encoding.UTF8.GetString(Convert.FromBase64String(parms[0]));
            var arguments = "";
            if (parms.Length > 1)
                arguments = Encoding.UTF8.GetString(Convert.FromBase64String(parms[1]));

            Process.Start(path, arguments);
        }
    }
}