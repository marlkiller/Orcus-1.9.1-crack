using System.Collections.Generic;
using Orcus.Administration.Plugins.BuildPlugin;
using Orcus.Plugins.Builder;
using Orcus.Plugins.PropertyGrid;
using Orcus.Shared.Core;

namespace Orcus.Administration.Plugins
{
    /// <summary>
    ///     Represents a build plugin which influences the creation of the client. Allowed settings implementations:
    ///     <see cref="IProvideBuilderSettings" />, <see cref="IProvideWindowSettings" /> or
    ///     <see cref="IProvideEditableProperties" />
    /// </summary>
    public abstract class BuildPluginBase
    {
        public List<IBuilderProperty> OverwrittenSettings { get; set; }

        /// <summary>
        ///     All properties in the builder were validated and is to define the output path
        /// </summary>
        /// <param name="builderArguments">Settings which can be changed to influence the building process</param>
        public abstract void Prepare(IBuilderArguments builderArguments);
    }
}