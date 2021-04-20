using Orcus.Shared.Connection;

namespace Orcus.Server.Core.DynamicCommands
{
    public struct TargetedClient
    {
        public TargetedClient(Client client)
        {
            Id = client.Id;
            ClientInformation = client.GetOnlineClientInformation();
            Client = client;
            IsOnline = true;
        }

        public TargetedClient(ClientInformation clientInformation)
        {
            Id = clientInformation.Id;
            ClientInformation = clientInformation;
            Client = null;
            IsOnline = false;
        }

        public int Id { get; }
        public ClientInformation ClientInformation { get; }
        public Client Client { get; }
        public bool IsOnline { get; set; }
    }
}