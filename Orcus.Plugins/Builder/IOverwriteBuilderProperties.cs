using System.Collections.Generic;
using Orcus.Shared.Core;

namespace Orcus.Plugins.Builder
{
    /// <summary>
    ///     Can be implemented to disable specific builder properties and give them new values
    /// </summary>
    public interface IOverwriteBuilderProperties
    {
        /// <summary>
        ///     The builder properties to overwrite
        /// </summary>
        List<IBuilderProperty> OverwrittenSettings { get; }
    }
}