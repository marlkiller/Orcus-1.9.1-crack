using System.Collections.Generic;
using Orcus.Plugins.Builder;
using Orcus.Shared.Core;

namespace Orcus.Plugins.ClientPlugin
{
    /// <summary>
    ///     A <see cref="ClientController" /> which implements <see cref="IProvideBuilderSettings" />
    /// </summary>
    public abstract class ClientControllerBuilderSettings : ClientController, IProvideBuilderSettings
    {
        private List<IBuilderPropertyEntry> _builderSettings;

        /// <summary>
        ///     The builder settings
        /// </summary>
        public virtual List<IBuilderPropertyEntry> BuilderSettings
            => _builderSettings ?? (_builderSettings = GetBuilderSettings());

        /// <summary>
        ///     Get the builder settings
        /// </summary>
        /// <returns>Returns the builder settings</returns>
        protected abstract List<IBuilderPropertyEntry> GetBuilderSettings();

        /// <summary>
        ///     Initialize the <see cref="ClientController" /> with the builder properties in the client
        /// </summary>
        /// <param name="builderProperties"></param>
        public abstract void InitializeSettings(List<IBuilderProperty> builderProperties);
    }
}