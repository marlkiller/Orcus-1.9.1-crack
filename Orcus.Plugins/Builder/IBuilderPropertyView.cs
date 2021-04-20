using System;
using System.Collections.Generic;
using Orcus.Plugins.PropertyGrid;
using Orcus.Shared.Core;

namespace Orcus.Plugins.Builder
{
    /// <summary>
    ///     The view for <see cref="IBuilderProperty" />
    /// </summary>
    public interface IBuilderPropertyView
    {
        /// <summary>
        ///     The location in the client builder
        /// </summary>
        BuilderPropertyPosition PropertyPosition { get; }

        /// <summary>
        ///     Strings which represent this builder property
        /// </summary>
        string[] Tags { get; }

        /// <summary>
        ///     The type of the <see cref="IBuilderProperty" />
        /// </summary>
        Type BuilderProperty { get; }

        /// <summary>
        ///     Validate the user inputs
        /// </summary>
        /// <param name="currentBuilderProperties">All builder properties</param>
        /// <param name="builderProperty">The builder property to validate</param>
        /// <returns>The validation result based on the current values of the <see cref="BuilderProperty" /></returns>
        InputValidationResult ValidateInput(List<IBuilderProperty> currentBuilderProperties,
            IBuilderProperty builderProperty);
    }
}