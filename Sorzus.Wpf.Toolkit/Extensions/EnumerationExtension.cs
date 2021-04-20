using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Sorzus.Wpf.Toolkit.Extensions
{
    /// <summary>
    ///     Fill an <see cref="ItemsControl" /> with the values of an Enum
    /// </summary>
    /// <example>
    /// <code>
    ///     &lt;ComboBox
    ///     ItemsSource="{Binding Source={my:Enumeration {x:Type my:Status}}}"
    ///     DisplayMemberPath="Description"
    ///     SelectedValue="{Binding CurrentStatus}"
    ///     SelectedValuePath="Value"  />
    /// </code>
    /// </example>
    public class EnumerationExtension : MarkupExtension
    {
        private Type _enumType;

        public EnumerationExtension(Type enumType)
        {
            if (enumType == null)
                throw new ArgumentNullException(nameof(enumType));

            EnumType = enumType;
        }

        public Type EnumType
        {
            get { return _enumType; }
            private set
            {
                if (_enumType == value)
                    return;

                var enumType = Nullable.GetUnderlyingType(value) ?? value;

                if (enumType.IsEnum == false)
                    throw new ArgumentException("Type must be an Enum.");

                _enumType = value;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var enumValues = Enum.GetValues(EnumType);

            return (
                from object enumValue in enumValues
                select new EnumerationMember
                {
                    Value = enumValue,
                    Description = GetDescription(enumValue)
                }).ToArray();
        }

        private string GetDescription(object enumValue)
        {
            var descriptionAttribute = EnumType
                .GetField(enumValue.ToString())
                .GetCustomAttributes(typeof (DescriptionAttribute), false)
                .FirstOrDefault() as DescriptionAttribute;


            return descriptionAttribute != null
                ? descriptionAttribute.Description
                : enumValue.ToString();
        }

        public class EnumerationMember
        {
            public string Description { get; set; }
            public object Value { get; set; }
        }
    }
}