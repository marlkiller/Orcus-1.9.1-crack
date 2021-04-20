using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Orcus.Plugins.PropertyGrid
{
    /// <summary>
    ///     Generic implementation of <see cref="IProperty" />
    /// </summary>
    /// <typeparam name="T">The type of the property</typeparam>
    public class Property<T> : IProperty
    {
        private readonly IProvideEditableProperties _provideEditableProperties;
        private readonly PropertyInfo _propertyInfo;

        /// <summary>
        ///     Initialize <see cref="Property{T}" />
        /// </summary>
        /// <param name="provideEditableProperties">The object which has the property</param>
        /// <param name="property">The property</param>
        /// <param name="name">The dispaly name of the property</param>
        /// <param name="description">The description of the property</param>
        /// <param name="category">The category of the property</param>
        public Property(IProvideEditableProperties provideEditableProperties, Expression<Func<T>> property, string name,
            string description, string category)
        {
            _provideEditableProperties = provideEditableProperties;
            Name = name;
            Description = description;
            Category = category;

            _propertyInfo = (PropertyInfo) ((MemberExpression) property.Body).Member;
        }

        /// <summary>
        ///     The current value of the property
        /// </summary>
        public T Value
        {
            get { return (T) _propertyInfo.GetValue(_provideEditableProperties, null); }
            set { _propertyInfo.SetValue(_provideEditableProperties, value, null); }
        }

        /// <summary>
        ///     The display name of the property
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     The description of the property
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     The category of the property
        /// </summary>
        public string Category { get; }

        object IProperty.Value
        {
            get { return Value; }
            set { Value = (T) value; }
        }

        PropertyInfo IProperty.PropertyInfo => _propertyInfo;
        Type IProperty.PropertyType => typeof (T);
        string IProperty.PropertyName => _propertyInfo.Name;
    }
}