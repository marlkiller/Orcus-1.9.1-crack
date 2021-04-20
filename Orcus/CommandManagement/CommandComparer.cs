using System.Collections.Generic;
using Orcus.Plugins;

namespace Orcus.CommandManagement
{
    public class CommandComparer : IEqualityComparer<Command>
    {
        public bool Equals(Command x, Command y)
        {
            return x.Identifier == y.Identifier;
        }

        public int GetHashCode(Command obj)
        {
            return (int) obj.Identifier;
        }
    }
}