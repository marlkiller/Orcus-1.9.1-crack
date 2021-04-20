using System;
using System.Reflection;

namespace Orcus.Plugins.PropertyGrid
{
    /// <summary>
    ///     Represents a property in the PropertyGrid
    /// </summary>
    public interface IProperty
    {
        /// <summary>
        ///     The display name of the property
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     The actual name of the property of the class
        /// </summary>
        string PropertyName { get; }

        /// <summary>
        ///     The description of the property
        /// </summary>
        string Description { get; }

        /// <summary>
        ///     The category of the property
        /// </summary>
        string Category { get; }

        /// <summary>
        ///     The current value of the property
        /// </summary>
        object Value { get; set; }

        /// <summary>
        ///     Information about the property
        /// </summary>
        PropertyInfo PropertyInfo { get; }

        /// <summary>
        ///     The type of the property
        /// </summary>
        Type PropertyType { get; }
    }
}