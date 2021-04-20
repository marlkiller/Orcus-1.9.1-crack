using System;

namespace Orcus.Shared.Connection
{
    [Serializable]
    public class OfflineClientInformation : ClientInformation
    {
        public DateTime LastSeen { get; set; }
    }
}