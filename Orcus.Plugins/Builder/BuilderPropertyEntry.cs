using Orcus.Shared.Core;

namespace Orcus.Plugins.Builder
{
    /// <summary>
    ///     A simple class which implements <see cref="IBuilderPropertyEntry" />
    /// </summary>
    public class BuilderPropertyEntry : IBuilderPropertyEntry
    {
        /// <summary>
        ///     Initialize a new <see cref="BuilderPropertyEntry" />
        /// </summary>
        /// <param name="builderProperty">The builder property</param>
        /// <param name="builderPropertyView">The view of the builder property</param>
        public BuilderPropertyEntry(IBuilderProperty builderProperty, IBuilderPropertyView builderPropertyView)
        {
            BuilderProperty = builderProperty;
            BuilderPropertyView = builderPropertyView;
        }

        /// <summary>
        ///     The builder property
        /// </summary>
        public IBuilderProperty BuilderProperty { get; }

        /// <summary>
        ///     The view of the builder property
        /// </summary>
        public IBuilderPropertyView BuilderPropertyView { get; }
    }
}