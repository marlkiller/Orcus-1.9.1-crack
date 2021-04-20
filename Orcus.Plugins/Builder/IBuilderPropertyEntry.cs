using Orcus.Shared.Core;

namespace Orcus.Plugins.Builder
{
    /// <summary>
    ///     A builder property entry which represents a <see cref="IBuilderProperty" /> and a
    ///     <see cref="IBuilderPropertyView" />
    /// </summary>
    public interface IBuilderPropertyEntry
    {
        /// <summary>
        ///     The builder property
        /// </summary>
        IBuilderProperty BuilderProperty { get; }

        /// <summary>
        ///     The view of the builder property
        /// </summary>
        IBuilderPropertyView BuilderPropertyView { get; }
    }
}