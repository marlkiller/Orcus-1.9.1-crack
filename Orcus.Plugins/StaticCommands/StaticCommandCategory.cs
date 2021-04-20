using System;

namespace Orcus.Plugins.StaticCommands
{
    /// <summary>
    ///     The category of a <see cref="StaticCommand" />
    /// </summary>
    [Serializable]
    public class StaticCommandCategory
    {
        /// <summary>
        ///     The static command does something for the client e. g. Uninstall, Kill, Reconnect, etc.
        /// </summary>
        public static StaticCommandCategory Client =
            new StaticCommandCategory(Resources.PluginsTranslation.StaticCommands_Category_Client);

        /// <summary>
        ///     The static command does something on the system e. g. change wallpaper, hide clock, etc.
        /// </summary>
        public static StaticCommandCategory System =
            new StaticCommandCategory(Resources.PluginsTranslation.StaticCommands_Category_System);

        /// <summary>
        ///     The static command executes/interactes with external software
        /// </summary>
        public static StaticCommandCategory ExternalSoftware =
            new StaticCommandCategory(Resources.PluginsTranslation.StaticCommands_Category_ExternalSoftware);

        /// <summary>
        ///     The static command does something which may considered funny
        /// </summary>
        public static StaticCommandCategory Fun =
            new StaticCommandCategory(Resources.PluginsTranslation.StaticCommands_Category_Fun);

        /// <summary>
        ///     The static command stress tests a server
        /// </summary>
        public static StaticCommandCategory StressTest =
            new StaticCommandCategory(Resources.PluginsTranslation.StaticCommands_Category_StressTest);

        /// <summary>
        ///     The static command does something with the computer (hardware) e. g. shutdown, restart, open CD drive, etc.
        /// </summary>
        public static StaticCommandCategory Computer =
            new StaticCommandCategory(Resources.PluginsTranslation.StaticCommands_Category_Computer);

        /// <summary>
        ///     The static command interacts with the user e. g. provides information, shows instructions, etc.
        /// </summary>
        public static StaticCommandCategory UserInteraction =
            new StaticCommandCategory(Resources.PluginsTranslation.StaticCommands_Category_UserInteraction);

        /// <summary>
        ///     Initialize a new <see cref="StaticCommandCategory" />
        /// </summary>
        /// <param name="name">The name of the category</param>
        public StaticCommandCategory(string name)
        {
            Name = name;
        }

        /// <summary>
        ///     The name of the category
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Create a new <see cref="StaticCommandCategory" />
        /// </summary>
        /// <param name="name">The name of the category</param>
        /// <returns>Returns a <see cref="StaticCommandCategory" /> with the <see cref="Name" /> as category name</returns>
        public static StaticCommandCategory Create(string name)
        {
            return new StaticCommandCategory(name);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}