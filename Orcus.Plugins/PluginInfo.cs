using System;

namespace Orcus.Plugins
{
    /// <summary>
    ///     Defines some information about the plugin
    /// </summary>
    public class PluginInfo
    {
        /// <summary>
        ///     The name of the plugin
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     A short description about what the plugin does
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     The name of the author
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        ///     The url to the author's page
        /// </summary>
        public string AuthorUrl { get; set; }

        /// <summary>
        ///     A unique id to identify the plugin
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        ///     The version of this plugin
        /// </summary>
        public PluginVersion Version { get; set; }

        /// <summary>
        ///     A thumbnail for plugin. The recommended size is 240 x 135 px
        /// </summary>
        public string Thumbnail { get; set; }

        /// <summary>
        ///     The type of the plugin
        /// </summary>
        public PluginType PluginType { get; set; }

        /// <summary>
        ///     The first library
        /// </summary>
        public string Library1 { get; set; }

        /// <summary>
        ///     The seconds library. This is always the payload for the client
        /// </summary>
        public string Library2 { get; set; }
    }
}