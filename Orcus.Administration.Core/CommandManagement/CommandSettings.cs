using System.Collections.Generic;
using Orcus.Shared.Connection;

namespace Orcus.Administration.Core.CommandManagement
{
    public class CommandSettings
    {
        public bool AllowMultipleThreads { get; set; }
        public List<PortableLibrary> Libraries { get; set; }
    }
}