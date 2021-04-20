using System;

namespace Orcus.Server.Core.Args
{
    public class ClientEventArgs : EventArgs
    {
        public ClientEventArgs(int clientId)
        {
            ClientId = clientId;
        }

        public int ClientId { get; }
    }
}