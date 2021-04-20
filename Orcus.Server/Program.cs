using System;
using System.Linq;
using System.Windows.Forms;
using NLog;
using NLog.Layouts;
using Orcus.Server.Core.Utilities;

namespace Orcus.Server
{
    internal static class Program
    {
        public static string LogLayout { get; private set; }
        public static LogLevel MinLogLevel { get; private set; }

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            NLogUtils.CreateConfigFileIfNotExists();

            SimpleLayout simpleLayout;
            LogLayout = LogManager.Configuration.Variables.TryGetValue("TextBoxLayout", out simpleLayout)
                ? simpleLayout.Text
                : "${date:format=HH\\:MM\\:ss.ffff} [${level:upperCase=true}]\t[${logger:shortName=true}] ${message}";

            MinLogLevel = LogManager.Configuration.Variables.TryGetValue("MinLogLevel", out simpleLayout)
                ? LogLevel.FromString(simpleLayout.Text)
                : LogLevel.Info;

            if (
                args.Any(
                    x =>
                        string.Equals(x, "/v", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(x, "/verbose", StringComparison.OrdinalIgnoreCase)))
            {
                MinLogLevel = LogLevel.Debug;
                EnabledDebugForAllRules();
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ServerApplicationContext());
        }

        private static void EnabledDebugForAllRules()
        {
            foreach (var rule in LogManager.Configuration.LoggingRules)
            {
                rule.EnableLoggingForLevel(LogLevel.Debug);
            }

            //Call to update existing Loggers created with GetLogger() or 
            //GetCurrentClassLogger()
            LogManager.ReconfigExistingLoggers();
        }
    }
}