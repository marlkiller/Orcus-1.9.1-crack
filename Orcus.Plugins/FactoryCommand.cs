namespace Orcus.Plugins
{
    /// <summary>
    ///     A specialized command which works with a <see cref="PluginFactory" />
    /// </summary>
    public abstract class FactoryCommand : Command
    {
        /// <summary>
        ///     Initialize the command with predefined (<see cref="IFactoryClientCommand.FactoryCommandType" />) plugin factory
        /// </summary>
        /// <param name="factory">The plugin factory</param>
        public abstract void Initialize(PluginFactory factory);
    }
}