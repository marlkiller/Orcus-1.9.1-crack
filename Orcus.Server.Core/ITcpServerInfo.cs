using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Orcus.Server.Core.Database;
using Orcus.Server.Core.Database.FileSystem;
using Orcus.Server.Core.DynamicCommands;

namespace Orcus.Server.Core
{
    public interface ITcpServerInfo
    {
        ConcurrentDictionary<int, Client> Clients { get; }
        DynamicCommandManager DynamicCommandManager { get; }
        DateTime OnlineSince { get; }
        SortedDictionary<ushort, Administration> Administrations { get; }
        PushManager PushManager { get; }
        DatabaseManager DatabaseManager { get; }

        void ChangeGroup(List<int> clients, string newName);
    }
}