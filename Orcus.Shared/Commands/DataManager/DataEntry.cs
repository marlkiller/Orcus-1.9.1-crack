using System;

namespace Orcus.Shared.Commands.DataManager
{
    [Serializable]
    public class DataEntry
    {
        public long Size { get; set; }
        public Guid DataType { get; set; }
        public int Id { get; set; }
        public string EntryName { get; set; }
        public int ClientId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}