using System;
using Orcus.Shared.Core;

namespace Orcus.Plugins.Builder
{
    /// <summary>
    ///     The index of the <see cref="IBuilderPropertyView" /> in the builder
    /// </summary>
    public class BuilderPropertyIndex
    {
        private BuilderPropertyIndex(Type previousBuilderProperty)
        {
            PreviousBuilderProperty = previousBuilderProperty;
        }

        /// <summary>
        ///     The builder property which comes before this builder property
        /// </summary>
        public Type PreviousBuilderProperty { get; }

        /// <summary>
        ///     Just position the builder property at the end
        /// </summary>
        public static BuilderPropertyIndex None { get; } = null;

        /// <summary>
        ///     Position the builder property after another builder property
        /// </summary>
        /// <typeparam name="T">The type of the builder property which comes before</typeparam>
        /// <returns>Return the builder property index which represents the given index</returns>
        public static BuilderPropertyIndex AfterBuilderProperty<T>() where T : IBuilderProperty
        {
            return new BuilderPropertyIndex(typeof (T));
        }
    }
}