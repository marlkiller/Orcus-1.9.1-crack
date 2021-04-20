namespace Orcus.Plugins
{
    /// <summary>
    ///     Defines how the plugin should be handeled
    /// </summary>
    public enum PluginType
    {
        /// <summary>
        ///     The plugin contains audio. The plugin has to implement the IAudioPlugin interface
        /// </summary>
        Audio,

        /// <summary>
        ///     The plugin is called after the client is built. The plugin has to implement the IBuildPlugin interface
        /// </summary>
        Build,

        /// <summary>
        ///     The plugin gets injected into the client and called on startup. The plugin has to inherit the
        ///     <see cref="ClientController" /> class
        /// </summary>
        Client,

        /// <summary>
        ///     The plugin contains a view and a command for the administration and a command for the client. The plugin has to
        ///     implement the ICommandAndViewPlugin interface
        /// </summary>
        CommandView,

        /// <summary>
        ///     The plugin only contains a view. The plugin has to implement the IViewPlugin interface
        /// </summary>
        View,

        /// <summary>
        ///     The plugin adds menu items to the main menu bar and context menu of clients. The plugin has to implement the
        ///     IAdministrationPlugin interface
        /// </summary>
        Administration,

        /// <summary>
        ///     A command view plugin but the command plugin on the client's side is loaded at startup
        /// </summary>
        CommandFactory,

        /// <summary>
        ///     A simple command which can be used in Crowd Control
        /// </summary>
        StaticCommand
    }
}