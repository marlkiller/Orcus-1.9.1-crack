using System;

namespace Orcus.Server.Core.Database.FileSystem
{
    public class FilePushRequest
    {
        public Guid Guid { get; set; }
        public DateTime Timestamp { get; set; }
        public Client Client { get; set; }
    }
}