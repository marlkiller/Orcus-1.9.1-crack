using System;
using System.Collections.Generic;
using System.Linq;
using Orcus.Shared.Core;

namespace Orcus.Plugins.PropertyGrid
{
    /// <summary>
    ///     Important methods to handle <see cref="IProvideEditableProperties" />
    /// </summary>
    public static class PropertyGridExtensions
    {
        /// <summary>
        ///     Convert a <see cref="IProperty" /> to a <see cref="PropertyNameValue" />
        /// </summary>
        /// <param name="property">The property to convert</param>
        /// <returns>The <see cref="PropertyNameValue" /> which represents the <see cref="IProperty" /></returns>
        public static PropertyNameValue ToPropertyNameValue(this IProperty property)
        {
            var propertyNameValue = new PropertyNameValue
            {
                Name = property.PropertyName,
                Value = property.Value
            };

            if (property.PropertyInfo.GetCustomAttributes(false).OfType<SerializeAsUtcAttribute>().Any())
            {
                var dateTime = property.Value as DateTime?;
                if (dateTime.HasValue)
                    propertyNameValue.Value = dateTime.Value.ToUniversalTime();
            }

            return propertyNameValue;
        }

        /// <summary>
        ///     Applies a list of <see cref="PropertyNameValue" /> on an object
        /// </summary>
        /// <param name="propertyObject">The object the properties should be applied to</param>
        /// <param name="propertyNameValues">The properties</param>
        public static void InitializeProperties(object propertyObject, List<PropertyNameValue> propertyNameValues)
        {
            var type = propertyObject.GetType();
            var properties = type.GetProperties();

            foreach (var propertyNameValue in propertyNameValues)
            {
                var property = properties.FirstOrDefault(x => x.Name == propertyNameValue.Name);
                if (property != null)
                {
                    object value = propertyNameValue.Value;
                    if (property.GetCustomAttributes(false).OfType<SerializeAsUtcAttribute>().Any())
                    {
                        var dateTime = propertyNameValue.Value as DateTime?;
                        if (dateTime.HasValue)
                            value = dateTime.Value.ToLocalTime();
                    }

                    property.SetValue(propertyObject, value, null);
                }
            }
        }
    }
}