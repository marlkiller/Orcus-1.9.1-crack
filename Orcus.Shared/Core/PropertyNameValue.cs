using System;

namespace Orcus.Shared.Core
{
    /// <summary>
    ///     The object which gets serialized and represents a property
    /// </summary>
    [Serializable]
    public class PropertyNameValue
    {
        /// <summary>
        ///     The name of the property
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     The value of the property
        /// </summary>
        public object Value { get; set; }
    }
}