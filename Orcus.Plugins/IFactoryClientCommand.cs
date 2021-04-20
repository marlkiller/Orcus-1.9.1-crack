using System;

namespace Orcus.Plugins
{
    /// <summary>
    ///     Root interface for a factory client command plugin
    /// </summary>
    public interface IFactoryClientCommand
    {
        /// <summary>
        ///     The factory client command which will be initialized with the <see cref="Factory" />. Must inherit
        ///     <see cref="FactoryCommand" />
        /// </summary>
        Type FactoryCommandType { get; }

        /// <summary>
        ///     The factory for the factory command
        /// </summary>
        PluginFactory Factory { get; }
    }
}