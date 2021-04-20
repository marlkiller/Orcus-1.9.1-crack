using System;
using System.Windows;
using Fclp;

namespace Orcus.Administration.Core.CLA
{
    public class CommandLineArgs
    {
        private static CommandLineArgs _current;

        public bool OpenLanguageCreator { get; set; }
        public bool OpenHardwareIdViewer { get; set; }

        public string SettingsFilePath { get; set; }
        public string LicenseFilePath { get; set; }

        public string ServerAddress { get; set; }
        public int Port { get; set; }
        public string Password { get; set; }
        public bool AutoConnect { get; set; }

        public static CommandLineArgs Current => _current ?? (_current = LoadCommandLineArgs());

        private static CommandLineArgs LoadCommandLineArgs()
        {
            var args = Environment.GetCommandLineArgs();
            var p = new FluentCommandLineParser<CommandLineArgs> {IsCaseSensitive = false};
            p.Setup(x => x.LicenseFilePath).As("license").SetDefault(null);
            p.Setup(x => x.SettingsFilePath).As("settings");
            p.Setup(x => x.ServerAddress).As('s', "server");
            p.Setup(x => x.Port).As('p', "port");
            p.Setup(x => x.Password).As("password");
            p.Setup(x => x.AutoConnect).As('a', "autoconnect");
            p.Setup(x => x.OpenLanguageCreator).As("languageCreator");
            p.Setup(x => x.OpenHardwareIdViewer).As("hardwareid");
            p.Setup(x => args.Length);

            var result = p.Parse(args);
            if (result.HasErrors)
            {
                MessageBox.Show(result.ErrorText, (string) Application.Current.Resources["Error"], MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return new CommandLineArgs();
            }

            return p.Object;
        }
    }
}