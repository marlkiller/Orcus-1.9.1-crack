using System;
using System.IO;
using System.Windows.Forms;
using Orcus.Plugins;
using Orcus.Shared.Settings;

namespace Orcus.Config
{
    internal class Consts : IPathInformation
    {
        static Consts()
        {
            var mutex = Settings.GetBuilderProperty<MutexBuilderProperty>().Mutex;
            var dataFolder = Settings.GetBuilderProperty<DataFolderBuilderProperty>().Path;

            KeyLogFile = Path.Combine(Environment.ExpandEnvironmentVariables(dataFolder),
                $"klg_{mutex}.dat");
            ExceptionFile = Path.Combine(Environment.ExpandEnvironmentVariables(dataFolder),
                $"err_{mutex}.dat");
            PluginsDirectory = Path.Combine(Environment.ExpandEnvironmentVariables(dataFolder),
                $"plg_{mutex}");
            FileTransferTempDirectory = Path.Combine(Environment.ExpandEnvironmentVariables(dataFolder),
                $"psh_{mutex}");
            PotentialCommandsDirectory = Path.Combine(Environment.ExpandEnvironmentVariables(dataFolder),
                $"ptc_{mutex}");
            StaticCommandPluginsDirectory = Path.Combine(Environment.ExpandEnvironmentVariables(dataFolder),
                $"stp_{mutex}");
            SendToServerPackages = Path.Combine(Environment.ExpandEnvironmentVariables(dataFolder),
                $"sts_{mutex}");
            LibrariesDirectory = Path.Combine(Environment.ExpandEnvironmentVariables(dataFolder),
                $"lib_{mutex}");
            ApplicationPath = Application.ExecutablePath;
        }

        public static string KeyLogFile { get; }
        public static string ExceptionFile { get; }
        public static string PluginsDirectory { get; }
        public static string ApplicationPath { get; }
        public static string FileTransferTempDirectory { get; }
        public static string PotentialCommandsDirectory { get; }
        public static string StaticCommandPluginsDirectory { get; }
        public static string SendToServerPackages { get; }
        public static string LibrariesDirectory { get; }

        string IPathInformation.ExceptionFile => ExceptionFile;
        string IPathInformation.PluginsDirectory => PluginsDirectory;
        string IPathInformation.ApplicationPath => ApplicationPath;
        string IPathInformation.FileTransferTempDirectory => FileTransferTempDirectory;
        string IPathInformation.PotentialCommandsDirectory => PotentialCommandsDirectory;
        string IPathInformation.StaticCommandPluginsDirectory => StaticCommandPluginsDirectory;
        string IPathInformation.SendToServerPackages => SendToServerPackages;
        string IPathInformation.KeyLogFile => KeyLogFile;
        string IPathInformation.LibrariesDirectory => LibrariesDirectory;
    }
}