using System;
using System.Collections.Generic;

namespace Orcus.Server.Core.Args
{
    public class MultipleClientsEventArgs : EventArgs
    {
        public MultipleClientsEventArgs(List<int> clients)
        {
            Clients = clients;
        }

        public List<int> Clients { get; }
    }
}