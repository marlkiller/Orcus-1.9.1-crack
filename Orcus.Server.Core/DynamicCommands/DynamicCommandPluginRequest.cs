namespace Orcus.Server.Core.DynamicCommands
{
    public class DynamicCommandPluginRequest
    {
        public DynamicCommandPluginRequest(Client client, int resourceId)
        {
            Client = client;
            ResourceId = resourceId;
        }

        public Client Client { get; }
        public int ResourceId { get; }
    }
}