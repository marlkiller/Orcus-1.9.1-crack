using System;

namespace Orcus.Administration.Plugins
{
    /// <summary>
    ///     A plugin with a command and a view
    /// </summary>
    public interface ICommandAndViewPlugin : IViewPlugin
    {
        /// <summary>
        ///     The model
        /// </summary>
        Type Command { get; }
    }
}