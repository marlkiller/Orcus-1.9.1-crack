using System.Collections.Generic;

namespace Orcus.Plugins.Builder
{
    /// <summary>
    ///     The plugin provides builder property settings which will be added to the client builder
    /// </summary>
    public interface IProvideBuilderSettings
    {
        /// <summary>
        ///     The builder properties
        /// </summary>
        List<IBuilderPropertyEntry> BuilderSettings { get; }
    }
}