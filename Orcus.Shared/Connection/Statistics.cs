using System;
using System.Collections.Generic;

namespace Orcus.Shared.Connection
{
    [Serializable]
    public class Statistics
    {
        public int ClientsOnline { get; set; }
        public int TotalClients { get; set; }
        public int ClientsOffline => TotalClients - ClientsOnline;
        public List<ClientCountStatistic<OSType>> OperatingSystems { get; set; }
        public int ClientsOnlineToday { get; set; }
        public List<ClientCountStatistic<string>> Languages { get; set; }
        public List<ClientCountStatistic<PermissionType>> Permissions { get; set; }
        public long DatabaseSize { get; set; }
        public DateTime UpSince { get; set; }
        public long UsedMemory { get; set; }
        public List<ClientCountStatistic<DateTime>> NewClientsConnected { get; set; }
        public List<ClientCountStatistic<DateTime>> ClientsConnected { get; set; }
    }

    [Serializable]
    public enum PermissionType : byte
    {
        Limited,
        Administrator,
        Service
    }

    [Serializable]
    public class ClientCountStatistic<T>
    {
        public ClientCountStatistic(T key, int clientsCount)
        {
            Key = key;
            ClientsCount = clientsCount;
        }

        public ClientCountStatistic()
        {
        }

        public T Key { get; set; }
        public int ClientsCount { get; set; }
    }
}