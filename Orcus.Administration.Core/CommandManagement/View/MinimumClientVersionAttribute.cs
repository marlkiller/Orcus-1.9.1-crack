using System;

namespace Orcus.Administration.Core.CommandManagement.View
{
    public class MinimumClientVersionAttribute : Attribute
    {
        public MinimumClientVersionAttribute(int minimumClientVersion)
        {
            MinimumClientVersion = minimumClientVersion;
        }

        public int MinimumClientVersion { get; }
    }
}