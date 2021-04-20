using System.Collections.Generic;
using Orcus.Plugins.PropertyGrid;

namespace Orcus.Plugins.ClientPlugin
{
    /// <summary>
    ///     A <see cref="ClientController" /> which implements <see cref="IProvideEditableProperties" />
    /// </summary>
    public abstract class ClientControllerProvideEditablePropertyGrid : ClientController, IProvideEditableProperties
    {
        protected ClientControllerProvideEditablePropertyGrid()
        {
            Properties = new List<IProperty>();
        }

        /// <summary>
        ///     The properties visible in the PropertyGrid
        /// </summary>
        public List<IProperty> Properties { get; }

        /// <summary>
        ///     Validate the properties
        /// </summary>
        /// <returns>Return if the values of the properties are valid</returns>
        public abstract InputValidationResult ValidateInput();
    }
}