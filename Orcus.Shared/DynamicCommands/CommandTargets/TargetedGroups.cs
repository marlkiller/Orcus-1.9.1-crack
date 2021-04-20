using System;
using System.Collections.Generic;

namespace Orcus.Shared.DynamicCommands.CommandTargets
{
    /// <summary>
    ///     A <see cref="CommandTarget" /> which targets specific groups
    /// </summary>
    [Serializable]
    public class TargetedGroups : CommandTarget
    {
        internal TargetedGroups(List<string> groups)
        {
            Groups = groups;
        }

        //for xml deserialization
        private TargetedGroups()
        {
        }

        /// <summary>
        ///     The names of the groups
        /// </summary>
        public List<string> Groups { get; set; }
    }
}