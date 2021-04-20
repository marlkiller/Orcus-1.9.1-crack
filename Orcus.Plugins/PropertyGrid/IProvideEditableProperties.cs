using System.Collections.Generic;

namespace Orcus.Plugins.PropertyGrid
{
    /// <summary>
    ///     The properties of the object can be edited
    /// </summary>
    public interface IProvideEditableProperties
    {
        /// <summary>
        ///     The properties to edit
        /// </summary>
        List<IProperty> Properties { get; }

        /// <summary>
        ///     Validates the values of the properties
        /// </summary>
        /// <returns></returns>
        InputValidationResult ValidateInput();
    }
}