using System.Collections.Generic;

namespace Orcus.Administration.ViewModels.Main
{
    public class StaticCommandGroup
    {
        public string Name { get; set; }
        public List<StaticCommandItem> StaticCommands { get; set; }
    }
}